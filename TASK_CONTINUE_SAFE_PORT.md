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
* Auth/server-list boundary exists.
* Account loading boundary exists.
* Player::sendLogin pre-world boundary exists.
* Player props serialization exists for source-confirmed login behavior.
* Runtime ownership exists:

  * RuntimeServer
  * RuntimeLevel
  * RuntimePlayer
  * RuntimeMap
* sendLevel static/dynamic/tail boundaries exist.
* Player visibility sync exists.
* Pure initial `.nw` parser exists for:

  * BOARD
  * LINK
  * SIGN
  * NPC payload passthrough
  * BADDY
* Board/layer packet builders exist.
* CHEST is blocked until LevelItem behavior is confirmed.
* `.graal` and `.zelda` parser are not implemented.
* Movement/player live runtime is not implemented.
* Tests are green.

Goal of this run:

Push the project as far as safely possible toward the first minimal playable milestone:

```txt
client connects -> logs in -> loads account -> enters simple .nw level -> receives board/links/signs/chests where confirmed -> appears in level -> can begin basic source-confirmed player sync/movement boundary
```

Do not stop after one small task if another safe task can be done.

---

# Milestone 1: Complete `.nw` static packets: links and signs

Trace and implement source-confirmed packet builders for `.nw` links and signs.

Focus on:

* `Level::getLinksPacket`
* `Level::getSignsPacket`
* `LevelLink.cpp/.h`
* `LevelSign.cpp/.h`
* exact packet IDs
* exact field order
* exact field encoding
* empty-list behavior
* ordering
* newline/rawdata/filequeue behavior if relevant
* how parsed LINK/SIGN records feed sendLevel

Allowed:

* Add DTOs/snapshots for links/signs if needed.
* Add builders for exact packet bytes.
* Integrate parsed `.nw` links/signs into `SendLevelBoundary`.
* Add tests for empty and small non-empty fixtures.
* Add golden fixtures.

Not allowed:

* Do not invent missing fields.
* Do not approximate formatting.
* Do not implement link traversal/movement yet unless directly required and source-confirmed.

---

# Milestone 2: Recover LevelItem and CHEST behavior

Trace `LevelItem` and any item catalog/constants required for `.nw` CHEST parsing and `Level::getChestPacket`.

Focus on:

* `LevelItem`
* `LevelChest`
* item ID/name mapping
* chest line parsing in `.nw`
* `getItemId`
* chest packet structure
* empty chest behavior
* malformed/unknown item behavior
* item amount/count behavior if present
* exact C++ fallback behavior

Allowed:

* Implement a minimal source-confirmed LevelItem catalog/mapping only if directly recoverable from C++.
* Implement `.nw` CHEST parsing only if behavior is clear.
* Implement chest packet builder if exact bytes are confirmed.
* Add tests/golden fixtures.

Not allowed:

* Do not invent item mappings.
* Do not create a fake item catalog.
* Do not implement inventory gameplay.
* Do not implement chest opening behavior.
* Do not implement item pickup/use behavior.

If CHEST remains blocked, document exactly what is missing and continue.

---

# Milestone 3: Integrate parsed `.nw` snapshot into sendLevel end-to-end

Create an end-to-end source-confirmed path:

```txt
.nw text fixture -> parsed level snapshot -> sendLevel packet sequence
```

Focus on:

* board
* layers if present
* links
* signs
* chests if unblocked
* baddies
* horses if source-confirmed
* NPC payload passthrough
* level name
* modTime
* packet order

Allowed:

* Add adapter from parsed `.nw` snapshot to existing sendLevel snapshots.
* Add integration tests with a small deterministic `.nw` fixture.
* Add golden fixtures for full packet sequence.
* Keep runtime simulation out of scope.

Not allowed:

* Do not execute NPC scripts.
* Do not implement baddy AI.
* Do not implement chest gameplay.
* Do not invent defaults.

---

# Milestone 4: `.graal` and `.zelda` research, optional implementation

Trace `loadGraal` and `loadZelda`.

Focus on:

* format headers/signatures
* board encoding
* object sections
* links/signs/chests/NPCs/baddies if present
* differences from `.nw`
* malformed behavior
* whether these formats are required for current game/client flow

Allowed:

* Document deeply.
* Implement pure parser only if the exact behavior is clear and safe.
* Add tests for format detection and minimal valid fixtures if confirmed.

Not allowed:

* Do not guess old binary/text layouts.
* Do not approximate.

If too risky, keep blocked and move on.

---

# Milestone 5: File/resource transfer and CFileQueue expansion

Trace file/resource transfer paths used during login/level entry.

Focus on:

