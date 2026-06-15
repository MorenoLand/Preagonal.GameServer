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
* Runtime ownership exists.
* sendLevel static/dynamic/tail boundaries exist.
* Player visibility sync exists.
* Pure `.nw` parser exists.
* `.nw` board/layer packet builders exist.
* `.nw` links/signs/chests packets exist.
* LevelItem catalog exists.
* Parsed `.nw` snapshot integrates into sendLevel up to board/layers/links/signs/chests.
* Tests are green.

Goal of this run:

Push the project toward a manually testable local server with a simple `.nw` level, while still keeping production behavior source-confirmed and clearly separating dev-only fakes from real compatibility code.

Continue through these milestones as far as safely possible.

---

# Milestone 1: Filesystem-backed level/resource loading boundary

Trace and implement source-confirmed level/resource lookup behavior needed to load real `.nw` files from disk.

Focus on:

* FileSystem usage for levels/resources
* case-insensitive lookup behavior
* level path normalization
* extension behavior
* nofoldersconfig behavior if source-confirmed
* modTime behavior
* missing level behavior
* malformed level behavior
* level cache behavior if source-confirmed
* resource/file lookup used during sendLevel

Allowed:

* Add filesystem abstractions.
* Add production-safe read-only level loading service if behavior is confirmed.
* Add tests using temporary files or in-memory fake filesystem.
* Add docs for unresolved filesystem quirks.

Not allowed:

* Do not invent search paths.
* Do not implement writes unless source-confirmed.
* Do not approximate cache/modTime behavior.

---

# Milestone 2: Integrate filesystem-loaded `.nw` into sendLevel

Create a source-confirmed path:

```txt
level filename -> filesystem lookup -> .nw parse -> level snapshot -> sendLevel packet sequence
```

Focus on:

* level name
* modTime
* board/layers
* links
* signs
* chests
* baddies if already supported
* NPC payload passthrough if already supported
* packet order
* missing/malformed level failure behavior

Allowed:

* Add integration tests with a small temporary `.nw` fixture.
* Add golden fixtures for complete packet sequence.
* Add adapters between level loading and existing SendLevelBoundary.
* Keep runtime simulation out of scope.

---

# Milestone 3: Dev-only local server shell

If enough source-confirmed pieces exist, create or improve a local development server shell that can be manually tested with a simple client connection.

Focus on:

* TCP listener skeleton
* session pipeline using existing protocol/session code
* dev-only account provider
* dev-only level provider using real `.nw` file loading
* clear configuration for local test world
* clear warning that fake/dev auth is not production-compatible
* ability to reach login -> account -> level entry path using a simple `.nw` fixture if safe

Allowed:

* Add dev-only bootstrap/configuration.
* Add in-memory/dev fake auth clearly marked.
* Add docs explaining how to run locally.
* Add integration tests where possible.

Not allowed:

* Do not pretend dev fake auth is real production auth.
* Do not bypass source-confirmed packet behavior.
* Do not implement unknown behavior just to make the client connect.
* Do not hide blockers.

---

# Milestone 4: Movement/player props receive boundary

If the local level-loading path is stable, begin tracing incoming movement/player props packets.

Focus on:

* Player::parsePacket
* PLI player prop/movement handlers
* Player::setProps
* Player::setProp
* x/y/z if present
* direction
* current level
* animation/gani
* prop forwarding to other players
* clipping/validation
* link traversal trigger
* disconnect/rejection behavior for invalid updates, if any

Allowed:

* Add parsers for confirmed incoming player prop packets.
* Add DTOs for source-confirmed movement/property updates.
* Add state mutation only for confirmed safe fields.
* Add forwarding builders only if exact bytes are confirmed.
* Add tests/golden fixtures.

Not allowed:

* Do not implement combat.
* Do not implement weapons/items.
* Do not implement NPC AI.
* Do not execute scripts.
* Do not invent anti-cheat or validation behavior.
* Do not implement link traversal unless fully traced.

---

# Milestone 5: Minimal live player sync runtime

If movement/player prop behavior is confirmed enough, integrate it with runtime ownership.

Focus on:

* player joining level
* player leaving level
* nearby player visibility
* forwarding property updates
* same-level and GMAP filtering
* deterministic ordering
* duplicate/disconnect cleanup

Allowed:

* Add tests with two or three players.
* Keep this strictly to sync/visibility.
* No gameplay systems.

---

# Milestone 6: Docs and tests

Create/update docs:

```txt
docs/spec/LEVEL_FILESYSTEM_LOADING_SPEC.md
docs/spec/LEVEL_RESOURCE_SPEC.md
docs/spec/SENDLEVEL_SPEC.md
docs/spec/RUN_LOCAL_DEV_SERVER.md
docs/spec/MOVEMENT_PLAYER_PROPS_SPEC.md
docs/spec/PLAYER_VISIBILITY_SYNC_SPEC.md
docs/spec/CFILEQUEUE_FLUSH_SPEC.md
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
* Whether a manual client connection test is now possible
* If not possible, list the exact 3 smallest blockers
* Safest next step

Continue as far as safely possible. Do not stop after one small task if another safe task can be done safely.
