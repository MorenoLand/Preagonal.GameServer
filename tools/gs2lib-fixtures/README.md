# gs2lib CFileQueue Fixture Harness

This harness captures socket bytes from the recovered `external/gs2lib`
`CFileQueue::sendCompress` implementation and matching inbound decode behavior
without modifying `ai_resources/` or patching `gs2lib`.

It compiles a local executable that:

1. creates a local TCP socket pair,
2. wraps the sending socket in `CSocket`,
3. queues deterministic payloads through `CFileQueue`,
4. calls `sendCompress`,
5. reads exact bytes from the receiving socket,
6. strips socket length prefixes for selected inbound cases,
7. applies the same decode order as `Player::decryptPacket`,
8. prints one pipe-delimited fixture line per case.

Run from the repository root:

```powershell
tools\gs2lib-fixtures\build.ps1
```

Requirements:

- Visual Studio 2022 C++ tools
- Ninja
- vcpkg at `C:\vcpkg`
- vcpkg packages for `zlib` and `bzip2` on `x64-windows`

The harness intentionally lives outside `ai_resources/`.
