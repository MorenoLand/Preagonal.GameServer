# Live World Session Forwarding Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/include/Server.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerProps.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/utilities/IdGenerator.h`

## Player Id Allocation

`Server` owns:

```cpp
IdGenerator<uint16_t> m_playerIdGenerator{ PLAYERID_INIT };
```

where `PLAYERID_INIT = 2`.

`IdGenerator<T>::getAvailableId()`:

1. If the free-id set is not empty, returns and removes the smallest free id.
2. Otherwise returns `m_nextId++`.

`Server::addPlayer(player, id)`:

- if `id == USHRT_MAX`, allocates from the id generator
- sets the player id
- stores the player in `m_playerList[id]`
- overwrites an existing map entry for the same id

The C# `RuntimeServer.AddPlayer(player)` now mirrors default allocation from id
`2` and reuses freed ids after deletion cleanup.

## Deferred Deletion

`Server::deletePlayer(player)`:

- returns true for null
- inserts the player into `m_deletedPlayers`
- on first insert, requests server-list deletion
- does not remove the player from `m_playerList` immediately

`cleanupDeletedPlayers()` later:

- frees the player id
- unregisters the socket
- erases the player from `m_playerList`
- calls player cleanup

The C# runtime preserves the deferred player-list removal and id reuse after
`CleanupDeletedPlayers()`. Server-list deletion and socket unregister are still
outside the minimal runtime model.

## Level Membership

`Level::addPlayer(id)` appends to a deque and returns the zero-based index.
`Level::removePlayer(id)` erases every matching id from the deque.
`Level::isPlayerLeader(id)` checks whether the deque front equals `id`.

The C# `RuntimeLevel` preserves these list semantics.

## Level-Area Forwarding

`Server::sendPacketToLevelArea(packet, player, exclude, sendIf)` is the
authoritative path for forwarding local player prop changes such as movement.

No map:

- iterate `level->getPlayers()` order
- skip excluded ids
- require `other->isClient()`
- apply optional predicate
- send the packet

With map:

- iterate server player-list container
- skip excluded ids
- require `other->isClient()`
- apply optional predicate
- require same map object
- group maps require matching group
- require `abs(otherMapX - senderMapX) < 2`
- require `abs(otherMapY - senderMapY) < 2`
- send the packet

The C# `LiveWorldForwardingSelector.SelectLevelAreaRecipients(...)` implements
this routing for explicit runtime player snapshots. No-map ordering follows
level membership order. Map ordering follows the current C# runtime server
collection order; exact C++ `std::unordered_map` iteration order remains a
compatibility risk until container behavior is golden-tested.

## Confirmed Packet Delivery Boundary

`LiveWorldSessionForwarder` is intentionally narrow:

- `ForwardConfirmedLevelAreaPacket(...)` forwards an already-built packet to the
  source-confirmed level-area recipients.
- `ApplyAndForwardConfirmedPlayerProps(...)` applies the confirmed incoming
  movement/player-prop subset, builds the confirmed `PLO_OTHERPLPROPS` movement
  packet, and forwards it to level-area recipients.

Confirmed forwarded player-prop subset:

- `PLPROP_X`
- `PLPROP_Y`
- `PLPROP_Z`
- `PLPROP_X2`
- `PLPROP_Y2`
- `PLPROP_Z2`
- `PLPROP_SPRITE`
- `PLPROP_CURLEVEL`
- `PLPROP_GANI`

Unsupported gameplay packet types are not forwarded through this boundary.

## Implemented C# Types

- `RuntimeUShortIdGenerator`
- `RuntimeServer`
- `RuntimeLevel`
- `RuntimePlayer`
- `LiveWorldForwardingSelector`
- `ILiveWorldSessionSink`
- `LiveWorldSessionForwarder`

## Tests

Tests cover:

- player id allocation starts at `2`
- smallest freed id is reused after deletion cleanup
- level list append/remove/leader behavior
- no-map level-area forwarding in level membership order
- map/group/distance filtering
- confirmed movement prop mutation and forwarded `PLO_OTHERPLPROPS` bytes

## Remaining Blockers

- Real socket/file-queue integration for live sessions.
- C++ `std::unordered_map` iteration order golden tests for map-area forwarding.
- Optional `sendIf` predicates beyond the confirmed client check.
- `sendPacketToAll`, `sendPacketToOneLevel`, and type-specific forwarding.
- Full gameplay packet dispatch.
- Server-list delete side effects during deferred cleanup.
- V8 NPC login/logout script hooks.
