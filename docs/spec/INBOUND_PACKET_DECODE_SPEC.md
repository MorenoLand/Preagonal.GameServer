# Inbound Packet Decode Specification

Authoritative sources:

- `ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp`
- `external/gs2lib/include/CEncryption.h`
- `external/gs2lib/src/CEncryption.cpp`
- `external/gs2lib/src/CString.cpp`

## C++ Receive Path

`Player::doMain` reads raw two-byte big-endian socket frame lengths from
`m_recvBuffer`. Once the full frame payload is buffered, it removes the frame
from `m_recvBuffer` and decodes it according to `m_encryptionCodecIn.getGen()`.

Confirmed frame decode:

- `ENCRYPT_GEN_1`: payload is already plain packet bytes.
- `ENCRYPT_GEN_2`: zlib-decompress the whole frame payload.
- `ENCRYPT_GEN_3`: zlib-decompress the whole frame payload, then each
  newline-delimited inner packet is decrypted individually in `parsePacket`.
- `ENCRYPT_GEN_4`: set encryption limit from `COMPRESS_BZ2`, decrypt the whole
  payload, then bzip2-decompress.
- `ENCRYPT_GEN_5+`: first byte is compression type, remove it, set encryption
  limit from that type, decrypt the remaining bytes, then decompress according
  to the type.

Confirmed gen5 compression type values:

```txt
COMPRESS_UNCOMPRESSED = 0x02
COMPRESS_ZLIB = 0x04
COMPRESS_BZ2 = 0x06
```

After frame decode, `parsePacket` splits normal packets by newline. `PLI_RAWDATA`
sets `m_nextIsRaw` and the next packet is read by exact byte length instead of
newline. Bundle handling remains a separate parser boundary.

## Captured Inbound Fixtures

The `tools/gs2lib-fixtures` harness now emits inbound decode fixtures by taking
the source-confirmed socket output, removing the two-byte socket length prefix,
and applying the same decode steps as `Player::decryptPacket`.

Confirmed fixtures:

```txt
inbound-gen2-short-abc-newline
framePayload=78 9C 4B 4C 4A E6 02 00 03 7E 01 31
decoded=61 62 63 0A
```

```txt
inbound-gen5-short-abc-newline
framePayload=02 79 7A B2 DC
decoded=61 62 63 0A
```

```txt
inbound-gen5-zlib-56a-newline
framePayload=04 60 84 9A 9A 5C D3 31 82 58 46 1C 13 5A
decoded=61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61
        61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61
        61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61
        61 61 61 61 61 61 61 0A
```

## C# Boundary

Implemented:

- `InboundPacketDecoder`
- gen1/gen6 passthrough
- gen2 zlib frame decode
- gen3 zlib frame decode plus per-packet gen3 decrypt after newline splitting
- gen5 uncompressed frame decode
- gen5 zlib frame decode
- explicit blocked exceptions for gen4 and gen5 bzip2
- newline splitting into inner packets without the trailing newline
- dev-only TCP shell integration after login using the session's inbound
  generation and login encryption key

The dev-only shell now feeds decoded post-login packets into the existing
`PLI_PLAYERPROPS` boundary. Unsupported packet ids still stop before gameplay
runtime dispatch.

## Blockers

- gen4 bzip2 inbound decode
- gen5 bzip2 inbound decode
- malformed compression-type compatibility beyond explicit blocked logging
- exact `PLI_RAWDATA` state integration after encrypted frame decode
- inbound `PLI_BUNDLE` expansion in the dev shell
- production socket buffering, multi-session forwarding, and gameplay handlers
