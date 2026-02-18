using System.Diagnostics.CodeAnalysis;
using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace WorldmapCulling
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class WorldmapCulling : BaseUnityPlugin
    {
        public const string PluginGUID = "birdhimself.WorldmapCulling";
        public const string PluginName = "WorldmapCulling";
        public const string PluginVersion = "0.0.1";
        private const float ZoomThreshold = 0.3f;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("WorldmapCulling has landed");

            // Register the Harmony patches
            Harmony.CreateAndPatchAll(typeof(WorldmapCulling).Assembly, PluginGUID);
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

            private static void Prefix(Minimap __instance, out MinimapUpdatePinsState __state)
            {
                __state = new MinimapUpdatePinsState()
                {
                    OgPinSizeLarge = __instance.m_pinSizeLarge,
                };

                if (__instance.m_mode != Minimap.MapMode.Large || __instance.m_largeZoom <= ZoomThreshold)
                {
                    return;
                }

                __instance.m_pinSizeLarge = (__instance.m_pinSizeLarge * ZoomThreshold) / __instance.m_largeZoom;

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

                if (__instance.m_mode != Minimap.MapMode.Large || __instance.m_largeZoom <= ZoomThreshold)
                {
                    return;
                }

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
    }
}

