# Movement And Player Props Boundary

Authoritative sources to trace next:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerProps.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/Player.h`
- `external/gs2lib/include/IEnums.h`

## Current Status

This milestone is not implemented yet.

The C# port has outbound login/player property serialization for confirmed
login and level-entry packets, but it does not yet parse live incoming movement
or player property updates.

## Required Recovery Before Implementation

Trace first:

- `Player::parsePacket`
- `PLI_PLAYERPROPS`
- any movement-related `PLI_*` packets
- `Player::setProps`
- `Player::setProp`
- forwarding flags such as `PLSETPROPS_FORWARD` and `PLSETPROPS_FORWARDSELF`
- x/y/z, direction, gani/animation, level, and map fields
- validation/clipping/disconnect behavior
- link traversal trigger behavior

Do not implement movement, anti-cheat, link traversal, or live forwarding until
the exact C++ behavior is documented and tested.
