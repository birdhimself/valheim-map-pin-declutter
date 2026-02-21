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
            "Whether to hide names"
        );
        HideNamesThreshold = config.Bind(
            "General",
            "HideNamesThreshold",
            0.02f,
            new ConfigDescription(
                "Zoom threshold when names will be hidden",
                new AcceptableValueRange<float>(0.015f, 1.0f)
            )
        );

        HideNamesByDistanceEnabled = config.Bind(
            "General",
            "HideNamesByDistanceEnabled",
            true,
            "Whether to hide names by distance to other pins instead of simply hiding all names"
        );
        HideNamesByDistanceThreshold = config.Bind(
            "General",
            "HideNamesByDistanceThreshold",
            1000,
            new ConfigDescription(
                "Pins that have other pins within this distance will have their names hidden (calculated relative to the current zoom level)",
                new AcceptableValueRange<int>(100, 3000)
            )
        );

        ZoomIconsEnabled = config.Bind(
            "General",
            "ZoomIconsEnabled",
            true,
            "Whether to zoom icons"
        );
        ZoomIconsThreshold = config.Bind(
            "General",
            "ZoomIconsThreshold",
            0.3f,
            new ConfigDescription(
                "Zoom threshold when icon size will start to be reduced",
                new AcceptableValueRange<float>(0.015f, 1.0f)
            )
        );
        ZoomIconsMinimumSize = config.Bind(
            "General",
            "ZoomIconsMinimumSize",
            0.3f,
            new ConfigDescription(
                "Minimum icon size when zooming icons",
                new AcceptableValueRange<float>(0.1f, 1.0f)
            )
        );
    }
}
