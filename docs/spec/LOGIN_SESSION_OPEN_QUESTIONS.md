# Login Session Open Questions

- Exact server-list response flow that turns `sendLoginPacketForPlayer` into `Player::sendLogin` needs a dedicated pass through `ServerList.cpp`.
- Account file loading, password verification, guest behavior, default account fallback, ban fields, and admin IP checks are not implemented.
- Allowed-version settings parsing is documented at a high level but not yet ported into a C# compatibility service.
- Full outbound `CFileQueue` compression, socket flushing, file-buffer prioritization, and websocket wrapping are not implemented.
- Login success world-entry packets remain blocked on player property, level, map, file, and warp recovery.
- `PLI_BUNDLE` is confirmed numerically, but login/session dispatch behavior is not confirmed from `Player::createFunctions`.
