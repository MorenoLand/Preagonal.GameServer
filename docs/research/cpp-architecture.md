# C++ Architecture Research

The original server uses a large mutable object model centered on `Server`, `Player`, `Level`, `NPC`, `Weapon`, and filesystem/account helpers.

Core ownership:

- `Server` owns player lists, deleted-player queue, levels/maps, NPCs, weapons/classes, settings, logs, server-list connection, socket manager, and optional NPC-server runtime.
- `Player` inherits `Account`, implements `CSocketStub`, owns socket/session state, receive buffer, encryption state, file queue, current level/map, cached levels, external players, and packet handlers.
- `Level`/`Map` own level board state, signs, links, chests, baddies, NPCs, items, and map relationships.
- `Account` handles text-file account load/save behavior.
- `FileSystem` and `CSettings` provide path/settings behavior.

C# mapping:

- Keep protocol encoding and packet IDs isolated in `GServ.Protocol`.
- Keep socket/session orchestration in `GServ.Network`.
- Keep gameplay state in `GServ.Game`, but do not implement gameplay until each C++ subsystem is recovered.
- Keep file/account persistence in `GServ.Persistence`, preserving text-file behavior before changing storage.
- Keep RC/NC/admin packets in `GServ.Admin`.
- Keep scripting separate in `GServ.Scripting`; it must preserve GS2/V8 behavior when implemented.
