# C# Structure Mapping

This document maps the initial C#/.NET foundation to the original C++ server. It is intentionally structural only; gameplay systems are not implemented yet.

## Source Files Used

Primary C++ sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/main.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/include/Server.h`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/PlayerLogin.cpp`

Supporting docs:

- `docs/original-cpp-server-technical-spec.md`
- `docs/csharp-dotnet-porting-plan.md`

Rust/Python references were not needed for this implementation step.

## Project Layout

- `src/GServ`: process entrypoint and host composition. This maps to C++ `main.cpp` and the top-level server startup loop.
- `src/GServ.Core`: shared compatibility constants, typed IDs, server generation, cached config defaults, and time helpers. This maps to C++ `Server.h`, `Server.cpp`, and shared primitive model concerns.
- `src/GServ.Protocol`: binary protocol utilities and packet abstractions. This maps to C++ `CString` `readG*`/`writeG*` usage and `Player::sendPacket`.
- `src/GServ.Network`: listener/session skeletons. This maps to C++ `Server::onRecv`, `PlayerLogin`, and `Player` subclasses.
- `src/GServ.Persistence.FlatFiles`: reserved for `FlatFileAccountLoader` and `FlatFileNPCLoader`.
- `src/GServ.Game`: reserved for player/world/gameplay systems.
- `src/GServ.Admin`: reserved for RC, NC, and server-list integration.
- `tests/GServ.Protocol.Tests`: compatibility tests for binary primitives, packet framing, and constants.

## Compatibility Notes

Packet numeric enums are intentionally not implemented yet. The current C++ checkout references `IEnums.h`, but that authoritative dependency is missing. The port keeps `PacketId` as a raw byte wrapper and exposes `PacketIdSourceStatus.NumericPacketIdsRecovered = false` until those definitions are recovered.

`GraalBinaryReader` and `GraalBinaryWriter` include the primitive behavior currently evidenced by C++ call sites and the generated spec. `GChar` offset behavior is directly evidenced by C++ subtracting 32 from packet bytes. Other integer helpers are covered by tests but must still be rechecked against the original `CString.h` when it is available.

`PacketFramer.FrameForSend` preserves the C++ `Player::sendPacket` newline rule: empty packets are not sent, packets get a newline unless disabled, and existing newlines are not duplicated.

`ServerCompatibilityOptions.Default` mirrors `ExternalServerCachedSettings` defaults that influence client-visible behavior.

## Next Mapping Targets

1. Recover `IEnums.h`, `CString.h`, `CEncryption.h`, and `CFileQueue.h`.
2. Replace provisional protocol primitive assumptions with direct C++-sourced fixtures.
3. Implement startup directory discovery from `main.cpp`.
4. Implement login packet parsing from `PlayerLogin` and `PlayerClient::handleLogin`.
