# TCP Session Pipeline Spec

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `external/gs2lib/src/CSocket.cpp`
- `external/gs2lib/src/CFileQueue.cpp`

## Confirmed C++ Receive Behavior

`Server::onRecv` accepts a TCP socket, constructs a `Player`, adds it to the
server player list, and registers it with the socket manager.

`Player::doMain` appends incoming socket bytes to `m_recvBuffer`, then reads
raw two-byte big-endian length-prefixed frames. If the full frame is not
buffered, it waits for more data. Once a frame is complete, it decompresses or
decrypts according to inbound encryption generation and calls
`Player::parsePacket`.

The first packet while `m_type == PLTYPE_AWAIT` is special: `parsePacket` reads
one newline-delimited login packet and calls `msgPLI_LOGIN` before normal
packet dispatch.

## Dev-Only C# Shell

Implemented:

- `DevOnlyLocalSessionPipeline`
  - reads source-confirmed length-prefixed frames through `PacketFramer`
  - parses the first login packet through `ClientSessionSkeleton`
  - runs existing pre-world auth checks
  - injects a clearly dev-only server-list success response only when
    `EnableDevOnlyAuth=true`
  - enters `PlayerSendLoginContinuation`, `PostLoginWorldEntryBoundary`,
    `WarpWorldEntryBoundary`, `NwLevelFileLoader`, and `SendLevelBoundary`
  - stops at `DynamicLevelPayloadSent` before live world simulation
- `DevOnlyLocalTcpServer`
  - accepts one TCP client at a time
  - reads exactly one length-prefixed frame without waiting for EOF
  - writes the uncompressed queued outbound bytes from `GraalFileQueue`
  - does not yet use production gen5 socket flush for the full login/level
    response because typical level payloads can exceed the gen5 zlib threshold
    and require blocked bzip2 output

This is a diagnostic shell, not a production session loop.

## Socket Flush Boundary

`GraalFileQueue.FlushSocket` now covers the source-confirmed socket-level paths
that do not depend on unverified compression output:

- `ENCRYPT_GEN_1` and `ENCRYPT_GEN_6`: queued bytes are emitted directly.
- `ENCRYPT_GEN_2` and `ENCRYPT_GEN_3`: queued bytes are zlib-compressed and
  prefixed by raw big-endian compressed length.
- `ENCRYPT_GEN_5` with payload length `<= 55`: emits big-endian length,
  compression type `0x02`, and iterator-XOR encrypted payload bytes.
- `ENCRYPT_GEN_5` with payload length `56..0x2000`: emits big-endian length,
  compression type `0x04`, and iterator-XOR encrypted zlib payload bytes.
- Partial socket writes preserve remaining framed bytes for the next flush.

The dev-only TCP shell intentionally remains on uncompressed diagnostic queue
bytes until full login/level response bzip2 handling is implemented.

## Known Gaps

- The TCP shell processes one login frame and then closes the connection.
- Outbound bzip2 socket framing for gen4 and gen5 payloads over `0x2000` bytes
  is still blocked.
- Websocket wrapping is not implemented.
- Continuous packet streaming, movement, reconnect cleanup, and multi-session
  forwarding are not implemented.
