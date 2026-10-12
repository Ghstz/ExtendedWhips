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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        public static void LogMsg(string msg)
        {
            Instance?.Log.LogInfo(msg);
        }

        private static readonly System.Collections.Generic.HashSet<string> _loggedOnce = new();

        /// <summary>Logs an exception once per unique type+message instead of swallowing it silently every frame.</summary>
        public static void LogOnce(Exception ex)
        {
            string key = ex.GetType().Name + ":" + ex.Message;
            if (_loggedOnce.Add(key)) Instance?.Log.LogWarning("[ExtendedWhips] Suppressed repeating error: " + ex);
        }

        /// <summary>True when gameplay hotkeys should be ignored (text field focused or photo mode active).</summary>
        public static bool InputBlocked => GUIUtility.keyboardControl != 0 || Cockpit.PhotoModeFreecam.IsPhotoModeActive;

        private static readonly KeyCode[] _latchedKeys = { KeyCode.J, KeyCode.H, KeyCode.L };
        private static readonly float[] _latchTimes = { -1f, -1f, -1f };
        private static int _lastPumpFrame = -1;

        /// <summary>Call once per rendered frame. Returns false if already pumped this frame.</summary>
        public static bool PumpInput()
        {
            int frame = Time.frameCount;
            if (frame == _lastPumpFrame) return false;
            _lastPumpFrame = frame;
            for (int i = 0; i < _latchedKeys.Length; i++)
            {
                if (Input.GetKeyDown(_latchedKeys[i])) _latchTimes[i] = Time.unscaledTime;
            }
            return true;
        }

        /// <summary>True once per physical key press (within 0.25 s), safe to call from physics-step code.</summary>
        public static bool ConsumeKeyDown(KeyCode key)
        {
            for (int i = 0; i < _latchedKeys.Length; i++)
            {
                if (_latchedKeys[i] != key) continue;
                bool hit = _latchTimes[i] >= 0f && Time.unscaledTime - _latchTimes[i] <= 0.25f;
                _latchTimes[i] = -1f;
                return hit;
            }
            return false;
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

                // If already patched with our trampoline jump (starts with 0xE9), skip
                if (current[0] == 0xE9)
                {
                    Log.LogInfo("[ExtendedWhips] GameAssembly.dll RVA 0x25B831 is already patched with trampoline.");
                    return;
                }

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
                    Log.LogWarning($"[ExtendedWhips] GameAssembly bytes at 0x25B831 ({BitConverter.ToString(current)}) did not match expected pattern.");
                    return;
                }

                // Allocate 4KB executable page within +/- 2GB range for 32-bit relative jump
                IntPtr trampoline = IntPtr.Zero;
                for (long offset = 0x100000; offset < 0x70000000; offset += 0x10000)
                {
                    IntPtr candidate = (IntPtr)((long)patchAddress + offset);
                    trampoline = VirtualAlloc(candidate, (UIntPtr)4096, 0x1000 | 0x2000, 0x40);
                    if (trampoline != IntPtr.Zero) break;
                }
                if (trampoline == IntPtr.Zero)
                {
                    for (long offset = 0x100000; offset < 0x70000000; offset += 0x10000)
                    {
                        IntPtr candidate = (IntPtr)((long)patchAddress - offset);
                        trampoline = VirtualAlloc(candidate, (UIntPtr)4096, 0x1000 | 0x2000, 0x40);
                        if (trampoline != IntPtr.Zero) break;
                    }
                }

                if (trampoline == IntPtr.Zero)
                {
                    Log.LogError("[ExtendedWhips] Failed to allocate trampoline memory near GameAssembly.dll.");
                    return;
                }

                // Target RVAs in GameAssembly.dll:
                // 0x25B882: Return epilogue (xor eax, eax; mov rbx, [rsp+60h]; ... ret)
                // 0x25B848: Fast path (test byte ptr [rbx+44h], 10h; ... call rax)
                // 0x25B837: Slow path (mov rdx, rbx; lea rcx, [rsp+28h]; ...)
                IntPtr targetRet = IntPtr.Add(hModule, 0x25B882);
                IntPtr targetFast = IntPtr.Add(hModule, 0x25B848);
                IntPtr targetSlow = IntPtr.Add(hModule, 0x25B837);

                // Build Trampoline (24 bytes):
                // 0x00: test rbx, rbx          (48 85 DB)
                // 0x03: jz targetRet           (0F 84 rel32)
                // 0x09: cmp qword ptr [rbx], 0 (48 83 3B 00)
                // 0x0D: jne targetFast         (0F 85 rel32)
                // 0x13: jmp targetSlow         (E9 rel32)
                int relRet = (int)((long)targetRet - ((long)trampoline + 9));
                int relFast = (int)((long)targetFast - ((long)trampoline + 19));
                int relSlow = (int)((long)targetSlow - ((long)trampoline + 24));

                byte[] code = new byte[24];
                code[0] = 0x48; code[1] = 0x85; code[2] = 0xDB;
                code[3] = 0x0F; code[4] = 0x84;
                BitConverter.GetBytes(relRet).CopyTo(code, 5);
                code[9] = 0x48; code[10] = 0x83; code[11] = 0x3B; code[12] = 0x00;
                code[13] = 0x0F; code[14] = 0x85;
                BitConverter.GetBytes(relFast).CopyTo(code, 15);
                code[19] = 0xE9;
                BitConverter.GetBytes(relSlow).CopyTo(code, 20);

                Marshal.Copy(code, 0, trampoline, 24);

                // Build hook at 0x25B831 (6 bytes):
                // jmp trampoline (E9 rel32) + nop (90)
                int relTrampoline = (int)((long)trampoline - ((long)patchAddress + 5));
                byte[] hook = new byte[6];
                hook[0] = 0xE9;
                BitConverter.GetBytes(relTrampoline).CopyTo(hook, 1);
                hook[5] = 0x90;

                // PAGE_EXECUTE_READWRITE = 0x40
                if (!VirtualProtect(patchAddress, (UIntPtr)8, 0x40, out uint oldProtect))
                {
                    Log.LogError("[ExtendedWhips] VirtualProtect failed to set memory protection for patch.");
                    return;
                }

                bool patched = false;
                bool atomicPossible = false;
                unsafe
                {
                    long addr = (long)patchAddress;
                    long aligned = addr & ~7L;
                    int shift = (int)(addr - aligned);
                    atomicPossible = shift + 6 <= 8;
                    if (atomicPossible)
                    {
                        // Entire 6-byte site lives in one aligned qword -> swap it atomically.
                        long* p = (long*)aligned;
                        long mask = 0xFFFFFFFFFFFFL << (shift * 8);
                        long hookBits = 0;
                        for (int i = 0; i < 6; i++) hookBits |= (long)hook[i] << ((shift + i) * 8);

                        for (int attempt = 0; attempt < 4 && !patched; attempt++)
                        {
                            long orig = System.Threading.Volatile.Read(ref *p);
                            long expectedBits = 0;
                            for (int i = 0; i < 6; i++) expectedBits |= (long)expected[i] << ((shift + i) * 8);
                            if ((orig & mask) != expectedBits) break; // someone else changed it; bail out to be safe
                            long updated = (orig & ~mask) | hookBits;
                            patched = System.Threading.Interlocked.CompareExchange(ref *p, updated, orig) == orig;
                        }
                    }
                }

                if (!patched && !atomicPossible) Marshal.Copy(hook, 0, patchAddress, 6); // non-atomic fallback (unaligned site only)

                VirtualProtect(patchAddress, (UIntPtr)8, oldProtect, out _);
                FlushInstructionCache(GetCurrentProcess(), patchAddress, (UIntPtr)8);

                Log.LogInfo($"[ExtendedWhips] Successfully hot-patched GameAssembly.dll RVA 0x25B831 via trampoline at 0x{trampoline.ToInt64():X} (atomic={patched})!");
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
        public const string PLUGIN_VERSION = "2.1.2";
    }
}
