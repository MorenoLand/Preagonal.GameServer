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
    response because that response exceeds the confirmed uncompressed gen5
    threshold and would require blocked zlib/bzip2 output

This is a diagnostic shell, not a production session loop.

## Socket Flush Boundary

`GraalFileQueue.FlushSocket` now covers the source-confirmed socket-level paths
that do not depend on unverified compression output:

- `ENCRYPT_GEN_1` and `ENCRYPT_GEN_6`: queued bytes are emitted directly.
- `ENCRYPT_GEN_5` with payload length `<= 55`: emits big-endian length,
  compression type `0x02`, and iterator-XOR encrypted payload bytes.
- Partial socket writes preserve remaining framed bytes for the next flush.

The dev-only TCP shell intentionally remains on uncompressed diagnostic queue
bytes until full login/level response compression is proven byte-exact.

## Known Gaps

- The TCP shell processes one login frame and then closes the connection.
- Outbound compressed socket framing for gen2/gen3/gen4 and gen5 payloads over
  55 bytes is still blocked on `CFileQueue::sendCompress` fixtures.
- Websocket wrapping is not implemented.
- Continuous packet streaming, movement, reconnect cleanup, and multi-session
  forwarding are not implemented.
