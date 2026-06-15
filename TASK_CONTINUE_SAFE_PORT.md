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

* Dev-only local TCP shell exists behind explicit `--dev-only-local`.
* DevOnlyLocalSessionPipeline exists.
* Pipeline currently supports login -> dev-only pre-world auth -> sendLogin boundary -> pre-warp -> filesystem-loaded `.nw` -> SendLevelBoundary.
* Account loading boundary exists.
* Filesystem-backed `.nw` loading boundary exists.
* sendLevel static/dynamic/tail exists.
* Current manual client test is blocked by:

  1. outbound gen2+ compression/encryption/framing exact behavior,
  2. continuous session loop,
  3. movement/player props runtime after level entry.
* Tests are green.

Goal of this run:

Implement the next safest compatibility-critical layer: socket-level CFileQueue flush/compression/encryption behavior, then improve the dev-only session loop if safe.

---

# Milestone 1: Trace CFileQueue send/flush/compression exactly

Deeply trace:

* `external/gs2lib/src/CFileQueue.cpp`
* `external/gs2lib/include/CFileQueue.h`
* `external/gs2lib/src/CEncryption.cpp`
* `external/gs2lib/include/CEncryption.h`
* `external/gs2lib/src/CSocket.cpp`
* `external/gs2lib/include/CSocket.h`
* all C++ server call sites that enqueue/send packets during login/level entry

Focus on:

* `sendCompress`
* queue structure
* packet bundling
* compression threshold
* compression constants/flags
* encryption generation behavior during send
* PLO_RAWDATA behavior
* PLO_FILE behavior
* PLO_BUNDLE behavior
* newline packet handling
* partial socket writes
* websocket branch behavior if present
* exact bytes emitted to the socket for gen1/gen2/gen3/gen4/gen5/gen6 if source-confirmed

Document everything before implementing.

---

# Milestone 2: Implement byte-exact GraalFileQueue flush where confirmed

Expand `GraalFileQueue` only where byte-exact behavior is source-confirmed.

Priority order:

1. uncompressed passthrough already supported: verify against C++ again
2. gen2 normal queue flush if confirmed
3. gen3 insert/remove behavior if confirmed
4. gen5 iterator XOR behavior if confirmed
5. compression path if exact zlib/format/flags are confirmed
6. file/rawdata transfer if confirmed
7. websocket path only if confirmed

Allowed:

* Add explicit unsupported exceptions/blocked states for unconfirmed branches.
* Add deterministic tests for every confirmed generation.
* Add golden fixtures for exact output bytes.
* Add small helper code only where source-confirmed.
* Use .NET compression only if it can be proven byte-compatible with C++ output/format; otherwise keep compression blocked.

Not allowed:

* Do not approximate compression output.
* Do not silently use .NET Deflate/ZLib if bytes differ from C++.
* Do not invent encryption iterator state.
* Do not guess websocket behavior.
* Do not hide unsupported branches.

---

# Milestone 3: Integrate GraalFileQueue flush into dev-only TCP shell

If Milestone 2 confirms enough behavior, update the dev-only TCP shell to use the new socket-level outbound flush.

Focus on:

* writing exactly framed/flushed bytes
* preserving packet order
* preserving encryption/compression generation state
* logging generation/queue mode for debugging
* keeping dev-only fake auth separated from production

Allowed:

* Add tests using fake socket/transport.
* Add integration test for one login-level-entry outbound sequence through the queue.
* Keep unsupported branches explicit.

---

# Milestone 4: Continuous session loop

If the flush layer is safe, improve the dev-only TCP shell from one-frame diagnostic to a continuous loop.

Focus on:

* read frames continuously
* pass frames into session pipeline
* flush outbound bytes after each frame
* detect disconnect
* keep state between packets
* handle rawdata mode if source-confirmed
* avoid inventing protocol behavior

Allowed:

* Add cancellation token support.
* Add clear logs.
* Add tests with fake transport.

Not allowed:

* Do not implement unknown player movement just for loop completeness.
* Do not swallow unsupported packets silently unless C++ behavior confirms it.
* Do not add production claims.

---

# Milestone 5: Movement/player props boundary research if time remains

If the CFileQueue and loop work are safe/completed, continue into incoming movement/player props boundary.

Focus on:

* `Player::parsePacket`
* `PLI_PLAYERPROPS` or equivalent
* `Player::setProps`
* `Player::setProp`
* x/y
* direction
* level name
* gani/animation
* forwarding to other players
* validation/clipping
* unsupported packet behavior

Allowed:

* Add parsers/DTOs/tests only for confirmed behavior.
* Do not integrate into live dev server unless source-confirmed enough.

---

# Milestone 6: Docs/tests/report

Create/update docs:

```txt
docs/spec/CFILEQUEUE_FLUSH_SPEC.md
docs/spec/TCP_SESSION_PIPELINE_SPEC.md
docs/spec/RUN_LOCAL_DEV_SERVER.md
docs/spec/MOVEMENT_PLAYER_PROPS_SPEC.md
docs/spec/GOLDEN_FIXTURES.md
docs/spec/KNOWN_BLOCKERS.md
KNOWN_BLOCKERS.md
```

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
* Whether a manual client connection test is now more realistic
* If manual client connection is still not recommended, list the exact smallest blockers
* Safest next step

Continue as far as safely possible. Do not stop after one small task if another safe task can be done safely.
