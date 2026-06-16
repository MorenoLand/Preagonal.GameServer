# Inventory, Shops, Social, and Quests Spec

## Status

Milestone 14 confirms and ports a narrow durable-gameplay subset:

- level item IDs and names already existed in C# and match C++;
- item pickup property payloads from `LevelItem::getItemPlayerProp`;
- player-drop removal rules from `Player::removeItem`;
- default weapon side effects for weapon pickup items are represented as state changes only;
- shop, trade, party, and quest/mission runtimes are explicitly blocked.

No shop, trade, party, or quest behavior is invented.

## Source Map

### Inventory And Items

- `ai_resources/GServer-CPP-ORIGINAL/server/include/level/LevelItem.h`
  - `LevelItemType` IDs: lines 9-36.
  - `getItemPlayerProp` declarations: lines 64-65.
  - rupee count helper: lines 88-102.
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/LevelItem.cpp`
  - `__itemList`: line 7.
  - `LevelItem::getItemId(signed char)`: line 42.
  - `LevelItem::getItemId(std::string)`: line 50.
  - `LevelItem::getItemName`: line 61.
  - `LevelItem::getItemPlayerProp`: line 68.
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
  - packet routing for `PLI_ITEMADD`/`PLI_ITEMDEL`/`PLI_ITEMTAKE`: lines 182-203.
  - `Player::removeItem`: line 2693.
  - `Player::msgPLI_ITEMADD`: line 2807.
  - `Player::msgPLI_ITEMDEL`: line 2843.
  - chest reward uses `LevelItem::getItemPlayerProp`: line 3131.

### Weapons

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
  - `Player::addWeapon(LevelItemType)`: line 2101.
  - `Player::addWeapon(std::string)`: line 2118.
  - `Player::addWeapon(std::shared_ptr<Weapon>)`: line 2124.
  - `Player::deleteWeapon(...)`: lines 2141-2153.
  - `Player::msgPLI_NPCWEAPONDEL`: line 3349.
  - `Player::msgPLI_WEAPONADD`: line 3376.
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Weapon.cpp`
  - default weapon constructor uses `LevelItem::getItemName`: line 27.
  - weapon packet construction: line 185.
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp`
  - startup creates default weapons for bow/bomb/superbomb/fireball/fireblast/nukeshot/joltbomb: lines 782-788.

### Chat, PM, Profiles, Guilds, Social

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
  - `Player::msgPLI_TOALL`: line 2602.
  - `Player::msgPLI_PRIVATEMESSAGE`: line 3243.
  - `Player::msgPLI_PROFILEGET`: line 4004.
  - `Player::msgPLI_PROFILESET`: line 4011.
  - guild verification request through server-list `SVO_VERIGUILD`: line 2085.
- `ai_resources/GServer-CPP-ORIGINAL/server/src/TriggerCommandHandlers.cpp`
  - `gr.addweapon`: line 54.
  - `gr.deleteweapon`: line 65.
  - `gr.addguildmember`: line 77.
  - `gr.removeguildmember`: line 107.
  - `gr.removeguild`: line 139.
  - `gr.setguild`: line 173.
- `external/gs2lib/include/IEnums.h`
  - `SVO_VERIGUILD = 9`: line 377.

### Shops, Trade, Party, Quests/Missions

No dedicated, source-confirmed C++ shop, trade, party, quest, or mission module was found in this pass. These systems may be implemented in scripts, flags, classes, or content files rather than C++ core code. They remain blocked until exact packet, script, and persistence behavior is recovered.

## Confirmed Item Pickup Payloads

`LevelItem::getItemPlayerProp` returns a player-property payload, not a complete `PLO_PLAYERPROPS` packet. Callers pass the payload into `Player::setProps(..., PLSETPROPS_FORWARD | PLSETPROPS_FORWARDSELF)`.

Confirmed payload rules:

- rupees:
  - green +1, blue +5, red +30, gold +100;
  - clamp resulting rupees to `0..9999999`;
  - payload: `PLPROP_RUPEESCOUNT + GINT rupeeCount`.
- bombs:
  - add `5`;
  - clamp to `0..99`;
  - payload: `PLPROP_BOMBSCOUNT + GCHAR bombCount`.
- darts/arrows:
  - add `5`;
  - clamp to `0..99`;
  - payload: `PLPROP_ARROWSCOUNT + GCHAR arrowCount`.
- heart:
  - add `1.0` HP;
  - clamp to `0..maxPower`;
  - payload: `PLPROP_CURPOWER + GCHAR(newPower * 2)`.
- glove1/glove2:
  - glove2 sets power `3`;
  - glove1 raises power to at least `2`;
  - payload: `PLPROP_GLOVEPOWER + GCHAR glovePower`.
- bow/bomb/superbomb/fireball/fireblast/nukeshot/joltbomb:
  - calls `Player::addWeapon(itemType)`;
  - returns empty payload.
- shield/mirrorshield/lizardshield:
  - candidate powers are `1`, `2`, and `3`;
  - existing higher shield power is preserved;
  - payload: `PLPROP_SHIELDPOWER + GCHAR shieldPower`.
- sword/battleaxe/lizardsword/goldensword:
  - candidate powers are `1`, `2`, `3`, and `4`;
  - existing higher sword power is preserved;
  - payload: `PLPROP_SWORDPOWER + GCHAR swordPower`.
- fullheart:
  - max hearts become `clip(maxPower + 1, 0, 20)`;
  - current power is set to full;
  - payload: `PLPROP_MAXPOWER + GCHAR max + PLPROP_CURPOWER + GCHAR(max * 2)`.
- spinattack:
  - if `PLSTATUS_HASSPIN` is already set, returns empty payload;
  - otherwise sets that bit and returns `PLPROP_STATUS + GCHAR status`.

## Confirmed Player-Drop Removal Rules

`Player::removeItem` is used by `spawnLevelItem` under `V8NPCSERVER` for player-dropped items. Confirmed rules:

- rupees require and subtract their rupee value: `1`, `5`, `30`, or `100`;
- bombs require/subtract `5`;
- darts require/subtract `5`;
- heart requires HP `> 1.0`, then subtracts `1.0`;
- glove1/glove2 removal exists only in the non-`V8NPCSERVER` branch and decrements glove power when current power is `> 1`;
- spinattack clears `PLSTATUS_HASSPIN` if set;
- weapon, shield, sword, fullheart, and most equipment removal is commented out or returns false in the confirmed C++ path.

## C# Mapping

- `src/GServ.Game/InventoryItemRules.cs`
  - `DurablePlayerInventoryState`
  - `InventoryItemRules.BuildPickupPlayerProps`
  - `InventoryItemRules.TryRemoveForPlayerDrop`
  - `DurableGameplayGuards`
- `tests/GServ.Game.Tests/InventoryItemRulesTests.cs`
  - golden byte payload tests and blocked-runtime guard tests.

## Blocked Systems

- Full inventory runtime wiring through `setProps`, level item removal, session forwarding, and persistence save timing.
- Real weapon packets for default and NPC weapons beyond already-existing packet builders.
- Shop, trade, party, quest, and mission behavior.
- Chat/PM word-filter, jail, external-player, NPC-server PM, server-list IRC/social, and profile behavior.
- Guild filesystem mutation and server-list verification side effects.
- Any behavior implemented through GS2 scripts, classes, flags, or content files.
