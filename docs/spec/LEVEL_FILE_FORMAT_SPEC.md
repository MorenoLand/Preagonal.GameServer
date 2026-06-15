# Level File Format Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Map.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/Level.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/Map.h`

## Level Type Selection

`Level::loadLevel(pLevelName)` chooses by extension:

- `.nw` -> `loadNW`
- `.graal` -> `loadGraal`
- `.zelda` -> `loadZelda`
- otherwise -> `detectLevelType`

`detectLevelType` reads the first eight bytes:

- `GLEVNW01` -> NW
- `GR-V1.03`, `GR-V1.02`, `GR-V1.01` -> Graal
- `Z3-V1.04`, `Z3-V1.03` -> Zelda

If no known version is detected, load fails.

## NW Format

`loadNW`:

- selects `FS_LEVEL` unless `nofoldersconfig` is true
- sets `m_actualLevelName = m_levelName = getFilename(pLevelName)`
- resolves `m_fileName = fileSystem->find(m_actualLevelName)`
- sets `m_modTime = fileSystem->getModTime(m_actualLevelName)`
- loads tokenized lines with `CString::loadToken(m_fileName, "\n", true)`
- returns false for empty files
- stores first line as `m_fileVersion`

Recognized line families:

- `BOARD x y width layer tileData`
- `CHEST x y itemName signIndex`
- `LINK ...`
- `NPC image x y`, followed by script lines until `NPCEND`
- `SIGN x y`, followed by text lines until `SIGNEND`
- `BADDY x y type`, followed by verses until `BADDYEND`

`BOARD` tile data is parsed as two base64 characters per tile. The tile value is:

```txt
getBase64Position(left) << 6 + getBase64Position(top)
```

Rows outside `0..64`, invalid widths, or data shorter than `width * 2` are
ignored.

## Graal And Zelda Formats

`loadGraal` and `loadZelda` parse older binary/RLE tile formats. Both set
`m_actualLevelName`, `m_levelName`, `m_fileName`, and `m_modTime` through the
level filesystem. Both validate their eight-byte file version before parsing.

These formats are source-confirmed but not implemented in C# yet because the RLE
and legacy sections need dedicated fixtures.

## Map Formats

`Map::loadBigMap` uses `FS_FILE` unless `nofoldersconfig` is true. It reads
tokenized lines, applies `guntokenize`, computes width/height, lowercases level
names, and stores `MapLevel(x, y)` entries.

`Map::loadGMap` uses `FS_LEVEL` unless `nofoldersconfig` is true. Confirmed
directives include:

- `WIDTH`
- `HEIGHT`
- `GENERATED`
- `LEVELNAMES` through `LEVELNAMESEND`
- `MAPIMG`
- `MINIMAPIMG`
- `NOAUTOMAPPING`
- `LOADFULLMAP`
- `LOADATSTART` through `LOADATSTARTEND`

GMAP level names are untokenized with `guntokenizeI`, split by newline, and
stored lowercased with map coordinates.

## C# Status

No production level-file parser is implemented yet.

The current C# `sendLevel` boundary accepts pre-serialized board/layer/link/sign
packet bytes through DTOs. This avoids inventing parser defaults while still
locking the packet order and framing behavior confirmed in `Player::sendLevel`.
