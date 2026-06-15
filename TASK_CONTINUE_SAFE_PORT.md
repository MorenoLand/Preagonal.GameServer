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

* Dev-only local TCP shell exists.
* GraalFileQueue.FlushSocket exists.
* Confirmed passthrough socket behavior exists for gen1/gen6.
* Confirmed gen5 short payload behavior exists for payload <= 55.
* gen5 fixture exists:

  * input: `abc\n`, key 0
  * output: `[00 05 02 79 7A B2 DC]`
* Compression/encryption/socket flush branches are still blocked for:

  * gen2
  * gen3
  * gen4
  * gen5 payload > 55
  * zlib/bzip2 exact output
  * integration into full login/level flow
* Tests are green.

Goal of this run:

Recover byte-exact CFileQueue compression/encryption fixtures using a small isolated C++ harness compiled against `external/gs2lib`, then implement only the confirmed matching C# behavior.

---

# Milestone 1: Build a C++ gs2lib fixture harness

Create a small local harness outside `ai_resources/`, preferably under:

```txt
tools/gs2lib-fixtures/
```

The harness should compile against `external/gs2lib` and exercise source-confirmed CFileQueue/CEncryption behavior.

Focus on generating exact socket bytes for:

* gen2 short payload
* gen2 long/compressed payload
* gen3 short payload
* gen3 long/compressed payload
* gen4 if source-confirmed
* gen5 payload <= 55 to verify existing fixture
* gen5 payload > 55
* payloads around compression thresholds
* newline packet payloads
* rawdata payloads if safe

The harness must not modify `ai_resources/`.

Allowed:

* Add a small CMake/project file or simple build script under `tools/gs2lib-fixtures/`.
* Add deterministic fixture inputs.
* Add fixture output files under tests/fixtures or docs/spec fixtures if useful.
* Add docs explaining how to run the harness.

Not allowed:

* Do not patch gs2lib.
* Do not change source behavior to make compilation easier unless done in harness wrapper only.
* Do not invent expected bytes.

If the harness cannot compile because of external dependencies, document the exact compile blockers and continue with source-confirmed C# work that is not blocked.

---

# Milestone 2: Capture and document byte-exact fixtures

Generate and document exact byte outputs for each confirmed case.

Create/update:

```txt
docs/spec/CFILEQUEUE_FIXTURE_HARNESS.md
docs/spec/CFILEQUEUE_FLUSH_SPEC.md
docs/spec/GOLDEN_FIXTURES.md
docs/spec/KNOWN_BLOCKERS.md
KNOWN_BLOCKERS.md
```

For every fixture, document:

* input payload bytes
* encryption generation
* key/iterator state
* compression mode/threshold
* exact output bytes
* C++/gs2lib function path used
* whether the C# implementation matches

---

# Milestone 3: Implement matching C# flush branches only where fixtures prove behavior

Expand `GraalFileQueue` only for branches proven by the harness.

Priority order:

1. gen5 payload > 55 if proven
2. gen2 uncompressed/compressed if proven
3. gen3 uncompressed/compressed if proven
4. gen4 if proven
5. rawdata/file transfer if proven

Allowed:

* Add deterministic golden tests comparing C# output to harness output.
* Add branch support only when bytes match exactly.
* Keep unsupported branches throwing explicit blocked exceptions without consuming pending queue data.
* Use .NET compression only if byte output matches gs2lib fixtures exactly.
* If output differs, keep the branch blocked and document why.

Not allowed:

* Do not approximate compression output.
* Do not accept “semantically equivalent” bytes.
* Do not consume queue on unsupported branches.
* Do not hide unsupported behavior.

---

# Milestone 4: Integrate confirmed flush into dev-only TCP shell

If enough branches are confirmed, integrate the flush path into dev-only local TCP shell.

Focus on:

* using the correct generation/key state
* preserving packet order
* preserving queue flush behavior
* making manual client test more realistic
* keeping fake auth/dev-only path clearly separated

Allowed:

* Add config/debug option for generation if source-confirmed.
* Add tests with fake transport.
* Add docs on manual testing limitations.

Not allowed:

* Do not fake production auth.
* Do not invent client encryption negotiation.

---

# Milestone 5: Continuous session loop if safe

If flush integration is stable, improve dev-only TCP shell from one-frame diagnostic to a continuous session loop.

Focus on:

* read frames repeatedly
* maintain session state
* flush outbound after each frame
* handle disconnect
* preserve rawdata mode if confirmed
* log unsupported packets clearly

Allowed:

* Add cancellation token support.
* Add fake transport tests.

Not allowed:

* Do not implement unsupported movement just to keep loop alive.
* Do not silently swallow unsupported packets unless C++ behavior confirms it.

---

# Milestone 6: Docs/tests/report

Run:

```bash
dotnet build GServharp.sln
dotnet test GServharp.sln
```

If a C++ harness exists, also run its build/test or document why it cannot run.

At the end, report:

* What was completed
* Whether the gs2lib fixture harness was created and ran
* Which fixtures were captured
* Which C# branches now match byte-exact fixtures
* Which branches remain blocked
* Which C++/gs2lib files were used
* Which C# files/tests were added or modified
* Which docs were updated
* Whether `ai_resources/` stayed untouched
* Build/test results
* Whether manual client connection is now more realistic
* If not recommended yet, list the exact smallest blockers
* Safest next step

Continue as far as safely possible. Do not stop after one small task if another safe task can be done safely.
