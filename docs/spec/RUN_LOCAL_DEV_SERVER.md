# Run Local Development Server

## Current Status

A minimal diagnostic local development server shell now exists. It is
explicitly dev-only and is not production-compatible.

The current C# codebase has protocol/session/account/level-boundary components,
and now has read-only filesystem-backed `.nw` loading into the static
`sendLevel` boundary. The shell can accept one length-prefixed login frame, run
the confirmed login/account/world-entry boundaries with dev-only auth, load a
`.nw` file, send the confirmed static level packets, and stop before runtime
world simulation.

## Run Command

Prepare a root folder with `world/start.nw`, then run:

```bash
dotnet run --project src/GServ/GServ.csproj -- --dev-only-local --dev-root <root> --dev-level start.nw --port 14900
```

The shell logs a warning on startup. Without `--dev-only-local`, it does not
enable the fake auth path.

Expected limitations:

- accepts one client/login frame at a time
- uses dev-only local auth, not the production list server
- writes uncompressed queued diagnostic bytes for the full login/level response
- stops before movement, NPCs, scripts, file transfer, and live world runtime
- closes after the diagnostic login/level boundary

The protocol project now has source-confirmed socket flush primitives for
gen1/gen6 passthrough, gen2/gen3 zlib, gen5 uncompressed payloads up to 55
bytes, and gen5 zlib payloads through `0x2000` bytes. The dev server does not
yet route its full response through production socket framing because real
login/level responses can cross into blocked bzip2-sized sends.

## Manual Closed-Client Status

A synthetic/manual TCP diagnostic is possible. A meaningful closed-source game
client session is still not expected to work because gen4/gen5 bzip2 socket
framing, continuous session streaming, and runtime movement are not
implemented.
