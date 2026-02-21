using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace MapPinDeclutter;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid)]
//[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
internal class MapPinDeclutter : BaseUnityPlugin
{
    private static MapPinDeclutter PluginInstance;

    private const string PluginGUID = "birdhimself.MapPinDeclutter";
    private const string PluginName = "MapPinDeclutter";
    public const string PluginVersion = "0.2.1";

    private PluginConfiguration config;

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private static bool ShouldHideNames(Minimap instance)
    {
        return PluginInstance.config.HideNamesEnabled.Value
               && instance.m_mode == Minimap.MapMode.Large
               && instance.m_largeZoom > PluginInstance.config.HideNamesThreshold.Value;
    }

    private static bool ShouldShowNameForPin(Minimap instance, Minimap.PinData pin)
    {
        if (ShouldAlwaysShowName(pin)) return true;

        var tolerance = PluginInstance.config.HideNamesByDistanceThreshold.Value * instance.m_largeZoom;

        var x = pin.m_pos.x;
        var z = pin.m_pos.z;

        var count = instance.m_pins.Count(p =>
        {
            var xDistance = Math.Abs(p.m_pos.x - x);
            var zDistance = Math.Abs(p.m_pos.z - z);

            return xDistance <= tolerance && zDistance <= tolerance;
        });

        // The given pin is always included in the result so a count of 1 should still show the pin.
        return count <= 1;
    }

    private static bool ShouldAlwaysShowName(Minimap.PinData pin)
    {
        return pin.m_type is Minimap.PinType.Ping or Minimap.PinType.Player or Minimap.PinType.Shout;
    }

    private static bool ShouldZoomIcons(Minimap instance)
    {
        return PluginInstance.config.ZoomIconsEnabled.Value
               && instance.m_mode == Minimap.MapMode.Large
               && instance.m_largeZoom > PluginInstance.config.ZoomIconsThreshold.Value;
    }

    private void Awake()
    {
        PluginInstance = this;

        config = new PluginConfiguration(Config);

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
            var threshold = PluginInstance.config.ZoomIconsThreshold.Value;
            var minimumSize = PluginInstance.config.ZoomIconsMinimumSize.Value;

            if (zoom <= threshold) return 1.0f;

            var t = (zoom - threshold) / (1.0f - threshold);
            var factor = 1.0f - t * (1.0f - minimumSize);

            return factor * pinSize;
        }

        private static void Prefix(Minimap __instance, out MinimapUpdatePinsState __state)
        {
            __state = new MinimapUpdatePinsState
            {
                OgPinSizeLarge = __instance.m_pinSizeLarge
            };

            if (!ShouldZoomIcons(__instance)) return;

            __instance.m_pinSizeLarge = ComputeIconSize(__instance);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (__instance.m_pinSizeLarge != _rememberedPinSizeLarge)
            {
                foreach (var pin in __instance.m_pins)
                {
                    if (pin.m_uiElement == null) continue;

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

            if (!ShouldHideNames(__instance)) return;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var pin in __instance.m_pins)
            {
                if (ShouldAlwaysShowName(pin)) continue;

                if (pin.m_NamePinData != null && pin.m_NamePinData.PinNameGameObject != null)
                    pin.m_NamePinData.PinNameGameObject.SetActive(
                        PluginInstance.config.HideNamesByDistanceEnabled.Value &&
                        ShouldShowNameForPin(__instance, pin));
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

            if (minimap == null) return;

            if (!ShouldHideNames(minimap)) return;

            var goField = AccessTools.Field(__instance.GetType(), "go");
            var go = goField?.GetValue(__instance) as GameObject;

            if (go == null || !go.activeSelf) return;

            var pin = minimap.m_pins.FirstOrDefault(p =>
                p.m_NamePinData?.PinNameGameObject != null &&
                p.m_NamePinData.PinNameGameObject.GetInstanceID() == go.GetInstanceID());

            if (pin == null) return;

            go.SetActive(PluginInstance.config.HideNamesByDistanceEnabled.Value
                ? ShouldShowNameForPin(minimap, pin)
                : ShouldAlwaysShowName(pin));
        }
    }
}
