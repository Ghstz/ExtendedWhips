using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using RootMotion;

namespace ExtendedWhips
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static Plugin? Instance { get; private set; }
        private Harmony? _harmony;

        public static void LogMsg(string msg)
        {
            Instance?.Log.LogInfo(msg);
        }

        public override void Load()
        {
            Instance = this;
            Log.LogInfo($"Loading {MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}...");

            try
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                _harmony.PatchAll();
                Log.LogInfo("Harmony vehicle physics, screenshot blocker & IMGUI hooks applied successfully!");
            }
            catch (Exception ex)
            {
                Log.LogError($"Harmony patch initialization failed: {ex.Message}");
            }

            // Create persistent GUI host for IMGUI rendering using clean DemoGUIMessage
            try
            {
                var go = new GameObject("ExtendedWhips_GUIHost");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.hideFlags |= HideFlags.HideAndDontSave;
                go.AddComponent<DemoGUIMessage>();
                Log.LogInfo("Persistent IMGUI host instantiated successfully.");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Persistent GUI host note: {ex.Message}");
            }

            Log.LogInfo("=============================================================");
            Log.LogInfo(" [ExtendedWhips] READY! In-Game Controls:");
            Log.LogInfo("  - [F4] / [F7] / [Insert] : Open / Close Vehicle Config Menu");
            Log.LogInfo("  - [F6]        : Toggle Live Telemetry HUD Overlay");
            Log.LogInfo("  - [F5]        : Quick Cycle Presets (Street / Offroad / Drift / Hyper / Stock)");
            Log.LogInfo("  - [Page Up]   : Increase Engine Torque (+0.5x)");
            Log.LogInfo("  - [Page Down] : Decrease Engine Torque (-0.5x)");
            Log.LogInfo("  - [F8]        : Reset Vehicle to Factory Stock Specs");
            Log.LogInfo("=============================================================");
        }
    }

    [HarmonyPatch(typeof(Wb.UI.WbUIInputHandler), nameof(Wb.UI.WbUIInputHandler.OnCaptureScreenshotPressed))]
    public static class BlockScreenshotHook1
    {
        public static bool Prefix()
        {
            Plugin.LogMsg("[ExtendedWhips] Intercepted default screenshot trigger on F7");
            return false;
        }
    }

    [HarmonyPatch(typeof(Wb.WbAdministrationDebug), nameof(Wb.WbAdministrationDebug.CaptureScreenshot))]
    public static class BlockScreenshotHook2
    {
        public static bool Prefix()
        {
            Plugin.LogMsg("[ExtendedWhips] Intercepted WbAdministrationDebug.CaptureScreenshot");
            return false;
        }
    }

    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "com.extendedwhips.consim";
        public const string PLUGIN_NAME = "ExtendedWhips";
        public const string PLUGIN_VERSION = "1.3.0";
    }
}
