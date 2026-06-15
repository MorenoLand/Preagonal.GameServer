# C#/.NET Porting Plan

This plan maps the C++ behavior into a professional C# architecture without changing the closed-source client contract.

## Guiding Rule

The C# server may be internally cleaner than the C++ server, but all client-facing behavior must match C++ exactly: packet bytes, packet order, timing, disconnect messages, save behavior, file formats, gameplay rules, and compatibility quirks.

## Proposed Solution Structure

Use a solution layout like:

```txt
src/
  GServ/
    Program.cs
    Hosting/
  GServ.Core/
    Time/
    Ids/
    Configuration/
    Compatibility/
  GServ.Protocol/
    PacketIds.cs
    GraalBinaryReader.cs
    GraalBinaryWriter.cs
    PacketFramer.cs
    Encryption/
  GServ.Network/
    ListenerService.cs
    Session/
    FileQueue/
  GServ.Game/
    Players/
    World/
    Levels/
    Items/
    Combat/
    NPCs/
    Weapons/
    Scripting/
  GServ.Persistence.FlatFiles/
    Accounts/
    NPCs/
    Guilds/
    ServerFlags/
  GServ.Admin/
    RC/
    NC/
    ServerList/
tests/
  GServ.Protocol.Tests/
  GServ.Compatibility.Tests/
  GServ.Persistence.Tests/
  GServ.Game.Tests/
docs/
```

The namespaces should be clean, but class and enum docs should retain C++ names where that helps compatibility audits.

## Phase 0: Recover Missing Protocol Dependencies

Do this before writing production packet code.

Required source recovery:

- `IEnums.h`
- `CString.h`
- `CEncryption.h`
- `CFileQueue.h`
- `CSocket.h`
- any helper headers defining `getVersionID`, packet IDs, encoding, compression, and encryption behavior

Deliverables:

- `docs/protocol/packet-id-map.md`
- `docs/protocol/graal-binary-codec.md`
- Golden tests for every integer/string codec primitive.
- A packet ID enum in C# generated or manually transcribed from authoritative C++ definitions.

Do not use Rust/Python packet numbers as final values unless confirmed against C++.

## Phase 1: Host, Config, and Runtime Skeleton

Map C++ startup behavior from `main.cpp` and `Server::init`.

Implement:

- Command-line/env option parsing with the same discovery order.
- Working-directory selection for server folders.
- Typed configuration loader for `serveroptions.txt` and `adminconfig.txt`.
- `allowedversions.txt` parser with generation-range support.
- Runtime timers matching C++ intervals.
- Structured logging that still preserves important message text for compatibility diagnostics.

Tests:

- Server discovery order.
- Override handling.
- Allowed version formatting.
- Cached setting defaults.

## Phase 2: Protocol, Framing, Encryption, and File Queue

Map behavior from `Player::sendPacket`, `Player::sendFile`, packet handlers, and recovered dependency code.

Implement:

- Graal binary reader/writer.
- Packet framer using newline rules and raw-data exceptions.
- Encryption generations.
- File queue compression and flushing.
- `PLO_RAWDATA`, `PLO_FILE`, large file framing, checksum handling.

Tests:

- Byte-for-byte codec fixtures.
- Raw data/file packet fixture tests.
- Version-specific file behavior for pre-2.1 and pre-2.14 clients.

## Phase 3: Session Lifecycle

Map behavior from `PlayerLogin`, `Player`, and `PlayerClient::handleLogin`.

Implement:

- Temporary login session that swaps into typed session handlers.
- Client, original client, RC, NC, and NPC-server session classes.
- Login parsing, encryption selection, version parsing, account/password/identity parsing.
- Disconnect and cleanup behavior.
- Duplicate login handling.
- Invalid packet threshold handling.
- Packet count warning behavior.

Tests:

- Login packet parsing fixtures.
- Unknown type disconnect.
- Duplicate login stale/non-stale branches.
- Exact disconnect message strings.

## Phase 4: Flat-File Persistence

Map behavior from `FlatFileAccountLoader`, `FlatFileNPCLoader`, `saveServerFlags`, and guild/NPC save flows.

Implement:

- Account parser/writer for `GRACC001`.
- Default account fallback and new account save.
- Guest account behavior.
- Player flags, chests, weapons, attributes.
- Server flags load/save.
- Save timing hooks.

