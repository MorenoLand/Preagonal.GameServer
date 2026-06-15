# Level Loading Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/Level.h`

## Confirmed Loader Selection

`Level::loadLevel(pLevelName)` chooses the parser by exact extension first:

- `.nw` -> `loadNW(pLevelName)`
- `.graal` -> `loadGraal(pLevelName)`
- `.zelda` -> `loadZelda(pLevelName)`
- anything else -> `detectLevelType(pLevelName)`

The extension checks are literal string comparisons in C++; uppercase
extensions are not confirmed as accepted.

`Level::detectLevelType(pLevelName)` reads the first eight bytes and maps:

- `GLEVNW01` -> NW
- `GR-V1.03`, `GR-V1.02`, `GR-V1.01` -> Graal
- `Z3-V1.04`, `Z3-V1.03` -> Zelda

If no known eight-byte signature is detected, the load fails.

## Confirmed NW Boundary

`loadNW`:

- selects `FS_LEVEL` unless `nofoldersconfig` is true
- sets `m_actualLevelName = m_levelName = getFilename(pLevelName)`
- resolves `m_fileName` through the level filesystem
- stores `m_modTime = fileSystem->getModTime(m_actualLevelName)`
- tokenizes the file by newline with `CString::loadToken(m_fileName, "\n", true)`
- returns false when no lines were loaded
- stores the first line as `m_fileVersion`

Recognized source line families include `BOARD`, `CHEST`, `LINK`, `NPC`,
`SIGN`, and `BADDY`.

## C# Status

Implemented:

- `LevelFileFormat`
- `LevelFileFormatDetector.FromExtension`
- `LevelFileFormatDetector.DetectFromHeader`
- `LevelFileFormatDetector.Choose`

The C# implementation mirrors the confirmed extension-first selection and the
confirmed eight-byte signatures.

The C# implementation now includes a pure `.nw` parser for source-confirmed
line behavior:

- empty content returns `Success=false`
- unknown line families are ignored
- `BOARD` writes decoded tiles into immutable snapshots
- `LINK` is accepted only through an explicit target-exists callback
- `SIGN` preserves multiline text through `SIGNEND`
- `CHEST` accepts only source-confirmed `LevelItem` names
- `NPC` preserves image, position, and raw script payload through `NPCEND`
- `BADDY` preserves x/y/type and verse lines through `BADDYEND`

The parser does not execute scripts, create runtime NPCs, run baddy AI, or
perform chest-opening gameplay.

## Blocked Parser Areas

- exact `CString::loadToken` behavior for every malformed/trailing newline edge
- production filesystem-backed snapshot loading
- baddy runtime ids and property packet construction
- NPC runtime creation and property packet construction
- legacy Graal and Zelda RLE/tile parsing
- level filesystem lookup and `nofoldersconfig` behavior
