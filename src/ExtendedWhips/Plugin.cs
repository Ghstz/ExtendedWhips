using System;
using System.Runtime.InteropServices;
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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        public static void LogMsg(string msg)
        {
            Instance?.Log.LogInfo(msg);
        }

        public override void Load()
        {
            Instance = this;
            Log.LogInfo($"Loading {MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}...");

            // Step 1: Hot-patch native IL2CPP GameAssembly null dereference defect at RVA 0x25B831
            PatchGameAssemblyCrash();

            // Step 2: Apply Harmony hooks for vehicle physics & IMGUI
            try
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                _harmony.PatchAll();
                Log.LogInfo("Harmony vehicle physics & IMGUI hooks applied successfully!");
            }
            catch (Exception ex)
            {
                Log.LogError($"Harmony patch initialization failed: {ex.Message}");
            }

            // Step 3: Create persistent GUI host for IMGUI rendering
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
            Log.LogInfo("  - [F4] or [Insert] : Open / Close Vehicle Config Menu");
            Log.LogInfo("  - [F6]             : Toggle Live Telemetry HUD Overlay");
            Log.LogInfo("  - [F5]             : Quick Cycle Presets (Street / Offroad / Drift / Hyper / Stock)");
            Log.LogInfo("  - [Page Up]        : Increase Engine Torque (+0.5x)");
            Log.LogInfo("  - [Page Down]      : Decrease Engine Torque (-0.5x)");
            Log.LogInfo("  - [F8]             : Reset Vehicle to Factory Stock Specs");
            Log.LogInfo("=============================================================");
        }

        private void PatchGameAssemblyCrash()
        {
            try
            {
                IntPtr hModule = GetModuleHandle("GameAssembly.dll");
                if (hModule == IntPtr.Zero)
                {
                    Log.LogWarning("[ExtendedWhips] GameAssembly.dll handle not found for hot-patching.");
                    return;
                }

                IntPtr patchAddress = IntPtr.Add(hModule, 0x25B831);

                // Expected original instructions: 48 83 3B 00 75 11 (cmp qword ptr [rbx], 0; jne +0x11)
                byte[] expected = new byte[] { 0x48, 0x83, 0x3B, 0x00, 0x75, 0x11 };
                byte[] current = new byte[6];
                Marshal.Copy(patchAddress, current, 0, 6);

                bool matches = true;
                for (int i = 0; i < 6; i++)
                {
                    if (current[i] != expected[i])
                    {
                        matches = false;
                        break;
                    }
                }

                if (!matches)
                {
                    // Check if already patched: 48 85 DB 74 4C 90
                    if (current[0] == 0x48 && current[1] == 0x85 && current[2] == 0xDB)
                    {
                        Log.LogInfo("[ExtendedWhips] GameAssembly.dll RVA 0x25B831 is already patched.");
                        return;
                    }
                    Log.LogWarning($"[ExtendedWhips] GameAssembly bytes at 0x25B831 ({BitConverter.ToString(current)}) did not match expected pattern.");
                    return;
                }

                // PAGE_EXECUTE_READWRITE = 0x40
                if (!VirtualProtect(patchAddress, (UIntPtr)6, 0x40, out uint oldProtect))
                {
                    Log.LogError("[ExtendedWhips] VirtualProtect failed to set memory protection for patch.");
                    return;
                }

                // Replacement: test rbx, rbx (48 85 DB); jz +0x4C (74 4C); nop (90)
                // Safely branches to ret_label when rbx is NULL instead of crashing
                byte[] patch = new byte[] { 0x48, 0x85, 0xDB, 0x74, 0x4C, 0x90 };
                Marshal.Copy(patch, 0, patchAddress, 6);

                VirtualProtect(patchAddress, (UIntPtr)6, oldProtect, out _);
                Log.LogInfo("[ExtendedWhips] Successfully hot-patched GameAssembly.dll RVA 0x25B831 (null dereference fix applied)!");
            }
            catch (Exception ex)
            {
                Log.LogError($"[ExtendedWhips] Failed to hot-patch GameAssembly.dll: {ex.Message}");
            }
        }
    }

    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "com.extendedwhips.consim";
        public const string PLUGIN_NAME = "ExtendedWhips";
        public const string PLUGIN_VERSION = "1.3.0";
    }
}
