# Level Links And Signs Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/LevelLink.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelLink.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/LevelSign.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelSign.h`
- `external/gs2lib/include/IEnums.h`

## Links

`Level::getLinksPacket()` iterates `m_links` in stored order and appends:

```txt
GCHAR PLO_LEVELLINK
LevelLink::getLinkStr()
"\n"
```

`PLO_LEVELLINK = 1`, so the packet id byte is `33`.

`LevelLink::getLinkStr()` uses:

```txt
"%s %i %i %i %i %s %s"
```

with:

```txt
newLevel x y width height newX newY
```

No extra Graal packing is applied to link fields after the packet id. The link
body is plain text.

## Signs

`Level::getSignsPacket(player)` iterates `m_signs` in stored order and appends:

```txt
GCHAR PLO_LEVELSIGN
LevelSign::getSignStr(player)
"\n"
```

`PLO_LEVELSIGN = 5`, so the packet id byte is `37`.

`LevelSign::getSignStr` writes:

```txt
GCHAR x
GCHAR y
encoded sign text
```

When no `Player*` translation context is supplied, the already encoded sign text
from construction is used. When a player is supplied, C++ translates the
unformatted text first and then encodes it; player translation remains blocked
until the player language/translation path is recovered.

The sign character table and special `#` symbols are ported from
`LevelSign.cpp`. Unknown non-carriage-return characters are emitted through the
source-confirmed `#K(<numeric code>)` path.

## C# Status

Implemented:

- `ServerToPlayerPacketId.LevelLink = 1`
- `ServerToPlayerPacketId.LevelSign = 5`
- `NwLevelPacketBuilder.BuildLinksPacket`
- `NwLevelPacketBuilder.BuildSignsPacket`
- source-confirmed sign text encoder for non-translated sign text

Not implemented:

- link traversal or movement
- player-specific sign translation
- live level mutation of signs/links
