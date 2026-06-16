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
* C++ gs2lib fixture harness exists under `tools/gs2lib-fixtures`.
* Byte-exact fixtures were captured through real CFileQueue -> CSocket::sendData.
* GraalFileQueue now supports:

  * gen1/gen6 passthrough
  * gen2/gen3 zlib socket flush
  * gen5 uncompressed payload <= 55
  * gen5 zlib payload 56..0x2000
* gen5 bzip2 > 0x2000 remains explicitly blocked.
* Manual client test is closer but still blocked by:

  1. gen4/gen5 bzip2 socket flush,
  2. integrating FlushSocket into dev TCP shell safely,
  3. continuous session loop + movement/player props runtime.
* Tests are green.

Goal of this run:

Integrate the confirmed socket flush into the dev-only TCP shell for small/medium payloads, add a continuous session loop, and begin the source-confirmed incoming movement/player-props boundary if safe.

Do not block on bzip2 unless it is required by the tested dev payload.

---

# Milestone 1: Integrate confirmed FlushSocket into dev-only TCP shell

Update the dev-only TCP shell to use `GraalFileQueue.FlushSocket` for outbound bytes where confirmed.

Focus on:

* preserving packet order
* preserving generation/key state
* using gen5 zlib/uncompressed where source-confirmed
* not using unsupported bzip2 branches
* not consuming pending queue on unsupported branches
* logging when payload size would require blocked bzip2
* making dev-only test payloads small enough to avoid bzip2 when possible

Allowed:

* Add explicit dev config to keep payload small or fail clearly if bzip2 is required.
* Add tests with fake transport/socket output.
* Add integration test for login -> level entry outbound sequence through FlushSocket.
* Add docs warning about bzip2 limitation.

Not allowed:

* Do not approximate bzip2.
* Do not silently fall back to a different compression mode.
* Do not bypass source-confirmed flush behavior.

---

# Milestone 2: Continuous session loop

Improve the dev-only TCP shell from single-frame diagnostic to continuous session loop.

Focus on:

* accept connection
* read frames repeatedly
* pass frames into session pipeline
* preserve session state between frames
* flush outbound bytes after each frame
* detect disconnect
* support cancellation token
* log unsupported incoming packets clearly
* stop safely on unsupported protocol rather than invent behavior

Allowed:

* Add fake transport tests.
* Add manual run docs.
* Add debug logging.

Not allowed:

* Do not invent handling for unknown PLI packets.
* Do not silently swallow unsupported packets unless C++ confirms this behavior.
* Do not implement gameplay.

---

# Milestone 3: Dev-only manual client test readiness

Make the dev server more ready for a controlled manual test with a tiny `.nw` level.

Focus on:

* create or document a tiny `.nw` fixture that avoids bzip2 threshold
* document exact command to run
* document expected stage reached
* document expected limitations
* document how to recognize blocked bzip2/unsupported movement packets
* ensure dev-only fake auth is visibly marked

Allowed:

* Add sample dev world under a clearly marked dev/test path if appropriate.
* Add docs only if adding files is not appropriate.
* Add integration test using the same fixture if possible.

Not allowed:

* Do not claim production readiness.
* Do not remove warnings.

---

# Milestone 4: Movement/player props receive boundary

If the dev TCP shell is stable enough, trace and implement the first source-confirmed incoming movement/player-props boundary.

Focus on:

* `Player::parsePacket`
* incoming player props packet IDs
* `Player::setProps`
* `Player::setProp`
* x/y
* z if present
* direction
* gani/animation
* current level
* prop forwarding to other players
* validation/clipping
* level link traversal trigger
* unsupported packet behavior

Allowed:

* Add parsers for confirmed incoming prop packets.
* Add DTOs for incoming property updates.
* Add tests/golden fixtures.
* Add source-confirmed mutation of RuntimePlayer only for safe fields.
* Add forwarding builders only if exact behavior is confirmed.

Not allowed:

* Do not implement combat/items/weapons.
* Do not implement NPC/scripts.
* Do not invent anti-cheat.
* Do not implement link traversal unless fully traced.
* Do not integrate movement into live dev server if behavior is incomplete.

---

# Milestone 5: Optional bzip2 research only if time remains

If the previous milestones are complete and safe, research bzip2 implementation options.

Focus on:

* exact bzip2 format emitted by gs2lib
* available .NET libraries
* whether byte output can match fixtures exactly
* licensing/dependency concerns
* tests against harness fixture

Allowed:

* Add dependency only if it is necessary, safe, and byte-exact.
* Otherwise keep bzip2 blocked.

Not allowed:

* Do not add approximate bzip2 behavior.

---

# Milestone 6: Docs/tests/report

Create/update docs:

```txt
docs/spec/TCP_SESSION_PIPELINE_SPEC.md
docs/spec/RUN_LOCAL_DEV_SERVER.md
docs/spec/CFILEQUEUE_FLUSH_SPEC.md
docs/spec/MOVEMENT_PLAYER_PROPS_SPEC.md
docs/spec/CFILEQUEUE_FIXTURE_HARNESS.md
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
* Whether FlushSocket is now used by the dev TCP shell
* Whether the continuous loop exists
* Whether a tiny `.nw` manual client test is now recommended
* Exact command to run if recommended
* Exact known limitations
* Which C# files/tests were added or modified
* Which docs were updated
* Whether `ai_resources/` stayed untouched
* Build/test results
* Safest next step

Continue as far as safely possible. Do not stop after one small task if another safe task can be done safely.
