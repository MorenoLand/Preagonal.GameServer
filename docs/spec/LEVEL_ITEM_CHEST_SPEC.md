# Level Item And Chest Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelItem.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/LevelItem.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelChest.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `external/gs2lib/include/IEnums.h`

## LevelItem Catalog

`LevelItemType` values are source-confirmed:

```txt
INVALID=-1
greenrupee=0
bluerupee=1
redrupee=2
bombs=3
darts=4
heart=5
glove1=6
bow=7
bomb=8
shield=9
sword=10
fullheart=11
superbomb=12
battleaxe=13
goldensword=14
mirrorshield=15
glove2=16
lizardshield=17
lizardsword=18
goldrupee=19
fireball=20
fireblast=21
nukeshot=22
joltbomb=23
spinattack=24
```

`LevelItem::getItemId(name)` is case-sensitive and returns `INVALID` for
unknown names. `getItemId(signed char)` rejects negative ids and ids greater
than or equal to the item count.

## NW CHEST Lines

`loadNW` accepts:

```txt
CHEST x y itemName signIndex
```

only when there are exactly five tokens and `LevelItem::getItemId(itemName)` is
not `INVALID`.

## Chest Packets

`Level::getChestPacket(player)` iterates `m_chests` in stored order. For each
chest it computes:

```txt
hasChest = player->hasChest(Level::getChestStr(chest))
```

`Level::getChestStr(chest)` is:

```txt
"%i:%i:%s" => x:y:levelName
```

Packet body per chest:

```txt
GCHAR PLO_LEVELCHEST
GCHAR hasChest ? 1 : 0
GCHAR x
GCHAR y
if !hasChest:
  GCHAR itemIndex
  GCHAR signIndex
"\n"
```

`PLO_LEVELCHEST = 4`, so the packet id byte is `36`.

## C# Status

Implemented:

- `LevelItemType`
- `LevelItemCatalog.GetItemId(string)`
- `LevelItemCatalog.GetItemId(int)`
- `LevelItemCatalog.GetItemName`
- `.nw` CHEST parsing for known item names
- `NwLevelPacketBuilder.BuildChestPacket`

Not implemented:

- chest opening gameplay
- inventory/player property mutation from item pickup
- account chest persistence beyond the `playerHasChest` predicate boundary
