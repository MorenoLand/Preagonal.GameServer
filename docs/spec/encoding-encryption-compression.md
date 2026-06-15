# Encoding, Encryption, And Compression Specification

CString raw primitives:

- `writeChar`: one raw byte.
- `writeShort`: two-byte big-endian.
- `writeInt`: four-byte big-endian.
- `writeInt3`: three-byte big-endian low 24 bits.
- Matching reads use the same order.

Graal-packed integers:

- `GCHAR`: one byte, `min(value, 223) + 32`; read subtracts 32.
- `GSHORT`: two base-128 bytes, max 28767, each plus 32.
- `GINT`: three base-128 bytes, max 3682399, each plus 32.
- `GINT4`: four base-128 bytes, max 471347295, each plus 32.
- `GINT5`: five bytes storing a full uint32, first byte uses 4 high bits then four 7-bit chunks, each plus 32.

Encryption generations from `CEncryption.h`:

- `ENCRYPT_GEN_1 = 0`: no encryption/no compression.
- `ENCRYPT_GEN_2 = 1`: no encryption/zlib compression.
- `ENCRYPT_GEN_3 = 2`: single-byte insertion/zlib compression.
- `ENCRYPT_GEN_4 = 3`: partial XOR/bz2 compression.
- `ENCRYPT_GEN_5 = 4`: partial XOR with uncompressed/zlib/bz2 choices.
- `ENCRYPT_GEN_6 = 5`: unknown Graal v6 path, effectively passthrough in `CEncryption`.

Encryption constants:

- `ITERATOR_START = {0, 0, 0x04A80B38, 0x04A80B38, 0x04A80B38, 0}`
- Update formula: `iterator = iterator * 0x08088405 + key`, uint32 overflow.
- Gen3 inserts `')'` at `(iterator & 0x0FFFF) % payloadLength`; decrypt removes one byte computed from the encrypted payload length.
- Gen4/Gen5 XOR little-endian iterator bytes. Iterator advances every four bytes. `limitFromType` maps `0x02 -> 12`, `0x04 -> 4`, `0x06 -> 4`.

Compression constants:

- `COMPRESS_UNCOMPRESSED = 0x02`
- `COMPRESS_ZLIB = 0x04`
- `COMPRESS_BZ2 = 0x06`

`CFileQueue` send behavior:

- Gen1 and Gen6 send queued bytes directly.
- Gen2/Gen3 zlib-compress and prefix raw big-endian short length.
- Gen4 bz2-compress, apply encryption limit for bz2, encrypt, prefix raw big-endian short length.
- Gen5 chooses no compression for payload length `<= 55`, zlib for `>55`, bz2 for `>0x2000`, encrypts with a compression-type-specific limit, and prefixes raw big-endian short `(length + 1)` plus compression type byte.
