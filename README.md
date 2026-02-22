# MapPinDeclutter

This Valheim mod attempts to declutter the world map by hiding the names of (most) pins when viewing the map zoomed out. It can also scale pin icons depending on zoom level.

## Configuration

This mod uses BepInEx configuration that can be changed via its configuration file or in-game via [ConfigurationManager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/). The following settings are currently provided:

### Hiding names

- **HideNamesEnabled** (default: `true`): enable hiding of pin names on the map. When enabled, pin names will be hidden based on zoom level or proximity to other pins
- **HideNamesThreshold** (default: `0.02`): the map zoom level above which pin names are hidden. Higher values mean names are hidden at a greater zoom-out distance. Ranges from `0.015` to `1.0`
- **HideNamesByDistanceEnabled** (default: `true`): when enabled, pin names are hidden only for pins that are crowded together, rather than hiding all names at once. Requires *HideNamesEnabled* to be enbaled
- **HideNamesByDistanceThreshold** (default: `1000`): the world-unit radius used to detect crowded pins. A pin's name will be hidden if any other pin falls within this distance, scaled to the current zoom level. Increase to hide names in more sparse areas. Ranges from `100` to `3000`

### Zooming icons

- **ZoomIconsEnabled** (default: `true`): enable scaling down of pin icons when zooming out. When enabled, icons will shrink as the map is zoomed out to reduce visual clutter
- **ZoomIconsThreshold** (default: `0.3`): the map zoom level at which pin icons begin to shrink. Above this threshold, icons scale down proportionally until they reach *ZoomIconsMinimumSize*. Ranges from `0.015` to `1.0`
- **ZoomIconsMinimumSize** (default: `0.3`): the smallest size pin icons can be scaled to when zooming out, as a fraction of their original size (e.g. `0.3` = 30% of full size). Prevents icons from becoming too small to see. Ranges from `0.1` to `1.0`
