# Protocol Open Questions

These questions block a faithful production protocol implementation.

## Missing Authoritative Dependencies

Recovery pass result: all five protocol-critical headers are still missing from this checkout. Build metadata shows they are expected from the external `gs2lib` dependency fetched by CMake from Bitbucket and added via `gs2lib_SOURCE_DIR/include`.

1. Recover `IEnums.h` or equivalent generated enum output.
   - Blocks numeric packet IDs for all `PLI_*`, `PLO_*`, `SVI_*`, `SVO_*`.
   - Blocks numeric `PLTYPE_*`, `CLVER_*`, `RCVER_*`, `NCVER_*`, status, flag, permission, compression, and file-type constants.
2. Recover `CString.h`.
   - Blocks raw bundle length byte order.
   - Blocks exact integer codec behavior.
   - Blocks exact string/token/CSV behavior.
   - Blocks exact compression helper behavior and CRC helper behavior.
3. Recover `CEncryption.h`.
   - Blocks all encrypted client generations.
   - Blocks file queue codec compatibility.
4. Recover `CFileQueue.h`.
   - Blocks outbound bundle construction and compression behavior.
   - Blocks send queue flushing and packet ordering compatibility.
5. Recover `CSocket.h`.
   - Blocks exact socket state, buffering, and websocket interaction behavior.

## Packet ID Risk

The C++ handler tables are authoritative for which packet symbols exist and which handler owns them, but they do not reveal numeric values without `IEnums.h`. The C# port must continue using raw byte wrappers or documented symbolic names only until numeric IDs are confirmed.

## Encoding Risk

Current C# tests cover the assumed printable integer behavior. Only GChar offset behavior is strongly supported by visible C++ call sites. Other integer helpers should be treated as provisional until `CString.h` is recovered or byte-for-byte captures from the C++ server are produced.

## Encryption And Compression Risk

The generation flow is documented, but the actual algorithms and compression-type constants are not. Implementing generation 4/5 login before these are recovered would likely fail against the closed-source client.

## Login Risk

The login field order is documented, but a working login requires:

- numeric session type constants,
- packet bundle length byte order,
- encryption reset semantics,
- outbound file queue bundling,
- server-list behavior or a faithful local substitute,
- exact `PLO_DISCMESSAGE` numeric ID and framing.

## Recommended Recovery Tactics

- Recover `gs2lib` commit `63b1ae96491c188905b50c6b61c8532c601a2122` from Bitbucket or another trusted project cache.
- Search project history, package artifacts, old build machines, CI caches, or dependency archives for the missing shared Graal server headers.
- If source recovery fails, build/run the C++ server with packet logging and capture byte fixtures for:
  - unencrypted generation 1/2 login attempts,
  - simple `PLO_DISCMESSAGE`,
  - `PLO_SIGNATURE`,
  - `PLI_PACKETCOUNT`,
  - `PLO_RAWDATA` plus `PLO_FILE`.
- Compare captured bytes against the current C# codec tests before adding numeric enums.

See also:

- `docs/cpp-missing-dependencies.md`
- `docs/protocol-dependency-call-sites.md`
- `docs/protocol-blockers.md`
