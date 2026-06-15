# C# Architecture Plan

Solution projects:

- `GServ`: executable host.
- `GServ.Core`: shared source references, common constants, future clocks/config abstractions.
- `GServ.Protocol`: packet IDs, Graal codecs, encryption/compression/framing, packet abstractions.
- `GServ.Network`: socket/session lifecycle and send/receive queues.
- `GServ.Game`: player/world/level/gameplay domain model.
- `GServ.Scripting`: GS2/V8-compatible scripting boundary and future runtime.
- `GServ.Persistence`: account/settings/filesystem persistence.
- `GServ.Admin`: RC/NC/admin/server-list command surfaces.

Dependency direction:

- Higher-level projects may depend on lower-level projects.
- `GServ.Protocol` must stay independent of gameplay/persistence.
- `GServ.Network` may depend on protocol and core, but not gameplay until session handoff is explicit.
- Scripting should depend on game abstractions only through stable compatibility interfaces.

Current implementation status:

- Only `Core`, `Protocol`, `Network`, and `Scripting` have minimal source-confirmed code.
- `Game`, `Persistence`, and `Admin` are empty boundaries with marker classes.
