using BepInEx.Configuration;

namespace MapPinDeclutter;

public class PluginConfiguration
{
    public readonly ConfigEntry<bool> HideNamesEnabled;
    public readonly ConfigEntry<float> HideNamesThreshold;

    public readonly ConfigEntry<bool> HideNamesByDistanceEnabled;
    public readonly ConfigEntry<int> HideNamesByDistanceThreshold;

    public readonly ConfigEntry<bool> ZoomIconsEnabled;
    public readonly ConfigEntry<float> ZoomIconsThreshold;
    public readonly ConfigEntry<float> ZoomIconsMinimumSize;

    public PluginConfiguration(ConfigFile config)
    {
        HideNamesEnabled = config.Bind(
            "General",
            "HideNamesEnabled",
            true,
            "Enable hiding of pin names on the map. When enabled, pin names will be hidden based on zoom level or proximity to other pins."
        );
        HideNamesThreshold = config.Bind(
            "General",
            "HideNamesThreshold",
            0.02f,
            new ConfigDescription(
                "The map zoom level above which pin names are hidden. Higher values mean names are hidden at a greater zoom-out distance.",
                new AcceptableValueRange<float>(0.015f, 1.0f)
            )
        );

        HideNamesByDistanceEnabled = config.Bind(
            "General",
            "HideNamesByDistanceEnabled",
            true,
            "When enabled, pin names are hidden only for pins that are crowded together, rather than hiding all names at once. Requires HideNamesEnabled to be enabled."
        );
        HideNamesByDistanceThreshold = config.Bind(
            "General",
            "HideNamesByDistanceThreshold",
            1000,
            new ConfigDescription(
                "The coordinate radius used to detect crowded pins. A pin's name will be hidden if any other pin falls within this distance, scaled to the current zoom level. Increase to hide names in more sparse areas.",
                new AcceptableValueRange<int>(100, 3000)
            )
        );

        ZoomIconsEnabled = config.Bind(
            "General",
            "ZoomIconsEnabled",
            true,
            "Enable scaling down of pin icons when zooming out. When enabled, icons will shrink as the map is zoomed out to reduce visual clutter."
        );
        ZoomIconsThreshold = config.Bind(
            "General",
            "ZoomIconsThreshold",
            0.3f,
            new ConfigDescription(
                "The map zoom level at which pin icons begin to shrink. Above this threshold, icons scale down proportionally until they reach ZoomIconsMinimumSize.",
                new AcceptableValueRange<float>(0.015f, 1.0f)
            )
        );
        ZoomIconsMinimumSize = config.Bind(
            "General",
            "ZoomIconsMinimumSize",
            0.3f,
            new ConfigDescription(
                "The smallest size pin icons can be scaled to when zooming out, as a fraction of their original size (e.g. 0.3 = 30% of full size). Prevents icons from becoming too small to see.",
                new AcceptableValueRange<float>(0.1f, 1.0f)
            )
        );
    }
}
