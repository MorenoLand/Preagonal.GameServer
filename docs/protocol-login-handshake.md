# Protocol Login Handshake

This document captures the original C++ login/session behavior before gameplay implementation.

## Source Files

- `server/include/network/IPacketHandler.h`
- `server/src/player/PlayerLogin.cpp`
- `server/src/player/PlayerClient.cpp`
- `server/src/player/PlayerRC.cpp`
- `server/src/player/PlayerNC.cpp`
- `server/src/player/Player.cpp`
- `server/src/ServerList.cpp`

## Initial Packet Handling

`IPacketHandler::processBuffer` reads packet bundles using a two-byte raw length prefix. If `PacketCount == 0`, the bundle is parsed as a login packet rather than normal newline-delimited opcodes. The login packet is read up to newline and passed to `handlePacket(std::nullopt, packet)`.

`PlayerLogin::msgLoginPacket` reads the first byte as a GChar and computes:

```txt
sessionTypeMask = 1 << firstByteAsGChar
```

It then creates one of:

- `PlayerClientOriginal` for `PLTYPE_CLIENT`.
- `PlayerClient` for other client types.
- `PlayerRC` for RC types.
- `PlayerNC` for NC types.
- `PlayerNPCServer` for NPC-server type.

Unknown masks log `New login, but unknown player type: {m_type}` and fail the login.

The temporary `PlayerLogin` copies packet state to the new player, sets `PacketCount = 1`, swaps itself out of the server player list, rewinds the packet read cursor to 0, and calls the new player object's `handleLogin`.

## Game Client Login Parsing

`PlayerClient::handleLogin`:

1. Stores socket remote IP in account state.
2. Reads the session type byte again with `readGChar` and computes the same bit mask.
3. Selects encryption:
   - `PLTYPE_CLIENT`: generation 2.
   - `PLTYPE_CLIENT2`: generation 4.
   - `PLTYPE_CLIENT3`: generation 5.
   - `PLTYPE_WEB`: generation 1 and file queue generation 1.
4. For `PLTYPE_CLIENT`, reads an 8-byte version string. If version is unknown, it switches to generation 3 and rewinds packet read position to byte 1.
5. If version is still unknown, reads an encryption key as GChar, resets encryption, sets file queue codec for generations above 3, then reads an 8-byte version string.
6. Reads account as `readChars(readGUChar())`.
7. Reads password as `readChars(readGUChar())`.
8. Reads identity as the remaining string.
9. Checks allowed client versions.
10. Checks max player capacity.
11. Requires the server-list connection.
12. Sends the verification request to the server list.

Failure packets use `PLO_DISCMESSAGE` with exact strings from C++; these strings must be preserved.

## RC Login Parsing

`PlayerRC::handleLogin`:

1. Reads the type byte again.
2. Selects encryption:
   - `PLTYPE_RC`: generation 2.
   - `PLTYPE_NC`: generation 2.
   - `PLTYPE_RC2`: generation 5.
3. If generation is above 3, reads an encryption key, resets encryption, and sets file queue codec.
4. Reads 8-byte version.
5. Reads account, password, and remaining identity using the same string pattern as game clients.
6. Checks max player capacity.
7. Requires server-list connection.
8. Sends server-list login verification.

## NC Login Parsing

`PlayerNC::handleLogin` currently accepts only `PLTYPE_NC`, uses generation 2, then reads version/account/password/identity using the same pattern. It performs the same capacity and server-list checks.

## Server-List Verification

`ServerList::sendLoginPacketForPlayer` sends `SVO_VERIACC2` with:

1. account length as one byte and account bytes
2. password length as one byte and password bytes
3. player id as short
4. player type as one byte
5. identity length as short
6. identity bytes

`ServerList::msgSVI_VERIACC2` receives:

1. account length as GChar/`readGUChar`
2. account bytes
3. player id as `readGUShort`
4. type as `readGUChar`
5. message as remaining string

If message is not `SUCCESS`, the player receives `PLO_DISCMESSAGE` with that message, account saving is suppressed, and the player disconnects. If message is `SUCCESS`, `player->sendLogin()` performs account loading and initial login packet sends.

## Initial Successful Login Sends

`Player::sendLogin` sends at least:

- `PLO_SIGNATURE` with byte value `73`.
- For login server names: `PLO_DISABLECLASSICMODE` and `PLO_GHOSTICON` with byte `1`.
- For game clients when NPC server exists: `PLO_HASNPCSERVER`.
- For game clients: `PLO_UNKNOWN168`.

`PlayerClient::sendLogin` then sends client properties, flags, weapons, server-list state, map/minimap/RPG/start-message packets, and additional gameplay/world packets. These are not implemented yet.

## Confirmed But Not Yet Implemented

- The first-byte login type shift rule is implemented as a C# helper and tested.
- The full login flow is not implemented because packet numeric IDs, exact bundle framing byte order, encryption, and file queue behavior are still not fully recovered.
