# Player Props Open Questions

- Full production `__sendLogin` emission needs real account/default-account values and broader version-specific fixture coverage.
- Account/default-account loading must be ported before production defaults can be trusted.
- `PLPROP_GANI` pre-2.1 bow image/power serialization is implemented for the confirmed login/property serializer path.
- `PLPROP_CURLEVEL` depends on GMAP and singleplayer level state; the C# subset only covers plain current-level string serialization.
- `PLPROP_PSTATUSMSG` depends on server status-list size.
- `PLPROP_GMAPLEVELX`, `PLPROP_GMAPLEVELY`, `PLPROP_X2`, `PLPROP_Y2`, and `PLPROP_Z2` need level/map/movement fixtures.
- `setProps` contains mutation, validation, forwarding, and optional V8 touch-test behavior; it is not implemented.
- The complete `__sendLogin` table is now represented as a tested constant.