Tests:

- Round-trip account files with CRLF.
- Default omission behavior.
- Flag cropping.
- Chest and weapon parsing.
- Load-only accounts do not save.

## Phase 5: Player Properties and State

Map behavior from `PlayerProps.h`, `PlayerProps.cpp`, and `PropertySerializers.cpp`.

Implement:

- Strongly typed `PlayerProp` enum with exact IDs.
- Property serializer/deserializer types.
- Login prop lists.
- Forward-to-all, forward-to-level, and echo-to-source behavior.
- Generation filtering.
- Client-vs-server set restrictions.

Tests:

- Every property serializer.
- Login prop packet composition.
- Restricted prop behavior with and without NPC server.
- Position prop aliasing (`X`/`X2`, `Y`/`Y2`, `Z`/`Z2`).

## Phase 6: World, Levels, Maps, and Files

Map behavior from `LevelLoader`, `Level`, and `Map`.

Implement:

- Level format loaders.
- Static level data cache.
- Dynamic loaded level instances.
- GMAP, bigmap, groupmap, and singleplayer level support.
- Level links, signs, chests, baddies, NPC templates.
- Board change batching and version-specific packets.

Tests:

- Load default `onlinestartlocal.nw`.
- Load fixtures for binary and NW formats.
- Link parsing with spaces.
- Board packet byte shape.
- GMAP coordinate conversion.

## Phase 7: Gameplay Systems

Map behavior from `PlayerClientPackets`, `PlayerClient`, `PlayerProps`, and `Level`.

Implement:

- Movement, touch checks, server-side signs/links.
- Chat, PM, word filter integration.
- Items, drops, chests, bombs, arrows, explosions, horses.
- Baddies and leader handling.
- PK/death/AP/spar rating behavior.
- Shoot v1/v2 conversion.
- Trigger actions and legacy trigger hacks, preserving settings defaults.

Tests:

- Bush/vase item drops.
- Item laying/taking.
- Death drops.
- AP regeneration and PK AP loss.
- Spar rating formula.
- Shoot packet conversion by client version.

## Phase 8: NPC Server, Scripting, Weapons, RC/NC

Map behavior from `NPCServer`, `NPC`, `Weapon`, `ScriptSystem`, `PlayerRCPackets`, and `PlayerNCPackets`.

Implement incrementally:

- NPC identity, storage type, props, movement queue, events.
- Control-NPC and level NPC event routing.
- Weapon file loading, default weapons, bytecode/script packet behavior.
- Class loading and hot reload.
- RC/NC login rights, packet handlers, account editing, file browser behavior.

Tests:

- NPC prop serialization.
- Weapon add/delete/update packets.
- RC account packet round-trips.
- NC class/NPC update behavior.

## Compatibility Verification Strategy

Use tests as executable documentation:

- Golden byte fixtures for packet encode/decode.
- Golden account file fixtures.
- Golden level loader fixtures.
- Behavior tests copied from C++ formulas and constants.
- Integration harness that can replay captured client packets into both C++ and C# when possible.

Every module should include a `SourceMapping` note in docs or XML comments listing the C++ files used.

## Documentation Work Queue

Add focused docs before implementation of each module:

- `docs/protocol/packet-id-map.md`
- `docs/protocol/session-login.md`
- `docs/protocol/player-properties.md`
- `docs/runtime/startup-and-config.md`
- `docs/persistence/accounts.md`
- `docs/world/levels-and-maps.md`
- `docs/gameplay/items-combat-movement.md`
- `docs/npcserver/npc-and-scripting.md`
- `docs/admin/rc-nc-serverlist.md`

Each doc must include:

- C++ source files used.
- Supporting Rust/Python references if any.
- Exact behavior.
- Unknowns.
- Compatibility tests to add.

## Early Stop Conditions

Pause implementation and investigate C++ again when:

- A packet ID or field size is missing.
- Rust/Python behavior is easier but differs from C++.
- A C++ comment identifies a client quirk.
- A behavior depends on client version.
- A file format parser silently ignores data.
- A setting changes forwarding, timing, or persistence.

The port is only complete when the closed-source client cannot distinguish it from the C++ server.
