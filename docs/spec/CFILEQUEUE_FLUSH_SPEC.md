# CFileQueue Flush Specification

Authoritative sources:

- `external/gs2lib/include/CFileQueue.h`
- `external/gs2lib/src/CFileQueue.cpp`
- `external/gs2lib/include/CSocket.h`
- `external/gs2lib/src/CSocket.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/ServerList.cpp`

## Player::sendPacket Boundary

`Player::sendPacket(CString pPacket, bool appendNL = true)`:

1. Returns immediately for an empty packet.
2. If `appendNL` is true and the packet does not already end with `\n`, appends
   `\n`.
3. Passes the packet to `m_fileQueue.addPacket(pPacket)`.

This means packet builders normally produce packet bodies, not socket bytes.
`CFileQueue` owns queueing, compression/encryption framing, websocket wrapping,
partial socket retry buffering, and file/normal ordering.

## addPacket

`CFileQueue::addPacket` consumes one or more newline-delimited packets:

- If the next unread packet ID byte is `< 0x20`, parsing stops.
- `PLO_RAWDATA` is special: the queue reads the raw-data header through newline,
  stores it in `pack100`, sets `prev100`, and reads `size100` from the header.
- The next packet after a raw-data header is read as exactly `size100` bytes.
  If that raw packet begins with `PLO_BOARDPACKET`, `pack100 + packet` goes to
  `normalBuffer`; otherwise it goes to `fileBuffer`.
- `PLO_LARGEFILESTART`, `PLO_LARGEFILEEND`, and `PLO_LARGEFILESIZE` are queued
  in `fileBuffer`.
- Other normal packets go to `normalBuffer`.
- For `ENCRYPT_GEN_6`, non-raw parsing takes the remaining `pPacket` instead of
  reading to newline.

## sendCompress Queue Selection

Confirmed thresholds:

- A front normal packet larger than `0xF000` is sent alone.
- A file packet is forced when `bytesSentWithoutFile > 0x7FFF`,
  `forceSendFiles` is true, or `sendCallsWithoutData >= 4`.
- Normal packets are accumulated while total length is `< 0xC000`, but the next
  packet is skipped if it would push the send buffer over `0xF000`.
- If the send buffer is `< 0x4000`, one file packet may be appended if total
  length remains `<= 0xF000`.
- Empty sends increment `sendCallsWithoutData` up to 5.

## Compression / Encryption Generations

- `ENCRYPT_GEN_1` and `ENCRYPT_GEN_6`: append `pSend` directly to `oBuffer` and
  call `sendData`.
- `ENCRYPT_GEN_2` and `ENCRYPT_GEN_3`: zlib-compress `pSend`, require length
  `<= 0xFFFD`, prefix raw big-endian short length, then send.
- `ENCRYPT_GEN_4`: bzip2-compress, require length `<= 0xFFFD`, set encryption
  limit for BZ2, encrypt, prefix raw big-endian short length, then send.
- `ENCRYPT_GEN_5`: choose uncompressed for length `<= 55`, zlib for `> 55`, BZ2
  for `> 0x2000`; require encrypted payload length `<= 0xFFFC`; prefix raw
  big-endian short `(encryptedLength + 1)`, then raw compression type byte, then
  encrypted payload.

Confirmed compression type constants from `CFileQueue.cpp` / `CEncryption.cpp`:

- `COMPRESS_UNCOMPRESSED = 0x02`
- `COMPRESS_ZLIB = 0x04`
- `COMPRESS_BZ2 = 0x06`

Confirmed gen5 threshold behavior:

- payload length `<= 55`: compression type `0x02`, no zlib/bzip2 compression
- payload length `> 55`: zlib path
- payload length `> 0x2000`: bzip2 path

Confirmed gen5 encryption limit behavior:

- compression type `0x02`: limit `0x0C`
- compression type `0x04`: limit `0x04`
- compression type `0x06`: limit `0x04`

## Socket Semantics

`CSocket::sendData` calls nonblocking `send`. On `EAGAIN`, it returns 0 without
disconnecting. On connection-loss errors it disconnects. It subtracts sent bytes
from `*dsize`; `CFileQueue` removes exactly the returned sent byte count from
`oBuffer`, so unsent bytes remain queued for the next flush.

## C# Boundary

The C# `GraalFileQueue` currently implements these source-confirmed flush
paths:

- normal newline packet splitting
- `PLO_RAWDATA` length transition
- `PLO_BOARDPACKET` raw-data routing to the normal queue
- file-buffer routing for non-board raw data and large-file packets
- queue selection thresholds used before compression
- output buffering across partial sends
- socket-level passthrough for `ENCRYPT_GEN_1`/`ENCRYPT_GEN_6`
- socket-level gen5 uncompressed payload framing for payloads `<= 55`
- gen5 uncompressed compression type byte `0x02`
- gen5 big-endian socket length prefix equal to `encryptedLength + 1`
- gen5 iterator-XOR encryption using the recovered `CEncryption` behavior
- explicit unsupported exceptions for gen2/gen3/gen4 and gen5 compressed
  payloads until zlib/bzip2 bytes are independently proven

Production compressed flush behavior remains blocked until zlib/bzip2 fixtures
are byte-exact. The implemented gen5 path is intentionally limited to the
source-confirmed uncompressed branch.

## Current Pass Status

This pass implemented the first socket-level flush boundary that does not
require unproven compression output:

- gen1/gen6 socket flush emits the queued bytes directly.
- gen5 payloads up to 55 bytes are framed as:
  `GSHORT(encryptedLength + 1) + compressionType + encryptedPayload`.
- partial socket writes leave remaining framed bytes buffered for the next
  flush, matching the `oBuffer` / `sendData` retry model in `CFileQueue`.
- unsupported compressed branches throw before consuming the pending diagnostic
  queue bytes, so future compression implementation can resume from the same
  payload.

The C# boundary still queues normal newline packets, `PLO_RAWDATA` headers,
pre-serialized board/layer payload bytes, dynamic level packets, and first
post-dynamic runtime packets in the same order C++ calls `Player::sendPacket`.

Still blocked:

- gen2/gen3 zlib socket-level flush bytes
- gen4 bzip2 + encryption socket-level flush bytes
- gen5 zlib/bzip2 socket-level flush bytes for payloads over 55 bytes
- websocket wrapping
- production file transfer through `PLO_FILE`
- level resource transfer beyond pre-serialized board/layer and runtime payloads
