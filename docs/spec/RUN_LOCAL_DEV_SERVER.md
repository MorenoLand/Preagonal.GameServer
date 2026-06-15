# Run Local Development Server

## Current Status

A runnable local development server shell is not implemented yet.

The current C# codebase has protocol/session/account/level-boundary components,
and now has read-only filesystem-backed `.nw` loading into the static
`sendLevel` boundary. Production-authentic login, server-list behavior, live
socket orchestration, and runtime world ownership are still incomplete.

## Safe Future Direction

When enough source-confirmed pieces exist, a dev-only server may be added with:

- explicit in-memory test account provider
- explicit dev-only auth provider, clearly marked as non-production
- explicit `.nw` level directory configuration using `IndexedServerFileSystem`
- clear warnings that fake auth/account data is not production behavior
- TCP accept loop wired through existing session skeletons
- no invented gameplay behavior

The dev server must not pretend fake account validation or fake filesystem
behavior is production-compatible.

Smallest remaining blockers before a meaningful manual closed-client test:

1. TCP/session pipeline from real socket bytes into `ClientSessionSkeleton`.
2. Dev-only auth/server-list substitute that cannot be mistaken for production
   auth.
3. Source-confirmed handoff from login/world-entry boundaries into the
   filesystem-loaded `.nw` `sendLevel` payload without inventing runtime NPC,
   file-transfer, or movement behavior.
