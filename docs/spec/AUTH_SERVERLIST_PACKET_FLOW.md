# Auth And Server-List Packet Flow

## Login Auth Request

```txt
client login packet
  -> Player::msgPLI_LOGIN
  -> capacity / ip ban / allowed version / server-list connected checks
  -> ServerList::sendLoginPacketForPlayer
  -> SVO_VERIACC2 queued to list server
```

`SVO_VERIACC2` uses Graal-packed lengths and IDs, not raw length prefixes inside the packet body.

## Login Auth Response

```txt
list-server SVI_VERIACC2
  -> account, id, type, message
  -> overwrite player account name
  -> if message != SUCCESS: PLO_DISCMESSAGE, load-only, disconnect
  -> if message == SUCCESS: Player::sendLogin()
```

The C# port currently stops before `Player::sendLogin` and marks the session `ServerListAuthAcceptedPreWorld`.

## Queue Behavior

`ServerList::sendPacket` appends a newline before passing the packet into
`CFileQueue::addPacket`. The list-server socket uses `ENCRYPT_GEN_2` after
registration, so `CFileQueue::sendCompress` zlib-compresses queued bytes and
prefixes a raw big-endian short length when flushed. The zlib socket flush
format is now fixture-confirmed in `docs/spec/CFILEQUEUE_FIXTURE_HARNESS.md`;
real list-server connection/auth lifecycle remains a separate milestone.
