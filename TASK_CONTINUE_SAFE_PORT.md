Read `AGENTS.md`, `COMPATIBILITY_RULES.md`, `SERVER_SPEC.md`, `PORTING_PLAN.md`, `KNOWN_BLOCKERS.md`, and all docs under `docs/`.

Continue the C#/.NET 1:1 port using only source-confirmed behavior.

Source of truth:

```txt
ai_resources/GServer-CPP-ORIGINAL/
external/gs2lib/
```

Do not modify anything inside `ai_resources/`.

Do not invent behavior.

If something is unclear, document it as unknown and continue with the next safe task.

Current status:

* Login/session boundary exists.
* Pre-world auth/server-list boundary exists.
* Player::sendLogin pre-world boundary exists.
* ReadyForLevelWarp boundary exists.
* Player::setLevel pre-runtime boundary exists.
* ReadyForLevelRuntime state exists.
* AccountFileParser exists.
* AccountLoadService exists.
* Player property serialization has source-confirmed login behavior and `__sendLogin` table.
* GraalFileQueue passthrough behavior exists.
* WarpPackets exists.
* WarpWorldEntryBoundary.BeginSetLevel exists.
* Level lookup abstractions/snapshots exist.
* Tests are green.

Now continue through these next safe milestones:

# 1. Trace `sendLevel` and level payload boundary

Trace `sendLevel`, level serialization, and any first packets emitted after `Player::setLevel`.

Focus on:

* exact function chain after `setLevel(...)`
* when `sendLevel` is called
* what inputs `sendLevel` requires
* level file/content format used by the server
* whether level data is sent as raw data, file queue, normal packets, or resource packets
* packet IDs involved
* packet order
* player position/level name behavior around sendLevel
* how missing/invalid level data behaves
* where real runtime begins after level transmission

Allowed:

* Add source-confirmed DTOs/interfaces for level content.
* Add packet builders only when exact bytes are confirmed.
* Add tests/golden fixtures for confirmed level packets.
* Add documentation for every confirmed branch.

Not allowed:

* Do not implement full level runtime.
* Do not implement NPC runtime.
* Do not execute scripts.
* Do not invent level file defaults or content.
* Do not approximate level serialization.

# 2. Level file/resource format research

Trace how the C++ server parses/loads level and resource files.

Focus on:

* Level.cpp / Level.h
* Map.cpp / Map.h
* FileSystem usage for levels/resources
* level text/binary format behavior
* GMAP behavior
* modTime behavior
* file/resource transfer trigger points
* caching behavior if present
* missing file behavior
* old client behavior differences

Allowed:

* Implement pure parsers only if the exact format is source-confirmed.
* Add test-only in-memory providers.
* Add docs and fixtures.

# 3. CFileQueue/resource transfer expansion

Expand `GraalFileQueue` only where needed for confirmed level/resource transmission.

Focus on:

* `PLO_FILE`
* `PLO_RAWDATA`
* file queue item structure
* file queue compression
* queue thresholds
* encryption during flush
* partial writes
* websocket branches if relevant

Allowed:

* Implement byte-exact uncompressed level/file transfer first if confirmed.
* Add compression/encryption only when exact bytes can be proven.
* Add golden fixtures.

Do not approximate compression or socket behavior.

# 4. First playable boundary

If `sendLevel` can be implemented safely, define the next stop state after the first source-confirmed level transmission.

Potential stop names:

```txt
ReadyForPostLevelRuntime
LevelPayloadSent
ReadyForGameplayRuntime
```

Only add a state if the source boundary is clear.

Do not implement actual gameplay.

# 5. Player properties around level entry

Expand only properties directly required by level entry.

Focus on:

* current level
* x/y
* direction
* gani/animation
* sprite/body/head/shield/sword/colors if sent here
* any properties resent during warp/level entry

Add exact byte tests.

# 6. Docs, tests, report

Create/update docs:

```txt
docs/spec/SENDLEVEL_SPEC.md
docs/spec/LEVEL_FILE_FORMAT_SPEC.md
docs/spec/LEVEL_RESOURCE_SPEC.md
docs/spec/WARP_WORLD_ENTRY_SPEC.md
docs/spec/CFILEQUEUE_FLUSH_SPEC.md
docs/spec/PLAYER_PROPS_SPEC.md
docs/spec/GOLDEN_FIXTURES.md
docs/spec/KNOWN_BLOCKERS.md
```

Run:

```bash
dotnet build GServharp.sln
dotnet test GServharp.sln
```

At the end, report:

* What was completed
* Which C++/gs2lib files were used
* Which C# files/tests were added or modified
* Which docs were updated
* Which golden fixtures were added
* Which behavior is now source-confirmed
* Which behavior remains blocked
* Whether `ai_resources/` stayed untouched
* Build/test results
* Safest next step

Continue as far as safely possible. Do not stop after one small task if another safe task can be done.
