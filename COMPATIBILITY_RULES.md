# Compatibility Rules

- C++ source wins over design preference.
- Recovered exact dependency source may define canonical behavior for shared library classes such as `CString`, `CEncryption`, `CFileQueue`, and `CSocket`.
- Do not modify `ai_resources/`.
- Do not invent packet IDs, field sizes, magic values, encryption behavior, timing, account defaults, or gameplay rules.
- Do not invent feature areas. A system is in scope only when the recovered C++
  source or exact dependency source contains its concrete client-facing
  behavior.
- Absence in the C++ source is compatibility behavior. Do not add built-in
  shops, trades, parties, quests, missions, social systems, or other generic
  gameplay services to make the server feel more complete.
- If behavior is unclear, document it and leave implementation guarded or absent.
- Preserve byte order, integer clamping, signed/unsigned quirks, packet delimiters, raw length transitions, compression thresholds, and bug-compatible behavior.
- Internal C# names may be professional, but docs and tests must trace back to original C++ names.
- Every implemented client-facing behavior needs a test or documented reason why it cannot yet be tested.
