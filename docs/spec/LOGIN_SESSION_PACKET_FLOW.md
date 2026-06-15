# Login Session Packet Flow

## Incoming Frame Flow

```txt
socket bytes
  -> raw big-endian short length
  -> compressed/encrypted frame payload
  -> generation-specific decrypt/decompress
  -> inner newline/raw packet stream
  -> first packet while PLTYPE_AWAIT goes to msgPLI_LOGIN
```

For gen2/gen3, the outer frame is zlib-decompressed before inner packet parsing. For gen3, individual inner packets are decrypted after newline/raw splitting. For gen4 and gen5, the full frame is decrypted then decompressed before inner parsing.

## Normal Inner Packet Flow

Normal inner packets are newline-delimited. `Player::parsePacket` reads each packet with `readString("\n")`, decrypts the packet for gen3 client traffic, reads the packet ID with `readGUChar`, increments `m_packetCount`, and dispatches through `TPLFunc[id]`.

## Raw-Data Inner Packet Flow

`PLI_RAWDATA` is packet ID 50. Its payload is a Graal `GINT` raw byte length.

After `msgPLI_RAWDATA`:

```txt
m_nextIsRaw = true
m_rawPacketSize = pPacket.readGUInt()
```

The next inner packet is read by exact byte count instead of newline. If the session is a client, or an RC with version greater than `RCVER_1_1`, a trailing newline byte is stripped from that raw packet.

## Bundle Flow

`IEnums.h` confirms `PLI_BUNDLE = 253` and `PLO_BUNDLE = 253`. The recovered foundation confirms bundle payloads use raw big-endian short length prefixes. The C++ `Player::createFunctions` table does not currently map `PLI_BUNDLE` to a handler in the traced source, so login/session use remains unconfirmed.

## Outbound Login Packets

Confirmed byte fixtures before compression/encryption:

- `PLO_SIGNATURE` plus `GCHAR 73`: `[57, 105]`.
- `PLO_DISCMESSAGE "No"`: `[48, 78, 111]`.
- Unknown type 512 disconnect with `sendPacket` newline: `[48] + ASCII("Your client type is unknown.  Please inform the OpenGraal Team.  Type: 512.") + [10]`.
