# sendLevel Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `external/gs2lib/include/IEnums.h`
- `external/gs2lib/include/CString.h`

## Entry From setLevel

After `Player::setLevel` resolves the level, optionally sends
`PLO_PLAYERWARP`/`PLO_PLAYERWARP2`, and updates `m_levelName`, it calls:

```cpp
if (m_versionId >= CLVER_2_1)
    succeed = sendLevel(newLevel, modTime, false);
else
    succeed = sendLevel141(newLevel, modTime, false);
```

The current C# milestone implements only the beginning of the modern
`sendLevel` path for `CLVER_2_1+`.

## Modern sendLevel Order

`Player::sendLevel(pLevel, modTime, fromAdjacent)`:

1. Returns `false` when `pLevel == nullptr`.
2. Sends `PLO_LEVELNAME + pLevel->getLevelName()` immediately.
3. Reads `l_time = getCachedLevelModTime(pLevel.get())`.
4. If `modTime == -1`, replaces it with `pLevel->getModTime()`.
5. If `l_time == 0`:
   - If `modTime != pLevel->getModTime()`:
     - send `PLO_RAWDATA + GINT(1 + 64 * 64 * 2 + 1)`
     - send `pLevel->getBoardPacket()`
     - for each non-zero layer, send `PLO_RAWDATA + GINT(layer.length())`
       followed by `pLevel->getLayerPacket(layerId)`
   - send `PLO_LEVELMODTIME + GINT5(pLevel->getModTime())`
   - send `pLevel->getLinksPacket()`
   - send `pLevel->getSignsPacket(this)`
6. If `!fromAdjacent`, runtime packets begin:
   - board changes
   - chests
   - horses
   - baddies
7. Then GMAP correction, ghost icon, leadership, new world time, active level,
   NPC packets, nearby player props, and forwarding begin.

## Board And Layer Packets

`Level::getBoardPacket()` returns:

```txt
PLO_BOARDPACKET as GCHAR
4096 raw short tiles
"\n"
```

The raw-data header length sent before it is hardcoded to:

```txt
1 + (64 * 64 * 2) + 1 = 8194
```

`Level::getLayerPacket(layer)` returns:

```txt
PLO_BOARDLAYER as GCHAR
raw layer byte
raw 0
raw 0
raw 64
raw 64
4096 raw short tiles
"\n"
```

The raw-data header for layers uses `layer.length()`.

## C# Boundary

Implemented:

- `ModernLevelPayload`
- `LevelLayerPayload`
- `SendLevelRequest`
- `SendLevelBoundary.BeginModern`
- `SessionLifecycle.LevelPayloadSent`

The C# boundary queues only:

- `PLO_LEVELNAME`
- optional raw board/layer payloads using pre-serialized bytes
- `PLO_LEVELMODTIME`
- pre-serialized links packet bytes
- pre-serialized signs packet bytes

The boundary stops before board changes/chests/horses/baddies and marks the
session `LevelPayloadSent`.

Not implemented:

- old `sendLevel141`
- `getCachedLevelModTime`
- production `Level::getBoardPacket` from tile state
- production `Level::getLayerPacket`
- links/signs builders from parsed level state
- board changes, chests, horses, baddies
- GMAP correction packet after dynamic payloads
- ghost icon, leadership, world time, active-level, NPC packets
- nearby player prop forwarding
