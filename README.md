# MapPinDeclutter

This Valheim mod attempts to declutter the world map by hiding the names of (most) pins when viewing the map zoomed out. It can also scale pin icons depending on zoom level.

## Configuration

This mod uses BepInEx configuration that can be changed via its configuration file or in-game via [ConfigurationManager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/). The following settings are currently provided:

### Hiding names

- **HideNamesEnabled** (default: `true`): enable the hide names functionality
- **HideNamesThreshold** (default: `0.02`): when to start hiding names - this is based on the zoom level the map in Valheim uses and can range from `0.015` to `1.0`, with higher numbers meaning it'll trigger at a more zoomed out level
- **HideNamesByDistanceEnabled** (default: `true`): enable hiding names by distance instead of just zoom level - names will only be hidden if other pins are too close when *HideNamesThreshold* is reached
- **HideNamesByDistanceThreshold** (default: `1000`): distance between pin coordinates (x and z axis) where other pins are considered within the threshold for hiding names - this threshold is multiplied by the current zoom level of the map, range from `100` to `3000`

### Zooming icons

- **ZoomIconsEnabled** (default: `true`): enable the icon zooming functionality
- **ZoomIconsThreshold** (default: `0.3`): when to start zooming icons - this is based on the zoom level the map in Valheim uses and can range from `0.015` to `1.0`, with higher numbers meaning it'll trigger at a more zoomed out level
- **ZoomIconsMinimumSize** (default: `0.3`): minimum size of the minimap pin icons when zoomed out as far as possible - ranges from `0.1` to `1.0` (`1.0` effectively disabling the feature) and is relative to the original Valheim pin size
