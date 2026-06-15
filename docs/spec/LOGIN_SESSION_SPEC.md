# Login Session Spec

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerLogin.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/Player.h`
- `external/gs2lib/include/IEnums.h`
- `external/gs2lib/src/IUtil.cpp`
- `external/gs2lib/src/CFileQueue.cpp`

## Confirmed Receive Boundary

`Player::doMain` processes socket bytes as raw big-endian length-prefixed frames:

1. Read a raw two-byte big-endian length.
2. If the full frame is not buffered, stop and wait for more bytes.
3. Read exactly that many bytes.
4. Decompress/decrypt the frame according to the current inbound encryption generation.
5. Pass the resulting inner payload to `Player::parsePacket`.

The first inner packet is special because `m_type == PLTYPE_AWAIT`. It is read to `\n`, counted, and passed to `msgPLI_LOGIN`. It is not gen3 per-packet decrypted before login parsing.

## Confirmed Login Packet Fields

`msgPLI_LOGIN` reads:

1. `GCHAR` client type exponent, converted with `m_type = 1 << value`.
2. For `PLTYPE_CLIENT`, exactly 8 bytes are first read as a legacy version token. If unknown, the C++ code switches inbound generation to gen3 and rewinds to byte offset 1.
3. For non-web clients, new clients, or RC2, read one `GCHAR` encryption iterator key when the C++ condition matches.
4. Read exactly 8 bytes as the version token when `m_versionId` is still unknown.
5. Read account as `GCHAR length` plus raw bytes.
6. Read password as `GCHAR length` plus raw bytes.
7. Read the remaining bytes as identity text.

The first identity token before `,` becomes the platform string when identity is not empty.

## Confirmed Type To Encryption Mapping

- `PLTYPE_CLIENT`: starts gen2; may fall back to gen3 if the first 8-byte version token is unknown.
- `PLTYPE_RC`: gen3.
- `PLTYPE_NPCSERVER`: gen3.
- `PLTYPE_NC`: gen3.
- `PLTYPE_CLIENT2`: gen4.
- `PLTYPE_CLIENT3`: gen5.
- `PLTYPE_RC2`: gen5 and reads the key before version.
- `PLTYPE_WEB`: gen1 and configures outbound file queue gen1.

## Confirmed Rejection Boundary

For an unknown client type, C++ sends:

```txt
PLO_DISCMESSAGE + "Your client type is unknown.  Please inform the OpenGraal Team.  Type: {m_type}."
```

`Player::sendPacket` appends `\n` when the packet does not already end in newline.

## Confirmed Success Boundary

After parsing version/account/password/identity, C++ checks server capacity, IP ban, allowed versions, and login-server connectivity. It then sends credentials to the server list with `sendLoginPacketForPlayer`. Full success continues later through server-list/account validation and `Player::sendLogin`, which enters gameplay/world setup. That is intentionally not implemented in C# yet.