* `CFileQueue`
* `sendFile`
* `PLO_FILE`
* `PLO_RAWDATA`
* board/file/resource queueing
* image/script/class/resource requests if present
* modTime/cache behavior
* compression thresholds
* compression flags
* encryption during flush
* partial write behavior
* websocket branches if relevant

Allowed:

* Implement byte-exact uncompressed resource/file transfer first.
* Add compression/encryption only if exact byte fixtures can be proven from C++/gs2lib.
* Add tests/golden fixtures.
* Add interfaces for file/resource provider.

Not allowed:

* Do not approximate compression.
* Do not invent resource lookup paths.
* Do not implement full CDN/cache behavior unless source-confirmed.

---

# Milestone 6: Movement/player props receive boundary

Start tracing incoming player movement/props packets, but only implement source-confirmed boundary behavior.

Focus on:

* `Player::parsePacket`
* handlers for player props/movement
* relevant `PLI_*` packets
* `Player::setProps`
* `Player::setProp`
* position fields
* x/y/z if present
* direction
* gani/animation
* current level
* prop forwarding to other players
* validation/clipping behavior
* level link traversal trigger, if present
* disconnect/rejection behavior for invalid props, if any

Allowed:

* Add parsers for confirmed incoming movement/player-prop packets.
* Add DTOs for incoming prop updates.
* Add source-confirmed player state mutation only for safe fields.
* Add forwarding packet builders only if exact behavior is confirmed.
* Add tests/golden fixtures.

Not allowed:

* Do not implement combat.
* Do not implement inventory.
* Do not implement full movement validation unless source-confirmed.
* Do not invent anti-cheat behavior.
* Do not implement link traversal unless traced.
* Do not implement live network loop beyond existing skeleton unless safe.

---

# Milestone 7: Minimal live player sync runtime

If movement/player prop behavior is confirmed enough, integrate it with minimal runtime ownership.

Focus on:

* player entering level
* nearby player sync
* player prop forwarding
* player leaving level
* duplicate/disconnect cleanup
* same-level and GMAP visibility filters
* deterministic packet order

Allowed:

* Implement minimal runtime sync without gameplay.
* Add tests with two or three players.
* Add session state transitions only where source-confirmed.

Not allowed:

* Do not implement combat, item use, weapons, NPC AI, or scripts.
* Do not invent movement behavior.

---

# Milestone 8: First runnable local server skeleton

If enough login -> level entry -> sendLevel -> visibility sync pieces exist, create or improve a local runnable server shell.

Focus on:

* accepting TCP connection if skeleton already supports it
* using existing protocol/session pieces
* using test/in-memory account and level providers only when clearly marked development-only
* making it possible to manually test a client connection to a simple `.nw` fixture
* keeping production behavior blocked where not source-confirmed

Allowed:

* Add a dev-only/in-memory bootstrap configuration.
* Add clear warnings that fake providers are not production-authentic.
* Add docs for how to run the dev server.
* Add integration tests if possible.

Not allowed:

* Do not pretend fake auth/account/level data is production behavior.
* Do not bypass compatibility rules.
* Do not implement unknown behavior just to make the client connect.

---

# Milestone 9: Documentation and blockers

Create/update docs:

```txt
docs/spec/LEVEL_NW_FORMAT_SPEC.md
docs/spec/LEVEL_LINKS_SIGNS_SPEC.md
docs/spec/LEVEL_ITEM_CHEST_SPEC.md
docs/spec/LEVEL_FILE_FORMAT_SPEC.md
docs/spec/LEVEL_LOADING_SPEC.md
docs/spec/SENDLEVEL_SPEC.md
docs/spec/CFILEQUEUE_FLUSH_SPEC.md
docs/spec/MOVEMENT_PLAYER_PROPS_SPEC.md
docs/spec/PLAYER_VISIBILITY_SYNC_SPEC.md
docs/spec/RUN_LOCAL_DEV_SERVER.md
docs/spec/GOLDEN_FIXTURES.md
docs/spec/KNOWN_BLOCKERS.md
KNOWN_BLOCKERS.md
```

For every implemented behavior:

* cite C++/gs2lib source files
* document exact behavior
* add tests
* add golden fixtures where possible
* keep unknowns explicit

Run:

```bash
dotnet build GServharp.sln
dotnet test GServharp.sln
```

At the end, report:

* What was completed
* Which milestones were completed
* Which milestones were blocked and why
* Which C++/gs2lib files were used
* Which C# files/tests were added or modified
* Which docs were updated
* Which golden fixtures were added
* Which behavior is now source-confirmed
* Which behavior remains blocked
* Whether `ai_resources/` stayed untouched
* Build/test results
* Whether the project is closer to a manual client connection test
* Safest next step

Continue as far as safely possible. Do not stop after one small task if another safe task can be done safely.
