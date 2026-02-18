using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

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

        class MinimapUpdatePinsState
        {
            public float ogShowNamesZoom;
            public float ogPinSizeLarge;
        }
        
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePins))]

        class Minimap_UpdatePins_Patch
        {
            private static float zoomThreshold = 0.3f;
            private static float rememberedPinSizeLarge = -1f;
            
            static void Prefix(Minimap __instance, out MinimapUpdatePinsState __state)
            {
                __state = new MinimapUpdatePinsState()
                {
                    ogShowNamesZoom = __instance.m_showNamesZoom,
                    ogPinSizeLarge = __instance.m_pinSizeLarge,
                };

                if (__instance.m_mode != Minimap.MapMode.Large)
                {
                    return;
                }

                if (__instance.m_largeZoom <= zoomThreshold)
                {
                    return;
                }

                __instance.m_showNamesZoom = zoomThreshold;
                __instance.m_pinSizeLarge = __state.ogPinSizeLarge * (1f + zoomThreshold - __instance.m_largeZoom);

                if (__instance.m_pinSizeLarge != rememberedPinSizeLarge)
                {
                    foreach (var pin in __instance.m_pins)
                    {
                        __instance.DestroyPinMarker(pin);
                    }

                    rememberedPinSizeLarge = __instance.m_pinSizeLarge;
                }
            }

            static void Postfix(Minimap __instance, MinimapUpdatePinsState __state)
            {
                __instance.m_showNamesZoom = __state.ogShowNamesZoom;
                __instance.m_pinSizeLarge = __state.ogPinSizeLarge;
            }
        }
    }
}

