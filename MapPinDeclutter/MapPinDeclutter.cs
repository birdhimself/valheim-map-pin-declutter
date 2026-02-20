using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace MapPinDeclutter
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class MapPinDeclutter : BaseUnityPlugin
    {
        private static MapPinDeclutter PluginInstance;

        private const string PluginGUID = "birdhimself.MapPinDeclutter";
        private const string PluginName = "MapPinDeclutter";
        public const string PluginVersion = "0.0.1";

        private ConfigEntry<bool> configHideNamesEnabled;
        private ConfigEntry<float> configHideNamesThreshold;
        private ConfigEntry<bool> configZoomIconsEnabled;
        private ConfigEntry<float> configZoomIconsThreshold;
        private ConfigEntry<float> configZoomIconsMinimumSize;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private static bool ShouldHideNames(Minimap instance)
        {
            return PluginInstance.configHideNamesEnabled.Value
                   && instance.m_mode == Minimap.MapMode.Large
                   && instance.m_largeZoom > PluginInstance.configHideNamesThreshold.Value;
        }

        private static bool ShouldZoomIcons(Minimap instance)
        {
            return PluginInstance.configZoomIconsEnabled.Value
                   && instance.m_mode == Minimap.MapMode.Large
                   && instance.m_largeZoom > PluginInstance.configZoomIconsThreshold.Value;
        }

        private void Awake()
        {
            PluginInstance = this;

            configZoomIconsEnabled = Config.Bind("General", "ZoomIconsEnabled", true, "Whether to zoom icons");
            configZoomIconsThreshold = Config.Bind("General", "ZoomIconsThreshold", 0.3f, new ConfigDescription("Zoom threshold when icon size will start to be reduced", new AcceptableValueRange<float>(0.0f, 1.0f)));
            configHideNamesEnabled = Config.Bind("General", "HideNamesEnabled", true, "Whether to hide names");
            configHideNamesThreshold = Config.Bind("General", "HideNamesThreshold", 0.3f, new ConfigDescription("Zoom threshold when names will be hidden", new AcceptableValueRange<float>(0.0f, 1.0f)));
            configZoomIconsMinimumSize = Config.Bind("General", "ZoomIconsMinimumSize", 0.3f, new ConfigDescription("Minimum icon size when zooming icons", new AcceptableValueRange<float>(0.1f, 1.0f)));

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("MapPinDeclutter has landed");

            // Register the Harmony patches
            Harmony.CreateAndPatchAll(typeof(MapPinDeclutter).Assembly, PluginGUID);
        }

        private class MinimapUpdatePinsState
        {
            public float OgPinSizeLarge;
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePins))]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private class Minimap_UpdatePins_Patch
        {
            private static float _rememberedPinSizeLarge = -1f;

            private static float ComputeIconSize(Minimap instance)
            {
                var zoom = instance.m_largeZoom;
                var pinSize = instance.m_pinSizeLarge;
                var threshold = PluginInstance.configZoomIconsThreshold.Value;
                var minimumSize = PluginInstance.configZoomIconsMinimumSize.Value;

                if (zoom <= threshold)
                {
                    return 1.0f;
                }

                var t = (zoom - threshold) / (1.0f - threshold);
                var factor = 1.0f - t * (1.0f - minimumSize);

                return factor * pinSize;
            }

            private static void Prefix(Minimap __instance, out MinimapUpdatePinsState __state)
            {
                __state = new MinimapUpdatePinsState()
                {
                    OgPinSizeLarge = __instance.m_pinSizeLarge,
                };

                if (!ShouldZoomIcons(__instance))
                {
                    return;
                }

                __instance.m_pinSizeLarge = ComputeIconSize(__instance);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (__instance.m_pinSizeLarge != _rememberedPinSizeLarge)
                {
                    foreach (var pin in __instance.m_pins)
                    {
                        if (pin.m_uiElement == null)
                        {
                            continue;
                        }

                        var size = pin.m_doubleSize ? __instance.m_pinSizeLarge * 2f : __instance.m_pinSizeLarge;
                        pin.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                        pin.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

                    }

                    _rememberedPinSizeLarge = __instance.m_pinSizeLarge;
                }
            }

            private static void Postfix(Minimap __instance, MinimapUpdatePinsState __state)
            {
                __instance.m_pinSizeLarge = __state.OgPinSizeLarge;

                if (!ShouldHideNames(__instance))
                {
                    return;
                }

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var pin in __instance.m_pins)
                {
                    if (pin.m_type is Minimap.PinType.Ping or Minimap.PinType.Player or Minimap.PinType.Shout)
                    {
                        continue;
                    }

                    if (pin.m_NamePinData != null && pin.m_NamePinData.PinNameGameObject != null)
                    {
                        pin.m_NamePinData.PinNameGameObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private class Minimap_DelayActivation_MoveNext_Patch
        {
            private static MethodBase TargetMethod()
            {
                var nested = AccessTools.FirstInner(typeof(Minimap), t => t.Name.Contains("DelayActivation"));

                return AccessTools.Method(nested, "MoveNext");
            }

            private static void Postfix(object __instance)
            {
                var type = __instance.GetType();
                var minimapField = AccessTools.Field(type, "<>4__this");
                var minimap = minimapField.GetValue(__instance) as Minimap;

                if (minimap == null)
                {
                    return;
                }

                if (!ShouldHideNames(minimap))
                {
                    return;
                }

                var goField = AccessTools.Field(__instance.GetType(), "go");
                var go = goField?.GetValue(__instance) as GameObject;

                if (go != null && go.activeSelf)
                {
                    go.SetActive(false);
                }
            }
        }
    }
}

