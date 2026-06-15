# Login And Session Specification

Confirmed sources:

- `server/src/player/Player.cpp`
- `server/src/player/PlayerLogin.cpp`
- `server/include/Player.h`
- `external/gs2lib/src/CFileQueue.cpp`
- `external/gs2lib/src/IUtil.cpp`

Receive flow:

1. `Player::onRecv` reads up to 0x8000 bytes from `CSocket::getData` into `m_recvBuffer`.
2. `Player::doMain` processes `m_recvBuffer` while it has more than one byte, reading raw big-endian short length-prefixed frames.
3. Each full frame is decompressed/decrypted according to inbound generation, then passed to `Player::parsePacket`.
4. `Player::parsePacket` treats `PLTYPE_AWAIT` specially: first packet is read to newline and passed to `msgPLI_LOGIN`.
5. Later packets are newline-delimited unless `m_nextIsRaw` is set.
6. `PLI_RAWDATA` sets `m_nextIsRaw = true` and `m_rawPacketSize = pPacket.readGUInt()`.
7. The next packet is read by exact byte count, and for clients or RC versions greater than 1.1 a trailing newline is stripped if present.
8. Gen3 decrypts individual packets after newline/raw splitting and before reading packet ID.
9. Packet ID is read with `readGUChar`, then dispatched through `TPLFunc[id]`.
10. `m_packetCount` increments once for initial login and once for each parsed packet.

Login prelude:

- The first field is a Graal char exponent: `m_type = 1 << pPacket.readGChar()`.
- Type determines inbound encryption generation:
  - `PLTYPE_CLIENT`: gen2.
  - `PLTYPE_RC`: gen3.
  - `PLTYPE_NPCSERVER`: gen3.
  - `PLTYPE_NC`: gen3.
  - `PLTYPE_CLIENT2`: gen4.
  - `PLTYPE_CLIENT3`: gen5.
  - `PLTYPE_RC2`: gen5 and reads encryption key before version.
  - `PLTYPE_WEB`: gen1 and file queue gen1.
- Unknown types send `PLO_DISCMESSAGE` with the original OpenGraal wording and abort login.

Outbound confirmed login packets:

- `PLO_SIGNATURE` packet is packet ID 25 plus `GCHAR 73`. Raw bytes before send-queue newline/compression are `[57, 105]`.
- `PLO_DISCMESSAGE` is packet ID 16 followed by raw text.
- Full successful login is not implemented because account validation, version handling, world loading, player prop emission, and file queue flushing must be recovered in more detail first.

Current C# milestone:

- Parses confirmed login packet fields through version/account/password/identity.
- Implements the confirmed login version-token catalog needed at the session boundary.
- Implements unknown login type rejection bytes including `Player::sendPacket` newline append.
- Implements raw big-endian length frame extraction and source-confirmed raw-data newline stripping option.
