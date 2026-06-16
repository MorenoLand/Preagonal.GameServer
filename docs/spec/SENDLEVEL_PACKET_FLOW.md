# sendLevel Packet Flow

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/level/Level.cpp`
- `external/gs2lib/include/IEnums.h`

## Player::warp Packet Flow

`Player::warp(level, x, y, modTime)` first checks whether the resolved target is
the current level.

Same-level target:

```txt
setProps(PLPROP_X, GCHAR(x*2), PLPROP_Y, GCHAR(y*2),
         PLSETPROPS_FORWARD | PLSETPROPS_FORWARDSELF)
return true
```

The current C# boundary queues the confirmed self packet:

```txt
PLO_PLAYERPROPS
PLPROP_X
GCHAR(x*2)
PLPROP_Y
GCHAR(y*2)
"\n"
```

Live level-area forwarding from `setProps` remains blocked until multi-session
level ownership is implemented.

Different-level target:

1. Resolve target level.
2. Resolve unstick level from `unstickmelevel`, defaulting to
   `onlinestartlocal.nw`.
3. Leave current level.
4. Set target X/Y.
5. Call `setLevel(target, modTime)`.
6. If that fails, restore old X/Y and call `setLevel(previousLevel)`.
7. If that fails, set unstick X/Y from `unstickmex`/`unstickmey`, defaulting to
   `30.0`/`35.0`, and call `setLevel(unstickLevel)`.
8. Return the original target `setLevel` result, even if fallback succeeded.

Confirmed fallback packet order when the target is missing and previous level
exists:

```txt
PLO_WARPFAILED "missing.nw" "\n"
PLO_PLAYERWARP oldX oldY "start.nw" "\n"
```

Confirmed fallback packet order when the target is missing, no previous level
exists, and unstick level exists:

```txt
PLO_WARPFAILED "missing.nw" "\n"
PLO_PLAYERWARP 30.0 35.0 "onlinestartlocal.nw" "\n"
```

## Player::setLevel Packet Flow

Missing level:

```txt
PLO_WARPFAILED + requested level name
return false
```

Found level:

1. Set current level pointer.
2. Apply singleplayer/group-map clone branches when needed.
3. Add the player id to the level player list.
4. Set `m_levelName`.
5. If `modTime == 0 || m_versionId < CLVER_2_1`, send a warp packet:
   - modern GMAP: `PLO_PLAYERWARP2`
   - otherwise: `PLO_PLAYERWARP`
6. Call modern `sendLevel` for `m_versionId >= CLVER_2_1`.
7. Call old `sendLevel141` for older clients.
8. If level send fails, `leaveLevel`, send `PLO_WARPFAILED`, and return false.
9. Apply sparring-zone AP mutation if needed.
10. Return true.

The C# boundary implements steps 1, 4, 5, and the transition immediately before
step 6/7. Clone ownership, player-list mutation, sparring-zone AP mutation, and
live runtime side effects remain blocked.

## Modern sendLevel Packet Flow

The implemented modern packet order is documented in `docs/spec/SENDLEVEL_SPEC.md`.
C# currently supports source-confirmed static, dynamic wrapper, post-dynamic,
and nearby-player-prop packets from explicit snapshots/pre-serialized payloads.

## Old sendLevel141 Packet Flow

`sendLevel141` is traced but not implemented. Confirmed high-level order:

1. Return false for null level.
2. Read cached level mod time.
3. If requested `modTime == -1`, use `pLevel->getModTime()`.
4. If cached mod time is non-zero, send `getBoardChangesPacket(l_time)`.
5. Otherwise, if requested mod time differs:
   - raw board header
   - board packet
   - `PLO_LEVELNAME` only when `m_firstLevel` is true
   - links/signs only when `serverside=false`
   - `PLO_LEVELMODTIME`
6. Otherwise send `PLO_LEVELBOARD`.
7. If not adjacent, send `getBoardChangesPacket2(l_time)` and chests.
8. If not adjacent, send horses and baddies.
9. If not adjacent and leader/singleplayer, send `PLO_ISLEADER`.
10. Send `PLO_NEWWORLDTIME`.
11. If not adjacent, send NPC packet.
12. If not singleplayer and not adjacent, sync nearby player props.

Blocked details:

- exact `getBoardChangesPacket2` bytes
- `m_firstLevel` lifecycle in the new C# session model
- old-client `serverside` behavior fixtures
- old-client NPC/baddy/player-prop packet fixtures

Do not implement `sendLevel141` until those byte fixtures are captured.
