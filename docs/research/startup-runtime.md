# Startup And Runtime Research

Primary source: `server/src/main.cpp`.

Startup flow:

1. Parse CLI flags unless `USE_ENV` is set.
2. CLI options include `--server`, `--port`, `--localip`, `--serverip`, `--interface`, `--staff`, and `--name`; short forms include `-s`, `-p`, and `-h`.
3. Environment overrides include `SERVER`, `PORT`, `LOCALIP`, `SERVERIP`, `INTERFACE`, `STAFFACCOUNT`, and `SERVERNAME`.
4. Compute the executable base path.
5. If no server is specified, read `startupserver.txt`; if absent, choose the only directory under `servers/`; otherwise fail with settings error.
6. Create `Server`, call `Server::init`, apply override settings, save settings, reload settings, then call `Server::operator()`.

Primary server loop:

- `Server::operator()` sets `running = true` and loops while running.
- Each iteration calls `doMain()`, then `cleanupDeletedPlayers()`.
- Restart is deferred through `m_doRestart`, which calls `cleanup()` and `init(...)`.
- Global shutdown is driven by signal handlers setting `shutdownProgram`.

Timing:

- `Server` constructor initializes `m_lastTimer`, `m_lastNewWorldTimer`, `m_last1mTimer`, `m_last3mTimer`, and `m_last5mTimer` from `high_resolution_clock`.
- `Server::calculateServerTime()` documents Thu Feb 01 2001 17:33:34 GMT+0000 as the likely `timevar` epoch.
- Exact per-tick timing and all `doMain()` timed branches remain a follow-up recovery item.
