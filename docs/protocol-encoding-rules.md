# Protocol Encoding Rules

This document records C++-confirmed protocol encoding behavior and open gaps.

## Packet Bundle Framing

Source: `server/include/network/IPacketHandler.h`.

Inbound client data is read from a socket into a receive buffer. `IPacketHandler::retrievePacketBundle` reads a two-byte raw length with `CString::readShort()`, then reads that many bytes as a packet bundle and removes `length + 2` bytes from the receive buffer.

The byte order of `CString::readShort()` is not confirmed because `CString.h` is absent. The C# port must not finalize bundle prefix byte order until `CString.h`, `CFileQueue.h`, or byte captures confirm it.

## Packet Delimiting

Source: `IPacketHandler::parsePacketsFromBundle` and `Player::sendPacket`.

- Normal packets inside a bundle are read with `readString("\n")`.
- Outbound player packets append `'\n'` unless `appendNL` is false or the packet already ends in newline.
- Empty outbound packets are ignored.
- Server-list outbound packets also append `'\n'` when missing.
- Login packet parsing is special: before normal packet count exists, the bundle is read as a login packet with `readString("\n")` and passed to `handlePacket(std::nullopt, packet)`.

## Raw Data

Source: `IPacketHandler::parsePacketsFromBundle` and `Player::sendFile`.

- When a packet ID equals `PLI_RAWDATA`, the handler reads a `GUInt` from the packet and marks the next packet as raw.
- The next packet is read by exact byte count instead of newline delimiting.
- `RemoveNewlinesFromRawPacket` can remove a trailing newline from the raw packet.
- File sending uses `PLO_RAWDATA` to announce the following `PLO_FILE` packet byte size.
- Older client versions may receive file packets without mod time and with different raw-data length calculation.

`PLI_RAWDATA` and `PLO_RAWDATA` numeric IDs remain unknown.

## Graal Printable Integer Encoding

Directly confirmed:

- Packet IDs are read with `readGUChar()`.
- Packet logging reconstructs output packet IDs as `static_cast<uint8_t>(pPacket[0]) - 32`, confirming the printable-byte offset for GChar packet IDs.
- `PlayerLogin::msgLoginPacket` computes `m_type = (1 << pPacket.readGChar())`, so the login type byte is a GChar index.
- Many packet fields use `readGUChar`, `readGChar`, `readGUShort`, `readGUInt`, and `readGUInt5`.

Still requires `CString.h` confirmation:

- Exact signed behavior and clamp behavior of `writeGShort`, `writeGInt`, `writeGInt4`, `writeGUInt5`.
- Exact byte order and signed behavior of raw `readShort`.
- Whether `operator >> (short)`, `operator >> (int)`, and `operator >> (long long)` always map to the same `writeG*` widths assumed by current C# tests.

## String Encoding

Confirmed usage:

- Fixed-width version strings are read with `readChars(8)`.
- Account/password-style strings are often encoded as GChar length followed by raw bytes: `readChars(readGUChar())`.
- Some server-list and RC fields use raw `short` length followed by raw bytes.
- Remaining packet tail strings are often read with `readString("")`.

Still unknown:

- Exact text encoding of `CString` bytes. Current C# uses Latin-1 as a byte-preserving single-byte encoding, but this remains a compatibility assumption until `CString.h` is recovered.
- Exact behavior of `readString(separator)` when the separator is missing.
- Exact behavior of tokenization helpers such as `gtokenize`, `guntokenize`, CSV parsing, and newline replacement.

## Encryption And Compression

Source: `IPacketHandler::processPacketBundle`, `PlayerClient::handleLogin`, `PlayerRC::handleLogin`, `PlayerNC::handleLogin`, `ServerList::connectServer`.

- Generation 1: no encryption or compression for inbound bundles.
- Generation 2: zlib-compressed bundle, no encryption.
- Generation 3: zlib-compressed bundle; individual packets are decrypted after splitting.
- Generation 4: bundle is BZ2-compressed and encrypted; decryption occurs before BZ2 decompression.
- Generation 5 and later: first byte is compression type; remove it, limit encryption by compression type, decrypt, then decompress zlib/BZ2 or leave uncompressed.
- Client login chooses generations based on `PLTYPE_*`.
- Server-list registration sends one packet with file queue codec generation 1, then switches to generation 2.

Missing definitions:

- `ENCRYPT_GEN_*` numeric values.
- `COMPRESS_ZLIB`, `COMPRESS_BZ2`, `COMPRESS_UNCOMPRESSED` numeric values.
- Encryption algorithm and key schedule.
- `CFileQueue` compression and output bundle behavior.

## C# Tests Added So Far

- GChar offset and round-trip tests.
- GShort/GInt/GInt4/GUInt5 round-trip tests based on current C++ call-site interpretation.
- Packet newline framing tests from `Player::sendPacket`.
- Login type byte to bit-mask test from `PlayerLogin::msgLoginPacket`.

The integer-width tests are useful guards for the current implementation, but the non-GChar primitives must be revalidated against `CString.h` before production protocol work.
