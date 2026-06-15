# Movement And Player Props Boundary

Authoritative sources to trace next:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerProps.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/Player.h`
- `external/gs2lib/include/IEnums.h`

## Confirmed Entry Points

`Player::createFunctions` maps:

```txt
PLI_PLAYERPROPS = 2 -> Player::msgPLI_PLAYERPROPS
```

`msgPLI_PLAYERPROPS` calls:

```cpp
setProps(pPacket, PLSETPROPS_SETBYPLAYER | PLSETPROPS_FORWARD);
```

Confirmed property IDs from `Account.h` and `PlayerProps.cpp` include:

```txt
PLPROP_GANI = 10
PLPROP_X = 15
PLPROP_Y = 16
PLPROP_SPRITE = 17
PLPROP_CURLEVEL = 20
PLPROP_X2 = 78
PLPROP_Y2 = 79
```

For legacy movement fields:

- `PLPROP_X` reads one `GUChar`, stores `m_x = value * 8`, clears paused
  status, updates movement timestamps, marks movement updated, enables touch
  testing, and prepares a forwarded `PLPROP_X2`.
- `PLPROP_Y` mirrors `X` for `m_y`.
- `PLPROP_Z` reads one `GUChar`, stores `(value - 50) * 8`, clears paused
  status, updates movement timestamps, enables touch testing, and prepares
  `PLPROP_Z2`.
- `PLPROP_SPRITE` reads one `GUChar`; non-V8 builds also enable touch testing.
- `PLPROP_CURLEVEL` reads a length-prefixed level name and, in non-V8 builds,
  assigns `m_levelName` without performing a full warp there.
- `PLPROP_GANI` reads a length-prefixed gani string for modern clients. The
  special value `"spin"` emits `PLO_HITOBJECTS` packets around the player.

These are not yet implemented because the side effects immediately cross into
touch/link/chest/NPC/combat/runtime forwarding behavior.

## Current Status

This milestone is not implemented yet.

The C# port has outbound login/player property serialization for confirmed
login and level-entry packets, but it does not yet parse live incoming movement
or player property updates.

## Required Recovery Before Implementation

Trace first:

- `Player::parsePacket`
- full `Player::setProps` loop and end-of-function forwarding behavior
- any movement-related `PLI_*` packets beyond `PLI_PLAYERPROPS`
- `Player::setProps`
- `Player::setProp`
- forwarding flags such as `PLSETPROPS_FORWARD` and `PLSETPROPS_FORWARDSELF`
- x/y/z, direction, gani/animation, level, and map fields
- validation/clipping/disconnect behavior
- link traversal trigger behavior

Do not implement movement, anti-cheat, link traversal, or live forwarding until
the exact C++ behavior is documented and tested.
