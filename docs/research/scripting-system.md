# Scripting System Research

The C++ server has two scripting-related paths:

- Always-built sources: `GS2ScriptManager.cpp` and `ScriptClass.cpp`.
- Optional `V8NPCSERVER` sources: `ScriptEngine.cpp`, `ScriptAction`, `ScriptExecutionContext`, `ScriptFactory`, V8 wrappers, and many `V8*Impl.cpp` bindings.

Build facts:

- `V8NPCSERVER` defaults `OFF`.
- When enabled, CMake links V8, `cpp-httplib`, OpenSSL, optional zstd, and generates `EmbeddedBootstrapScript.h` from `bin/servers/default/bootstrap.js`.
- `gs2compiler` is required by the server target and exposed through `GS2COMPILER_INCLUDE_DIRECTORY`.

Behavior confirmed but not implemented:

- Server queues script actions for NPC/player events such as `npc.playerlogin` and `npc.playerlogout` under `V8NPCSERVER`.
- NC/database NPC behavior is gated by `V8NPCSERVER` in many files.
- README documents special NPC commands: `join somefile;`, `singleplayer`, and trigger hacks such as `gr.addweapon`, `gr.addguildmember`, `gr.setgroup`, `gr.appendfile`, `gr.rcchat`, `gr.es`, `gr.attr1` through `gr.attr30`, and `gr.updatelevel`.

Current C# status:

- `GServ.Scripting` is a boundary only.
- `ScriptingRuntimeStatus.IsRuntimeImplemented` is intentionally `false`.
- No scripting runtime, compiler, event queue, or bindings are implemented yet.
