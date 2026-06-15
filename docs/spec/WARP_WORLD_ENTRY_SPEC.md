# Warp / World Entry Boundary Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerLogin.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `external/gs2lib/include/IEnums.h`

## Entry Point From Login

The previous milestone stops at `ReadyForLevelWarp`, immediately before:

```cpp
warp(m_levelName, getX(), getY());
```

This call is in `Player::sendLoginClient`. It starts real level/map runtime and
must not be implemented until level loading, cache behavior, NPC packets, board
packets, and resource/file transfer are fully traced.

## Player::warp

Confirmed sequence:

1. Read settings for `unstickmelevel`, `unstickmex`, and `unstickmey` defaults
   (`onlinestartlocal.nw`, `30.0`, `35.0`).
2. Resolve target level with `Level::findLevel`.
3. If already in the same level, only update `PLPROP_X` and `PLPROP_Y` through
   `setProps(..., PLSETPROPS_FORWARD | PLSETPROPS_FORWARDSELF)` and return.
4. Call `leaveLevel()`.
5. Reset and possibly set `m_pmap` from target level.
6. Set player X/Y to requested values.
7. Call `setLevel(pLevelName, modTime)`.
8. On failure, try previous level; if that fails, try unstickme level; if that
   fails, return false.

## Player::setLevel First Packets

`setLevel` resolves the level again. If not found, it sends:

```cpp
PLO_WARPFAILED + pLevelName
```

and returns false.

After a level is found, `setLevel` may clone singleplayer/group-map levels,
adds the player to the level, and updates `m_levelName`.

If `modTime == 0 || m_versionId < CLVER_2_1`:

- GMAP modern clients receive `PLO_PLAYERWARP2` with packed X/Y/Z, map X/Y, and
  map name.
- Otherwise clients receive `PLO_PLAYERWARP` with packed X/Y and level name.

Then C++ calls either:

- `sendLevel(newLevel, modTime, false)` for `m_versionId >= CLVER_2_1`
- `sendLevel141(newLevel, modTime, false)` for older clients

## sendLevel Runtime Boundary

`sendLevel` immediately sends `PLO_LEVELNAME`, level raw-data/board packets,
layer raw data, `PLO_LEVELMODTIME`, links, signs, board changes, chests, horses,
baddies, GMAP level-name corrections, ghost icon, leadership, world time, active
level, NPC packets, and nearby player props.

This is already runtime world-entry behavior and depends on real `Level`
loading, cached mod times, maps, NPCs, and player lists.

## C# Boundary

The C# protocol project now includes isolated builders for the first confirmed
warp packet bodies:

- `PLO_WARPFAILED + levelName`
- `PLO_PLAYERWARP + GCHAR(x*2) + GCHAR(y*2) + levelName`
- `PLO_PLAYERWARP2 + GCHAR(x*2) + GCHAR(y*2) + GCHAR(z*2+50) + GCHAR(mapX) + GCHAR(mapY) + mapName`
- `PLO_LEVELNAME + levelName`

The C# network project now includes `WarpWorldEntryBoundary.BeginSetLevel`,
which models only the source-confirmed beginning of `Player::setLevel`:

- require the session to be in `ReadyForLevelWarp`
- resolve the target level through `ILevelLookup.FindLevel(levelName)`
- if missing, queue `PLO_WARPFAILED + pLevelName + "\n"` and return
  `MissingLevel`
- if found and `modTime == 0 || version < CLVER_2_1`, queue one warp packet:
  - GMAP and modern client: `PLO_PLAYERWARP2`
  - otherwise: `PLO_PLAYERWARP`
- if found and modern client with non-zero `modTime`, queue no warp packet
  before the runtime boundary
- transition to `ReadyForLevelRuntime`, which maps to the point immediately
  before the C++ call to `sendLevel(...)` or `sendLevel141(...)`

No C# full `warp`, fallback-to-current-level, fallback-to-unstickme,
singleplayer clone, group-map clone, `sendLevel`, or `sendLevel141` runtime
implementation is added.

## Confirmed First Packet Candidates

These packets are confirmed in source but not yet implemented as world-entry
builders:

- Failure: `PLO_WARPFAILED + levelName`
- Non-GMAP warp notification: `PLO_PLAYERWARP + GCHAR(x*2) + GCHAR(y*2) + levelName`
- GMAP warp notification: `PLO_PLAYERWARP2 + GCHAR(x*2) + GCHAR(y*2) + GCHAR(z*2+50) + GCHAR(mapX) + GCHAR(mapY) + mapName`
- Level send begins with `PLO_LEVELNAME + levelName`

Golden bytes for the isolated packet bodies and first `BeginSetLevel` sequences
are documented in `docs/spec/GOLDEN_FIXTURES.md`. Runtime ordering and level
data bytes remain deferred until level packet fixtures are introduced.

## Current Pass Status

The C# server can now cross `ReadyForLevelWarp` only through the
source-confirmed `setLevel` pre-runtime boundary and then stops at
`ReadyForLevelRuntime`.

Still not implemented:

- same-level warp `setProps(PLPROP_X, PLPROP_Y)` branch
- `warp` fallback to previous level
- `warp` fallback to `unstickmelevel`
- singleplayer and group-map clone behavior
- `sendLevel`/`sendLevel141`
- level board/layer/raw-data/resource/NPC/player-list runtime packets
