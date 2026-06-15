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
* Account loading boundary exists.
* ReadyForLevelRuntime exists.
* sendLevel static/dynamic/tail boundaries exist.
* Player visibility sync exists.
* Minimal runtime ownership exists:

  * RuntimeServer
  * RuntimeLevel
  * RuntimePlayer
  * RuntimeMap
* Level format detection exists:

  * `.nw`
  * `.graal`
  * `.zelda`
  * headers/signatures
* Tests are green.

Now continue through these next safe milestones:

# 1. Pure `loadNW` parser

Trace and implement a pure parser for source-confirmed `.nw` level format behavior.

Focus on:

* `Level::loadNW`
* line parsing
* board/tile data parsing
* BOARD section
* level width/height assumptions
* modTime behavior if related
* links
* signs
* chests
* baddies
* horses
* NPC lines only as serialized/source payload, without execution
* unknown line behavior
* malformed/empty file behavior
* exact differences between `.nw` and other formats

Allowed:

* Add immutable DTOs/snapshots for parsed `.nw` data.
* Add pure parser code.
* Add small fixtures as test strings.
* Add tests for board, empty file, unknown lines, links/signs if confirmed.
* Add docs and golden fixtures.

Not allowed:

* Do not execute scripts.
* Do not implement NPC runtime.
* Do not implement baddy AI.
* Do not invent recovery behavior for malformed files.
* Do not implement `.graal`/`.zelda` fully unless safely confirmed as separate pure parsers.

# 2. Feed parsed `.nw` snapshots into sendLevel

If the `.nw` parser is confirmed enough, connect its output to existing `SendLevelBoundary`.

Focus on:

* raw board payload
* layer payloads
* links payload
* signs payload
* chests/horses/baddies payload
* NPC serialized payload passthrough
* modTime
* packet order

Allowed:

* Add integration tests from `.nw` fixture → `sendLevel` packet sequence.
* Add golden fixtures for exact packet bytes.
* Keep runtime simulation out of scope.

# 3. Research `.graal` and `.zelda` parsers

Trace `loadGraal` and `loadZelda`.

Focus on:

* format signatures
* board decoding
* object sections
* differences from `.nw`
* blockers

Allowed:

* Document deeply.
* Implement only if behavior is clear and safe.
* Otherwise leave blocked with exact notes.

# 4. Runtime ownership integration with parsed levels

If safe, connect parsed level snapshots to minimal runtime ownership.

Focus on:

* level name
* modTime
* board/layer snapshot
* links/signs/chests/horses/baddies snapshots
* player membership
* sendLevel using runtime level snapshot

Allowed:

* Add interfaces/adapters.
* Add deterministic tests.
* Do not add live updates yet.

# 5. Docs and tests

Create/update docs:

```txt
docs/spec/LEVEL_NW_FORMAT_SPEC.md
docs/spec/LEVEL_FILE_FORMAT_SPEC.md
docs/spec/LEVEL_LOADING_SPEC.md
docs/spec/SENDLEVEL_SPEC.md
docs/spec/LEVEL_RUNTIME_OWNERSHIP_SPEC.md
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
