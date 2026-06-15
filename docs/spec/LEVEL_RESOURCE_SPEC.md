# Level And Resource Lookup Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/Level.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/Map.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/FileSystem.cpp`

## Level::findLevel

`Level::findLevel(pLevelName, loadAbsolute)` first searches the server
`levelList` by comparing `getLevelName().toLower()` to
`pLevelName.toLower()`. This lookup is case-insensitive and returns the first
matching loaded level.

If `loadAbsolute` is true, C++ chooses the general filesystem or `FS_LEVEL`
filesystem depending on `nofoldersconfig`, checks `fileSystem->find(pLevelName)`,
and if missing calls:

```cpp
fileSystem->addFile(pLevelName);
fileSystem->addDir(getPath(pLevelName), "*", true);
```

Normal login/world-entry `Player::setLevel` calls `Level::findLevel(pLevelName,
m_server)` without `loadAbsolute`, so the absolute-load branch is documented but
not implemented in the current C# boundary.

If no loaded level is found, C++ creates a `Level`, calls
`level->loadLevel(pLevelName)`, and returns `nullptr` if loading fails. After a
successful load, it scans server maps and calls `map->isLevelOnMap(...)`; if a
match is found, the level stores that map and map coordinates.

## Map Metadata Used Before Runtime

Before `sendLevel` runtime starts, `Player::setLevel` needs only:

- level name from `Level::getLevelName()`
- map pointer from the earlier `warp` flow
- map type (`MapType::GMAP` vs `MapType::BIGMAP`)
- map name for `PLO_PLAYERWARP2`
- level map coordinates from `getMapX()` and `getMapY()`
- client version comparison with `CLVER_2_1`

The C# `ILevelLookup` returns `LevelEntrySnapshot` and optional
`LevelMapSnapshot` containing only this confirmed pre-runtime data.

## Resource Transfer Boundary

`sendLevel` is the first point where level data/resource transfer begins. It
immediately sends `PLO_LEVELNAME`, then may send:

- `PLO_RAWDATA` board payloads
- layer raw-data packets
- `PLO_LEVELMODTIME`
- links and signs
- board changes, chests, horses, baddies
- GMAP level-name correction
- ghost icon, leadership, new-world-time, active-level, NPC packets
- nearby player props

These packets depend on full `Level` loading, cached mod times, board/layer
data, maps, NPCs, and player lists.

The current C# boundary implements only the static beginning of the modern
`sendLevel` payload using pre-serialized level packet bytes. It still does not
parse level files or construct board/layer/link/sign packets from real runtime
state.

## C# Boundary

Implemented:

- `ILevelLookup.FindLevel(levelName)` abstraction
- `LevelEntrySnapshot`
- `LevelMapSnapshot`
- `LevelMapType.BigMap = 0`, `LevelMapType.Gmap = 1`, matching C++ `MapType`
- `WarpWorldEntryBoundary.BeginSetLevel` pre-runtime packet selection
- `SendLevelBoundary.BeginModern` for `CLVER_2_1+` static level payload:
  `PLO_LEVELNAME`, optional raw board/layers, `PLO_LEVELMODTIME`, links, signs

Not implemented:

- production `Level::findLevel` disk loading
- filesystem extension/directory behavior for levels/resources
- `loadAbsolute` filesystem mutation
- `Level::loadLevel`
- map file parsing
- board/layer/resource packet construction from parsed level state
- `sendFile`/large-file resource transfer during level entry
- old-client `sendLevel141`
- dynamic level-entry runtime packets after links/signs
