# Zelda Level Format Specification

Status: source-confirmed parser implemented for the static `.zelda` file
payload boundary. Runtime side effects remain blocked.

## Source Files

- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/Level.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/LevelLink.cpp`
- `external/gs2lib/src/CString.cpp`

## Header And Graal Fallback

`Level::loadZelda` reads the first eight bytes into `m_fileVersion`.

Before validating Zelda versions, it checks:

```cpp
if (m_fileVersion.subString(0, 2) == "GR")
    return loadGraal(pLevelName);
```

This preserves the old client quirk where versions 1.39 through 1.41r1 saved
`.zelda` files as `.graal` content. The C# parser therefore delegates `GR*`
headers to `GraalLevelParser`.

Confirmed Zelda versions:

```txt
Z3-V1.03 -> v = 3
Z3-V1.04 -> v = 4
```

Unknown versions return false.

## Name And Filesystem Behavior

`loadZelda` selects `FS_LEVEL` unless `nofoldersconfig` is true. It sets:

```txt
m_actualLevelName = m_levelName = pLevelName
m_fileName = fileSystem->find(pLevelName)
m_modTime = fileSystem->getModTime(pLevelName)
```

Like `.graal`, it does not strip directory components with `getFilename`.

## Tile RLE

The tile loop is structurally identical to the `.graal` loader, but the
recovered source sets `bits` using:

```cpp
int bits = (v > 4 ? 13 : 12);
```

Because only `v = 3` and `v = 4` are accepted, both confirmed Zelda versions
use 12-bit tile codes.

Codes are packed least-significant-bit first:

- tile mask `0x0fff`
- control bit `0x0800`
- repeat count `code & 0xff`
- double-repeat flag `code & 0x100`

Regular repeat and double-repeat follow the same C++ loops documented in
`LEVEL_GRAAL_FORMAT_SPEC.md`, including the double-repeat guard
`boardIndex < 64 * 64 - 1`.

## Section Order

After tiles, sections are parsed in this exact order:

1. Links
2. Baddies
3. Signs

`.zelda` has no NPC section and no chest section in this recovered source.

## Links

Links are newline-delimited text lines until empty line or `#`. The parser
tokenizes by spaces, joins extra leading tokens into the destination level name,
and only adds the link when `fileSystem->find(level)` succeeds.

The C# pure parser therefore accepts an explicit `linkTargetExists` callback and
skips links when the callback is absent or returns false.

## Baddies

Baddies are raw signed bytes:

```txt
{signed char x}{signed char y}{signed char type}
```

The sentinel is:

```txt
0xff 0xff 0xff
```

When the sentinel appears, C++ reads one newline-terminated string and exits the
baddy section.

Only `Z3-V1.04` (`v > 3`) reads a newline-terminated verse string and splits it
on `\`. `Z3-V1.03` does not consume verse bytes after each baddy. Fixtures must
therefore place the sentinel immediately after a v1.03 baddy unless additional
raw baddy triples are intended.

## Signs

Sign lines are read until an empty line:

```txt
{GCHAR x}{GCHAR y}{text rest}
```

The C++ passes `encoded = true` to `addSign`, so the parser preserves encoded
text. Client packet sign-code translation remains outside this milestone.

## C# Status

Implemented:

- `ZeldaLevelParser`
- `Z3-V1.03` and `Z3-V1.04` validation
- `GR*` fallback to `GraalLevelParser`
- 12-bit LSB-first tile decoding
- regular and double-repeat RLE
- static link, baddy, and sign payload preservation
- v1.04-only baddy verse consumption
- tests in `ZeldaLevelParserTests`

Blocked:

- Production filesystem/runtime loader wiring for `.zelda`
- baddy ids, props, AI, drops, and timers
- sign text client translation beyond preserving encoded text
- write/save behavior
