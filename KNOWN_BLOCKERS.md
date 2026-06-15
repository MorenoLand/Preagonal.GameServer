# Known Blockers

- Exact original `gs2compiler` submodule commit is not present in this fresh source snapshot. The repository URL is confirmed and current source was cloned, but scripting work should recover the exact commit before implementing runtime behavior.
- Full `IEnums.h` packet catalog is large; only foundation-critical IDs are implemented in C# so far.
- Full login success is blocked on account/default account loading, password/server-list verification response flow, server capacity/list-server checks, player property emission, and world warp behavior.
- The login packet parse boundary is implemented, but account validation and `Player::sendLogin` success continuation are intentionally not.
- `CFileQueue` compression and socket flushing are documented but not implemented in C# yet.
- WebSocket handling is gated by `WOLFSSL_ENABLED` code paths and needs a dedicated pass.
- `Server::doMain()` timing branches need a dedicated timing recovery pass.
- Gameplay systems, account persistence, RC/NC file browser, server-list protocol, and scripting bindings are not implemented.
