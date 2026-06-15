# Run Local Development Server

## Current Status

A runnable local development server shell is not implemented yet.

The current C# codebase has protocol/session/account/level-boundary components,
but production-authentic login, filesystem-backed level loading, server-list
behavior, and live socket orchestration are still incomplete.

## Safe Future Direction

When enough source-confirmed pieces exist, a dev-only server may be added with:

- explicit in-memory test account provider
- explicit in-memory `.nw` level fixture provider
- clear warnings that fake auth/account data is not production behavior
- TCP accept loop wired through existing session skeletons
- no invented gameplay behavior

The dev server must not pretend fake account validation or fake filesystem
behavior is production-compatible.
