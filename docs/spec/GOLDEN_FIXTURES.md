# Golden Fixtures

All byte values are decimal unless noted.

## Login Packets

### PLO_SIGNATURE

C++:

```cpp
sendPacket(CString() >> (char)PLO_SIGNATURE >> (char)73);
```

Packet bytes before queue newline/compression:

```txt
[57, 105]
```

### PLO_DISCMESSAGE

C++:

```cpp
sendPacket(CString() >> (char)PLO_DISCMESSAGE << "No");
```

Packet bytes before queue newline/compression:

```txt
[48, 78, 111]
```

With `Player::sendPacket` newline append:

```txt
[48, 78, 111, 10]
```

### Unknown Login Type

Input login prelude:

```txt
GCHAR 9 => raw byte [41], m_type = 1 << 9 = 512
```

Confirmed response:

```txt
[48] + ASCII("Your client type is unknown.  Please inform the OpenGraal Team.  Type: 512.") + [10]
```

## Framing

Outer socket frame:

```txt
[0, 3, 97, 98, 99] => one inner frame "abc"
```

Raw-data transition:

```txt
PLI_RAWDATA GINT(4) "\n" "abc\n"
```

With client/newer-RC raw newline stripping enabled, raw payload becomes:

```txt
"abc"
```
