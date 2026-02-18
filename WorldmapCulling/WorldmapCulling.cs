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
        
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateMap))]

        class Minimap_UpdateMap_Patch
        {
            static void Prefix(Minimap __instance, Player player, float dt, bool takeInput)
            {
                if (__instance.m_mode != Minimap.MapMode.Large)
                {
                    return;
                }

                __instance.m_showNamesZoom = 0.3f;
                
                Jotunn.Logger.LogInfo($"We zoom {__instance.m_largeZoom} / {__instance.m_showNamesZoom}");
            }
        }
    }
}

