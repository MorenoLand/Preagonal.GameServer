# Protocol Blockers

This document is the implementation gate for protocol work. Do not implement the blocked areas until the named source is recovered or byte-for-byte C++ fixtures replace it.

## Hard Blockers

| Area | Blocked by | Why it matters |
| --- | --- | --- |
| Numeric packet enums | `IEnums.h` | The client-facing byte IDs for every packet are unknown. |
| Login type masks | `IEnums.h` | The first login byte maps to `PLTYPE_*`; the bit-shift rule is known but the accepted mask values are not. |
| Version gates | `IEnums.h` and version helpers | `CLVER_*`, `RCVER_*`, `NCVER_*` values drive login, file transfer, maps, shooting, and old-client behavior. |
| Bundle length prefix | `CString.h` and `CFileQueue.h` | Inbound reads `readShort`; outbound writes are inside `CFileQueue`. Byte order is not confirmed. |
| Integer codecs | `CString.h` | Current C# helpers are provisional except the GChar offset. |
| String encoding/tokenization | `CString.h` | Packet strings, CSV, gtokenize/guntokenize, and account/file text behavior must match exactly. |
| Encryption | `CEncryption.h` | Generation flow is known, but algorithms, key schedule, and constants are missing. |
| Compression/file queue | `CFileQueue.h` | Outbound bundling, compression, queue flushing, and raw/file packet shipping cannot be made faithful yet. |
| Socket lifecycle | `CSocket.h` | Exact getData/sendData/state/socket-manager behavior is unknown. |

## Partial Non-Blocking Facts

- The C++ server expects the missing headers from `gs2lib`.
- Packet symbol names and handler ownership are confirmed by visible C++.
- `SVF_HEAD = 0`, `SVF_BODY = 1`, `SVF_SWORD = 2`, `SVF_SHIELD = 3`, and `SVF_FILE = 4` are directly defined in `ServerList.h`.
- Outbound newline behavior is confirmed in `Player::sendPacket` and `ServerList::sendPacket`.
- Login first-byte decoding is confirmed: `m_type = 1 << pPacket.readGChar()`.
- Encryption/compression generation dispatch is confirmed at the flow level in `IPacketHandler.h`.

## Safe Work While Blocked

- Improve documentation.
- Add source-status tests.
- Add tests for behavior directly visible in C++ call sites.
- Build scaffolding around raw `PacketId` bytes without assigning enum values.
- Prepare fixture harnesses that can replay captured C++ bytes once available.

## Unsafe Work While Blocked

- Assigning packet numeric IDs from Rust/Python.
- Implementing generation 4/5 encryption from guesses.
- Implementing production login against closed-source clients.
- Finalizing bundle framing byte order.
- Implementing file queue compression/flushing semantics.
- Treating current C# non-GChar integer codecs as final.
