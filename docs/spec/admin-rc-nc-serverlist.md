# Admin, RC, NC, And Server List Specification

Confirmed packet families:

- `IEnums.h` defines extensive `PLI_RC_*`, `PLO_RC_*`, `PLI_NC_*`, and `PLO_NC_*` IDs.
- `Player::createFunctions` maps RC packets to handlers in `Player.cpp`, `PlayerRC.cpp`, and NC handlers under `V8NPCSERVER`.
- `ServerList.cpp` uses `ServerToListServer` and `ListServerToServer` enums from `IEnums.h`.

Confirmed behavior:

- RC/NC login uses the same first-packet prelude but enforces staff/admin IP checks before full login.
- RC login sends `PLO_CLEARWEAPONS`, `rcmessage.txt` lines as `PLO_RC_CHAT`, `PLO_UNKNOWN190`, and broadcasts `New RC: account`.
- NC login sends database NPCs and classes, then announces `New NC: account`.
- Client login registers the player with the list server through `Server::playerLoggedIn`.

Unknown/risky:

- Exact RC file-browser upload/download ordering needs golden tests.
- Server-list account/profile/buddy/PM behavior needs a dedicated pass through `ServerList.cpp`.
- NC behavior depends on `V8NPCSERVER` and exact `gs2compiler` source commit.
