using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using RootMotion;
using ExtendedWhips.Hooks;
using ExtendedWhips.Tuning;
using ExtendedWhips.Spawning;
using ExtendedWhips.Models;
using ExtendedWhips.Effects;
using ExtendedWhips.Cockpit;

namespace ExtendedWhips.UI
{
    [HarmonyPatch(typeof(DemoGUIMessage), nameof(DemoGUIMessage.OnGUI))]
    public static class DemoGuiMessagePatch
    {
        private static int _loggedPumps = 0;

        public static bool Prefix(DemoGUIMessage __instance)
        {
            if (__instance == null || __instance.gameObject == null || __instance.gameObject.name != "ExtendedWhips_GUIHost")
            {
                return true; // Let original game DemoGUIMessage run normally
            }

            if (_loggedPumps++ < 3)
            {
                Plugin.LogMsg($"[ExtendedWhips] IMGUI host OnGUI active (Frame={Time.frameCount}, Event={Event.current?.type})");
            }

            try
            {
                // Once per rendered frame (OnGUI fires several times per frame), independent of physics step / timeScale
                if (Plugin.PumpInput())
                {
                    VehicleTunerHook.HandleInput();
                    PhotoModeFreecam.UpdateFreecam();
                }
            }
            catch (Exception ex_) { Plugin.LogOnce(ex_); }

            try
            {
                ExtendedWhipsUI.OnRenderGUI();
            }
            catch (Exception ex)
            {
                Plugin.LogMsg($"[ExtendedWhips] IMGUI Render Error: {ex.Message}");
            }
            return false; // Skip DemoGUIMessage default label drawing
        }
    }

    public static class ExtendedWhipsUI
    {
        // UI State
        public static bool IsMenuOpen { get; private set; } = false;
        public static bool IsHudVisible { get; set; } = true;

        private static bool _pendingMenuToggle = false;
        private static bool _pendingMenuClose = false;
        private static bool _pendingHudToggle = false;
        private static int _pendingTab = -1;

        private static bool _hudActiveThisFrame = false;
        private static bool _toastActiveThisFrame = false;

        private static Rect _menuRect = new Rect(40, 30, 680, 620);
        private static int _selectedTab = 0;
        private static Vector2 _scrollPosition = Vector2.zero;

        private static readonly string[] _tabRow1 = { "[PRESETS]", "[ENGINE]", "[GEARING & DIFF]", "[SUSPENSION]" };
        private static readonly string[] _tabRow2 = { "[TIRES & ALIGN]", "[BRAKES & CHASSIS]", "[TELEMETRY]", "[WHIPS]" };
        private static string _shareCodeInput = "";

        // Toast notification state
        private static string _toastTitle = "";
        private static string _toastMessage = "";
        private static float _toastExpireTime = 0f;

        public static void ShowToast(string title, string message, float duration = 3.5f)
        {
            _toastTitle = title;
            _toastMessage = message;
            _toastExpireTime = Time.time + duration;
        }

        public static void RequestToggleMenu() => _pendingMenuToggle = true;
        public static void RequestCloseMenu() => _pendingMenuClose = true;
        public static void RequestToggleHud() => _pendingHudToggle = true;
        public static void RequestTab(int tabIndex) => _pendingTab = tabIndex;

        public static void ToggleMenu() => RequestToggleMenu();
        public static void ToggleHud() => RequestToggleHud();

        public static void OnRenderGUI()
        {
            if (Event.current == null) return;

            // Synchronize state transitions exclusively at the start of Layout passes
            // This guarantees 100% identical control hierarchy between Layout and Repaint passes.
            if (Event.current.type == EventType.Layout)
            {
                if (_pendingMenuToggle)
                {
                    IsMenuOpen = !IsMenuOpen;
                    _pendingMenuToggle = false;
                    try
                    {
                        Cursor.visible = IsMenuOpen;
                        Cursor.lockState = IsMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
                    }
                    catch (System.Exception ex_) { Plugin.LogOnce(ex_); }
                    Plugin.LogMsg($"[ExtendedWhips] Menu toggled on Layout pass: IsMenuOpen = {IsMenuOpen}");
                }
                else if (_pendingMenuClose)
                {
                    IsMenuOpen = false;
                    _pendingMenuClose = false;
                    try
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                    catch (System.Exception ex_) { Plugin.LogOnce(ex_); }
                    Plugin.LogMsg("[ExtendedWhips] Menu closed on Layout pass");
                }

                if (_pendingHudToggle)
                {
                    IsHudVisible = !IsHudVisible;
                    _pendingHudToggle = false;
                    ShowToast("HUD DISPLAY", IsHudVisible ? "Live Telemetry Overlay: ON" : "Live Telemetry Overlay: OFF", 2.0f);
                }

                if (_pendingTab >= 0)
                {
                    _selectedTab = _pendingTab;
                    _pendingTab = -1;
                }

                _hudActiveThisFrame = IsHudVisible && (VehicleTunerHook.CurrentVehicle != null);
                _toastActiveThisFrame = (Time.time <= _toastExpireTime && !string.IsNullOrEmpty(_toastTitle));
            }

            // Escape closes menu
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && IsMenuOpen)
            {
                RequestCloseMenu();
                Event.current.Use();
                return;
            }

            // Draw Toast Banner
            try { DrawToast(); } catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            // Draw On-Screen HUD Overlay
            try { DrawHud(); } catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            // Draw Configuration Menu Window
            if (IsMenuOpen)
            {
                try { DrawMenuWindow(); } catch (System.Exception ex_) { Plugin.LogOnce(ex_); }
            }

            // Draw Photo Mode GUI Overlay
            try { PhotoModeFreecam.DrawPhotoModeGui(); } catch (System.Exception ex_) { Plugin.LogOnce(ex_); }
        }

        // ==========================================
        // 1. FLOATING TOAST NOTIFICATION
        // ==========================================
        private static void DrawToast()
        {
            if (!_toastActiveThisFrame) return;

            float width = 480f;
            float height = 65f;
            float x = (Screen.width - width) / 2f;
            float y = 20f;

            GUI.backgroundColor = new Color(0.04f, 0.07f, 0.12f, 0.95f);
            GUI.Box(new Rect(x, y, width, height), "");

            // Cyan left accent bar
            GUI.backgroundColor = new Color(0.0f, 0.85f, 1.0f, 1.0f);
            GUI.Box(new Rect(x, y, 5, height), "");

            GUILayout.BeginArea(new Rect(x + 15, y + 8, width - 25, height - 16));
            try
            {
                GUILayout.Label($"<b><size=14><color=#00E5FF>>> {_toastTitle}</color></size></b>");
                GUILayout.Label($"<size=12><color=#F0F0F0>{_toastMessage}</color></size>");
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        // ==========================================
        // 2. LIVE ON-SCREEN TELEMETRY HUD
        // ==========================================
        private static void DrawHud()
        {
            if (!_hudActiveThisFrame) return;
            var v = VehicleTunerHook.CurrentVehicle;
            if (v == null) return;

            float width = 330f;
            float height = 255f;
            float x = Screen.width - width - 20f;
            float y = 20f;

            GUI.backgroundColor = new Color(0.05f, 0.07f, 0.10f, 0.88f);
            GUI.Box(new Rect(x, y, width, height), "");

            // Top cyan line
            GUI.backgroundColor = new Color(0.0f, 0.85f, 1.0f, 0.9f);
            GUI.Box(new Rect(x, y, width, 3), "");

            GUILayout.BeginArea(new Rect(x + 12, y + 8, width - 24, height - 16));
            try
            {
                // Header Row: Vehicle name & Config button
                GUILayout.BeginHorizontal();
                string vehName = v.transform.root != null ? v.transform.root.name : v.gameObject.name;
                if (vehName.Length > 16) vehName = vehName.Substring(0, 16) + "...";
                GUILayout.Label($"<b><color=#00E5FF>WHIPS</color> // <color=#FFD700>{vehName}</color></b>");
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = IsMenuOpen ? new Color(1.0f, 0.3f, 0.3f, 1.0f) : new Color(0.0f, 0.6f, 0.9f, 1.0f);
                if (GUILayout.Button(IsMenuOpen ? "[Close]" : "[Menu F4]", GUILayout.Width(75), GUILayout.Height(22)))
                {
                    RequestToggleMenu();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(2);

                // Mode Badge
                string presetColor = VehicleTunerHook.CurrentPreset switch
                {
                    TuningPreset.Stock => "#AAAAAA",
                    TuningPreset.StreetPerformance => "#00FF88",
                    TuningPreset.OffroadLifted => "#FFAA00",
                    TuningPreset.DriftSpec => "#00E5FF",
                    TuningPreset.RallyCross => "#FFDD00",
                    TuningPreset.HyperInsane => "#FF00FF",
                    _ => "#00FFCC"
                };
                string presetName = VehicleTunerHook.CurrentPreset switch
                {
                    TuningPreset.Stock => "FACTORY STOCK",
                    TuningPreset.StreetPerformance => "STREET PERFORMANCE",
                    TuningPreset.OffroadLifted => "OFFROAD CRAWLER",
                    TuningPreset.DriftSpec => "DRIFT SPEC",
                    TuningPreset.RallyCross => "RALLY CROSS",
                    TuningPreset.HyperInsane => "HYPER OVERKILL",
                    _ => "CUSTOM TUNED"
                };
                if (EvPowertrainSystem.IsEvSwapActive) presetName += " | âš¡ EV CYBER";
                GUILayout.Label($"<b>Mode: <color={presetColor}>[{presetName}]</color></b>");

                // Speed & Gear
                float speedKmh = Mathf.Abs(v.speed * 3.6f);
                float speedMph = Mathf.Abs(v.speed * 2.23694f);
                int gear = 0;
                float rpm = 0f;
                try
                {
                    if (v.data != null)
                    {
                        rpm = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.EngineRpm) / 1000f;
                        gear = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.GearboxGear);
                    }
                }
                catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

                string gearStr = gear switch
                {
                    0 => "N",
                    -1 => "R",
                    _ => $"D{gear}"
                };

                GUILayout.Label($"<b>Speed:</b> {speedKmh:F0} km/h ({speedMph:F0} mph) | <b>Gear:</b> <color=#00FF88>{gearStr}</color>");

                // RPM Gauge with ASCII progress bar
                float maxRpm = v.engine != null ? v.engine.maxRpm : 3500f;
                float rpmRatio = Mathf.Clamp01(rpm / Mathf.Max(maxRpm, 1f));
                int totalBars = 16;
                int filledBars = Mathf.RoundToInt(rpmRatio * totalBars);
                string barColor = rpmRatio > 0.85f ? "#FF3333" : rpmRatio > 0.65f ? "#FFAA00" : "#00FF88";
                string bar = new string('=', filledBars) + new string('.', totalBars - filledBars);
                GUILayout.Label($"<b>RPM:</b> {rpm:F0} / {maxRpm:F0} <color={barColor}>[{bar}]</color>");

                // Nitrous Bottle Gauge
                float nosPsi = CustomVehicleNitrous.BottlePsi;
                float nosRatio = Mathf.Clamp01(nosPsi / CustomVehicleNitrous.MaxPsi);
                int nosBars = 10;
                int filledNos = Mathf.RoundToInt(nosRatio * nosBars);
                string nosBarStr = new string('=', filledNos) + new string('.', nosBars - filledNos);
                string nosStatus = CustomVehicleNitrous.IsBoosting ? "<color=#00E5FF><b>BOOST ACTIVE!</b></color>" :
                                   CustomVehicleNitrous.IsPurging ? "<color=#FFFFFF><b>PURGING CO2</b></color>" :
                                   nosPsi > 100f ? "<color=#00FF88>ARMED [LShift]</color>" : "<color=#FF3333>EMPTY</color>";
                GUILayout.Label($"<b>NOS:</b> {nosPsi:F0} PSI <color=#00E5FF>[{nosBarStr}]</color> {nosStatus}");

                // Torque & Steer calculations
                float torque = v.engine != null ? v.engine.peakRpmTorque : 0f;
                float steerAngle = v.steering != null ? v.steering.maxSteerAngle : 35f;
                float estHp = torque * (rpm * 0.1047f) / 745.7f;
                if (estHp < 0) estHp = 0;

                // System Active Indicators
                if (LaunchControlSystem.IsLaunchControlActive)
                {
                    GUILayout.Label("<b><color=#FF0055>>> [2-STEP LAUNCH ARMED: 4,500 RPM]</color></b>");
                }
                else if (CustomEmergencyLighting.IsStrobesActive)
                {
                    string sirenTxt = CustomEmergencyLighting.IsSirenActive ? " + SIREN [H]" : "";
                    GUILayout.Label($"<b><color=#FFAA00>>> [ESCORT STROBES ACTIVE{sirenTxt}]</color></b>");
                }
                else if (InCabinStereo.IsPlaying)
                {
                    GUILayout.Label($"<b><color=#00FFCC>â™« {InCabinStereo.CurrentTrackTitle}</color></b>");
                }
                else
                {
                    GUILayout.Label($"<b>Torque:</b> {torque:N0} Nm ({VehicleTunerHook.TorqueMultiplier:F1}x) ~ {estHp:N0} HP");
                }
                GUILayout.Label($"<b>Steer Lock:</b> {steerAngle:F0} deg | <b>Final Drive:</b> {(v.differential != null ? v.differential.gearRatio : 3.5f):F2}");
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        // ==========================================
        // 3. FULL VEHICLE CONFIGURATION MENU WINDOW
        // ==========================================
        private static bool _loggedMenuWindow = false;
        private static void DrawMenuWindow()
        {
            if (!_loggedMenuWindow)
            {
                Plugin.LogMsg("[ExtendedWhips] DrawMenuWindow active and rendering!");
                _loggedMenuWindow = true;
            }

            var v = VehicleTunerHook.CurrentVehicle;
            var stock = VehicleTunerHook.CurrentStockSpecs;

            GUI.backgroundColor = new Color(0.05f, 0.07f, 0.11f, 0.98f);
            GUILayout.BeginArea(_menuRect, GUI.skin.box);
            try
            {
                // 1. Header row
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b><color=#00E5FF><size=15>EXTENDED WHIPS</size></color> // <color=#FFFFFF>FORZA VEHICLE TUNING SUITE</color></b>");
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f, 1.0f);
                if (GUILayout.Button("[X]", GUILayout.Width(35), GUILayout.Height(22)))
                {
                    RequestCloseMenu();
                }
                GUILayout.EndHorizontal();

                // 2. Subheader vehicle info
                string vehRoot = v != null && v.transform.root != null ? v.transform.root.name : "No Vehicle Detected";
                float curTorque = v != null && v.engine != null ? v.engine.peakRpmTorque : 0;
                float stockTorque = stock != null ? stock.PeakTorque : 0;
                GUILayout.Label($"<b>Vehicle:</b> <color=#FFD700>{vehRoot}</color> | <b>Torque:</b> {curTorque:N0} Nm (Stock: {stockTorque:N0} Nm)");

                GUILayout.Space(2);

                // 3. Tab Bar (Two clean rows for full accessibility)
                GUILayout.BeginHorizontal();
                for (int i = 0; i < _tabRow1.Length; i++)
                {
                    GUI.backgroundColor = _selectedTab == i ? new Color(0.0f, 0.85f, 1.0f, 1.0f) : new Color(0.14f, 0.18f, 0.24f, 1.0f);
                    if (GUILayout.Button(_tabRow1[i], GUILayout.Height(26)))
                    {
                        RequestTab(i);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                for (int i = 0; i < _tabRow2.Length; i++)
                {
                    int tabIndex = i + 4;
                    GUI.backgroundColor = _selectedTab == tabIndex ? new Color(0.0f, 0.85f, 1.0f, 1.0f) : new Color(0.14f, 0.18f, 0.24f, 1.0f);
                    if (GUILayout.Button(_tabRow2[i], GUILayout.Height(26)))
                    {
                        RequestTab(tabIndex);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(6);

                // 4. Tab Body inside ScrollView
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(430));
                try
                {
                    switch (_selectedTab)
                    {
                        case 0: DrawPresetsTab(); break;
                        case 1: DrawEngineTab(v, stock); break;
                        case 2: DrawGearingTab(v, stock); break;
                        case 3: DrawSuspensionTab(v, stock); break;
                        case 4: DrawTiresAlignTab(v, stock); break;
                        case 5: DrawBrakesChassisTab(v, stock); break;
                        case 6: DrawTelemetryTab(v, stock); break;
                        case 7: DrawWhipsTab(); break;
                    }
                }
                finally
                {
                    GUILayout.EndScrollView();
                }

                GUILayout.Space(6);

                // 5. Footer Actions Bar
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.7f, 0.3f, 0.1f, 1.0f);
                if (GUILayout.Button("Reset Stock [F8]", GUILayout.Height(28)))
                {
                    VehicleTunerHook.ResetToStock(showToast: true);
                }

                GUI.backgroundColor = new Color(0.15f, 0.2f, 0.28f, 1.0f);
                if (GUILayout.Button(IsHudVisible ? "Hide HUD [F6]" : "Show HUD [F6]", GUILayout.Height(28)))
                {
                    RequestToggleHud();
                }

                GUI.backgroundColor = new Color(0.1f, 0.5f, 0.8f, 1.0f);
                if (GUILayout.Button("Close Menu [F4]", GUILayout.Height(28)))
                {
                    RequestCloseMenu();
                }
                GUILayout.EndHorizontal();
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        // ==========================================
        // TAB 0: PRESETS
        // ==========================================
        private static void DrawPresetsTab()
        {
            GUILayout.Label("<b><color=#00E5FF>Quick Vehicle Tuning Presets:</color></b>");
            GUILayout.Label("Choose a curated professional tune or build your own custom setup in the tabs above.");
            GUILayout.Space(6);

            // 1. Stock OEM
            GUI.backgroundColor = new Color(0.2f, 0.25f, 0.3f, 1.0f);
            if (GUILayout.Button("[ FACTORY STOCK (OEM Default) ]\nRestore 1.0x torque, factory redline, stock suspension height, OEM gearing and brakes.", GUILayout.Height(42)))
            {
                VehicleTunerHook.ResetToStock(showToast: true);
            }
            GUILayout.Space(6);

            // 2. Street Performance
            GUI.backgroundColor = new Color(0.0f, 0.6f, 0.35f, 1.0f);
            if (GUILayout.Button("[ STREET PERFORMANCE ]\n2.5x Torque | 1.35x RPM | Lowered Stance (-5cm) | 1.8x Springs | 2.0x Brakes (65/35 Bias) | Aero Downforce", GUILayout.Height(42)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.StreetPerformance, showToast: true);
            }
            GUILayout.Space(6);

            // 3. Drift Spec
            GUI.backgroundColor = new Color(0.0f, 0.5f, 0.75f, 1.0f);
            if (GUILayout.Button("[ DRIFT SPEC ]\n3.2x Torque | 65 deg Drift Angle Kit | 100% Locked Spool Diff | 5.0x Instant E-Brake | Slipperier Rear Tires | Assists OFF", GUILayout.Height(42)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.DriftSpec, showToast: true);
            }
            GUILayout.Space(6);

            // 4. Offroad Crawler
            GUI.backgroundColor = new Color(0.75f, 0.45f, 0.05f, 1.0f);
            if (GUILayout.Button("[ OFFROAD HEAVY CRAWLER ]\n3.8x Crawl Torque | +22cm Lifted Suspension | 1.35x Giant Mud Tires | 100% 4x4 Locked Diffs | Soft Long-Travel Springs", GUILayout.Height(42)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.OffroadLifted, showToast: true);
            }
            GUILayout.Space(6);

            // 5. Rally Cross
            GUI.backgroundColor = new Color(0.8f, 0.6f, 0.1f, 1.0f);
            if (GUILayout.Button("[ RALLY CROSS ]\n2.8x Torque | +6cm Rally Clearance | Heavy Dampers | 55 deg Steer Lock | 50/50 Brake Balance | Loose Surface Traction", GUILayout.Height(42)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.RallyCross, showToast: true);
            }
            GUILayout.Space(6);

            // 6. Hyper Overkill
            GUI.backgroundColor = new Color(0.6f, 0.1f, 0.6f, 1.0f);
            if (GUILayout.Button("[ HYPER MISSILE ]\n5.5x Monster Torque | 2.0x Redline | 300+ km/h Gearing | Slammed Stance (-10cm) | High-Speed Downforce Suction", GUILayout.Height(42)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.HyperInsane, showToast: true);
            }
        }

        // ==========================================
        // TAB 1: ENGINE & POWERTRAIN
        // ==========================================
        private static void DrawEngineTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><color=#00E5FF>Engine & Powertrain Tuning:</color></b>");
            GUILayout.Space(4);

            var p = VehicleTunerHook.Profile;
            float stockTorque = stock?.PeakTorque ?? 2600f;
            float calcTorque = stockTorque * p.TorqueMultiplier;

            // 1. Peak Torque
            p.TorqueMultiplier = DrawSliderWithNudge("Peak Torque Output", p.TorqueMultiplier, 0.2f, 8.0f, "F1", "x", 0.1f,
                $"{calcTorque:N0} Nm (Stock: {stockTorque:N0} Nm)");

            GUILayout.Space(8);

            // 2. Idle Crawl Torque
            float stockIdle = stock?.IdleTorque ?? 600f;
            float calcIdle = stockIdle * p.IdleTorqueMultiplier;
            p.IdleTorqueMultiplier = DrawSliderWithNudge("Low-End Crawl Pull", p.IdleTorqueMultiplier, 0.2f, 5.0f, "F1", "x", 0.1f,
                $"{calcIdle:N0} Nm (Heavy Trailer & Rock Crawling)");

            GUILayout.Space(8);

            // 3. RPM Limiter / Redline
            float stockRpm = stock?.MaxRpm ?? 3500f;
            float calcRpm = stockRpm * p.RpmMultiplier;
            p.RpmMultiplier = DrawSliderWithNudge("RPM Limiter / Redline", p.RpmMultiplier, 1.0f, 2.5f, "F2", "x", 0.05f,
                $"{calcRpm:N0} RPM (Stock: {stockRpm:N0} RPM)");

            GUILayout.Space(8);

            // 4. Idle RPM
            float stockIdleRpm = stock?.IdleRpm ?? 700f;
            float calcIdleRpm = stockIdleRpm * p.IdleRpmMultiplier;
            p.IdleRpmMultiplier = DrawSliderWithNudge("Engine Idle Speed", p.IdleRpmMultiplier, 0.5f, 2.5f, "F2", "x", 0.05f,
                $"{calcIdleRpm:N0} RPM (Stock: {stockIdleRpm:N0} RPM)");

            GUILayout.Space(8);

            // 5. Flywheel Inertia (Rev Speed)
            p.FlywheelInertiaMultiplier = DrawSliderWithNudge("Flywheel Inertia (Rev Speed)", p.FlywheelInertiaMultiplier, 0.15f, 3.0f, "F2", "x", 0.05f,
                p.FlywheelInertiaMultiplier < 0.5f ? "Lightweight Racing (Snap Revs)" : p.FlywheelInertiaMultiplier > 1.5f ? "Heavy Industrial Flywheel" : "OEM Balanced");

            GUILayout.Space(8);

            // 6. Engine Compression Braking
            p.CompressionBrakingMultiplier = DrawSliderWithNudge("Compression / Jake Brake", p.CompressionBrakingMultiplier, 0.1f, 4.0f, "F1", "x", 0.1f,
                p.CompressionBrakingMultiplier < 0.5f ? "Free Coasting (Minimal Drag)" : "Heavy Decel Retarder Drag");

            GUILayout.Space(8);

            // 7. Instant EV Powertrain & Cyber Sound Swap
            GUI.backgroundColor = EvPowertrainSystem.IsEvSwapActive ? new Color(0.0f, 0.9f, 0.8f, 1.0f) : new Color(0.2f, 0.3f, 0.4f, 1.0f);
            if (GUILayout.Button(EvPowertrainSystem.IsEvSwapActive ? "[ âš¡ 1,500 HP CYBER EV SWAP: ACTIVE ]" : "[ âš¡ SWAP TO 1,500 HP CYBER EV MOTOR ]", GUILayout.Height(30)))
            {
                EvPowertrainSystem.IsEvSwapActive = !EvPowertrainSystem.IsEvSwapActive;
                ShowToast("POWERTRAIN SWAP", EvPowertrainSystem.IsEvSwapActive ? "Installed Instant-Torque 1,500 HP Cyber EV Motor & Inverter" : "Restored Internal Combustion Engine");
            }
        }

        // ==========================================
        // TAB 2: GEARING & TRANSMISSION
        // ==========================================
        private static void DrawGearingTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><color=#00E5FF>Transmission, Gearing & Differential (LSD):</color></b>");
            GUILayout.Space(4);

            var p = VehicleTunerHook.Profile;
            float stockFinal = stock?.FinalDriveRatio ?? 3.5f;
            float curFinal = stockFinal * p.FinalDriveMultiplier;

            // 1. Final Drive Ratio
            p.FinalDriveMultiplier = DrawSliderWithNudge("Final Drive Gear Ratio", p.FinalDriveMultiplier, 0.50f, 2.50f, "F2", "x", 0.05f,
                $"{curFinal:F2}:1 (Shorter Acceleration <--> Taller Top Speed)");

            GUILayout.Space(8);

            // 2. Shift Duration
            p.ShiftSpeedMultiplier = DrawSliderWithNudge("Gear Shift Duration", p.ShiftSpeedMultiplier, 0.10f, 2.00f, "F2", "x", 0.05f,
                p.ShiftSpeedMultiplier <= 0.20f ? "Dogbox / Dual-Clutch (Instant ~25ms)" : p.ShiftSpeedMultiplier >= 1.5f ? "Heavy Truck Shift" : "OEM Shift Time");

            GUILayout.Space(8);

            // 3. Clutch Torque Capacity
            float stockClutch = stock?.ClutchMaxTorque ?? 6000f;
            p.ClutchTorqueMultiplier = DrawSliderWithNudge("Clutch Torque Capacity", p.ClutchTorqueMultiplier, 0.5f, 5.0f, "F1", "x", 0.1f,
                $"{stockClutch * p.ClutchTorqueMultiplier:N0} Nm (Prevents Slipping on High Torque)");

            GUILayout.Space(8);

            // 4. Differential Accel Lock %
            p.DiffPowerLock = DrawSliderWithNudge("LSD Acceleration Lock", p.DiffPowerLock, 0.0f, 1.0f, "P0", "", 0.05f,
                p.DiffPowerLock >= 0.95f ? "100% Welded Spool (Max Slide)" : p.DiffPowerLock <= 0.10f ? "Open Differential" : "Limited Slip");

            GUILayout.Space(8);

            // 5. Differential Decel Lock %
            p.DiffCoastLock = DrawSliderWithNudge("LSD Deceleration Lock", p.DiffCoastLock, 0.0f, 1.0f, "P0", "", 0.05f,
                p.DiffCoastLock >= 0.80f ? "Locked on Braking (Straight Stability)" : "Free Turn-in (Agile Entry)");

            GUILayout.Space(8);

            // 6. Differential Preload
            p.DiffPreload = DrawSliderWithNudge("Diff Preload Torque", p.DiffPreload, 0f, 300f, "F0", " Nm", 10f,
                $"{p.DiffPreload:F0} Nm Initial Clamping Force");
        }

        // ==========================================
        // TAB 3: SUSPENSION & STANCE
        // ==========================================
        private static void DrawSuspensionTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><color=#00E5FF>Suspension Stance, Springs & Wheel Geometry:</color></b>");
            GUILayout.Space(4);

            var p = VehicleTunerHook.Profile;

            // 1. Ride Height
            float cm = p.RideHeightOffset * 100f;
            string rideInfo = cm < -5f ? $"{cm:F0} cm (Slammed Track Stance)" : cm > 10f ? $"+{cm:F0} cm (Lifted Monster Hauler)" : $"{cm:F0} cm (Near OEM)";
            if (p.RideHeightOffset < 0f && stock != null && stock.Wheels.Count > 0)
            {
                float effective = VehicleTunerHook.ResolveRideHeightOffset(VehicleReskinManager.GetActiveSkin(v), stock.Wheels[0].SuspensionDistance);
                if (effective > p.RideHeightOffset + 0.0005f)
                {
                    rideInfo = $"{cm:F0} cm requested, limited to {effective * 100f:F1} cm (bump-stop safe)";
                }
            }
            p.RideHeightOffset = DrawSliderWithNudge("Ride Height Offset", p.RideHeightOffset, -0.20f, 0.35f, "F2", " m", 0.02f, rideInfo);

            GUILayout.Space(8);

            // 2. Spring Stiffness
            p.SpringStiffnessMultiplier = DrawSliderWithNudge("Spring Stiffness Rate", p.SpringStiffnessMultiplier, 0.2f, 4.0f, "F2", "x", 0.1f,
                p.SpringStiffnessMultiplier > 2.0f ? "Rock-Solid Track Stiffness" : p.SpringStiffnessMultiplier < 0.6f ? "Plush Soft Crawling Travel" : "Balanced Stride");

            GUILayout.Space(8);

            // 3. Damper Rebound / Bump
            p.DamperMultiplier = DrawSliderWithNudge("Damper Rebound / Bump Rate", p.DamperMultiplier, 0.2f, 3.5f, "F2", "x", 0.1f,
                p.DamperMultiplier > 1.8f ? "Firm Race Damping (Anti-Bounce)" : p.DamperMultiplier < 0.6f ? "Soft Float" : "OEM Damping");

            GUILayout.Space(8);

            // 4. Wheel & Tire Diameter
            p.TireSizeMultiplier = DrawSliderWithNudge("Wheel & Tire Diameter", p.TireSizeMultiplier, 0.75f, 1.50f, "F2", "x", 0.05f,
                p.TireSizeMultiplier < 0.9f ? "Low Profile Tires" : p.TireSizeMultiplier > 1.2f ? "Giant Offroad Mud Boggers" : "Stock Tire Size");

            GUILayout.Space(8);

            // 5. Wheel Mass
            p.WheelMassMultiplier = DrawSliderWithNudge("Wheel Unsprung Mass", p.WheelMassMultiplier, 0.5f, 2.5f, "F1", "x", 0.1f,
                p.WheelMassMultiplier < 0.8f ? "Lightweight Alloy Wheels" : "Heavy Duty Steelies");
        }

        // ==========================================
        // TAB 4: TIRES & ALIGNMENT
        // ==========================================
        private static void DrawTiresAlignTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><color=#00E5FF>Tires, Steering Geometry & Drift Alignment:</color></b>");
            GUILayout.Space(4);

            var p = VehicleTunerHook.Profile;
            float stockSteer = stock?.MaxSteerAngle ?? 35f;

            // 1. Max Steering Angle (Drift Lock)
            p.CustomSteerAngle = DrawSliderWithNudge("Max Steering Lock Angle", p.CustomSteerAngle, 20f, 75f, "F0", " deg", 1.0f,
                p.CustomSteerAngle >= 60f ? "Formula Drift Angle Kit (Extreme Lock)" : $"OEM: {stockSteer:F0} deg");

            GUILayout.Space(8);

            // 2. Toe Alignment
            p.SteerToeAngle = DrawSliderWithNudge("Steering Toe Alignment", p.SteerToeAngle, -5.0f, 5.0f, "F1", " deg", 0.2f,
                p.SteerToeAngle < -1f ? "Toe-Out (Aggressive Turn-in)" : p.SteerToeAngle > 1f ? "Toe-In (Highway Stability)" : "0 deg Neutral Alignment");

            GUILayout.Space(8);

            // 3. Ackermann Geometry
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Ackermann Steering:</b> <color=#00FF88>{(p.AckermanSteering ? "ON (Realistic Road Geometry)" : "OFF (Parallel Drift Angles)")}</color>", GUILayout.Width(360));
            GUI.backgroundColor = p.AckermanSteering ? new Color(0.2f, 0.7f, 0.4f) : new Color(0.7f, 0.4f, 0.1f);
            if (GUILayout.Button(p.AckermanSteering ? "Toggle [OFF]" : "Toggle [ON]", GUILayout.Width(100), GUILayout.Height(22)))
            {
                p.AckermanSteering = !p.AckermanSteering;
                VehicleTunerHook.MarkCustom();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            // 4. Forward Tire Grip
            p.TireForwardGripMultiplier = DrawSliderWithNudge("Longitudinal Traction Grip", p.TireForwardGripMultiplier, 0.5f, 3.0f, "F2", "x", 0.1f,
                p.TireForwardGripMultiplier > 2.0f ? "Drag Slicks (Zero Wheelspin)" : p.TireForwardGripMultiplier < 0.8f ? "Burnout Smoke" : "All-Season Street");

            GUILayout.Space(8);

            // 5. Lateral Drift Slip
            p.TireDriftSlipMultiplier = DrawSliderWithNudge("Lateral Drift / Slide Grip", p.TireDriftSlipMultiplier, 0.3f, 2.5f, "F2", "x", 0.1f,
                p.TireDriftSlipMultiplier < 0.7f ? "Drift Spec (Slides Effortlessly)" : p.TireDriftSlipMultiplier > 1.8f ? "Slot-Car Track Stiction" : "Normal Grip");

            GUILayout.Space(8);

            // 6. Formula Drift 70Â° Angle Kit & Gyro Drift Assist
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = DriftAssistSystem.IsAngleKitEnabled ? new Color(0.0f, 0.85f, 1.0f) : new Color(0.2f, 0.3f, 0.4f);
            if (GUILayout.Button(DriftAssistSystem.IsAngleKitEnabled ? "[ Wisefab 70Â° Angle Kit: ON ]" : "[ Wisefab 70Â° Angle Kit: OFF ]", GUILayout.Height(28)))
            {
                DriftAssistSystem.IsAngleKitEnabled = !DriftAssistSystem.IsAngleKitEnabled;
                if (DriftAssistSystem.IsAngleKitEnabled) p.CustomSteerAngle = 68f;
                ShowToast("ANGLE KIT", DriftAssistSystem.IsAngleKitEnabled ? "Installed Formula Drift 68Â°â€“70Â° Steering Angle Knuckles" : "Restored Standard Steering Lock");
            }

            GUI.backgroundColor = DriftAssistSystem.IsGyroAssistEnabled ? new Color(0.0f, 0.8f, 0.4f) : new Color(0.4f, 0.2f, 0.2f);
            if (GUILayout.Button(DriftAssistSystem.IsGyroAssistEnabled ? "[ Gyro Drift Assist: ON ]" : "[ Gyro Drift Assist: OFF ]", GUILayout.Height(28)))
            {
                DriftAssistSystem.IsGyroAssistEnabled = !DriftAssistSystem.IsGyroAssistEnabled;
            }
            GUILayout.EndHorizontal();
        }

        // ==========================================
        // TAB 5: BRAKES, ASSISTS & CHASSIS
        // ==========================================
        private static void DrawBrakesChassisTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><color=#00E5FF>Brakes, Driver Assists & Chassis Aerodynamics:</color></b>");
            GUILayout.Space(4);

            var p = VehicleTunerHook.Profile;
            float stockBrake = stock?.MaxBrakeTorque ?? 5000f;
            float calcBrake = stockBrake * p.BrakeTorqueMultiplier;

            // 1. Braking Force
            p.BrakeTorqueMultiplier = DrawSliderWithNudge("Master Braking Force", p.BrakeTorqueMultiplier, 0.5f, 4.0f, "F1", "x", 0.1f,
                $"{calcBrake:N0} Nm (Stock: {stockBrake:N0} Nm)");

            GUILayout.Space(8);

            // 2. Brake Bias
            float frontPct = p.BrakeBias * 100f;
            float rearPct = (1f - p.BrakeBias) * 100f;
            p.BrakeBias = DrawSliderWithNudge("Brake Balance Bias", p.BrakeBias, 0.20f, 0.85f, "P0", "", 0.05f,
                $"{frontPct:F0}% Front / {rearPct:F0}% Rear");

            GUILayout.Space(8);

            // 3. Handbrake Force
            float stockHb = stock?.HandbrakeTorque ?? 8000f;
            p.HandbrakeTorqueMultiplier = DrawSliderWithNudge("Handbrake / E-Brake Torque", p.HandbrakeTorqueMultiplier, 1.0f, 6.0f, "F1", "x", 0.2f,
                $"{stockHb * p.HandbrakeTorqueMultiplier:N0} Nm (Instant Rear Drift Lock)");

            GUILayout.Space(8);

            // 4. Driver Assists (ABS, TCS, ESP)
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Electronic Driver Assists:</b>", GUILayout.Width(200));

            GUI.backgroundColor = p.AbsEnabled ? new Color(0.1f, 0.7f, 0.3f) : new Color(0.6f, 0.2f, 0.2f);
            if (GUILayout.Button(p.AbsEnabled ? "ABS [ON]" : "ABS [OFF]", GUILayout.Width(85), GUILayout.Height(24)))
            {
                p.AbsEnabled = !p.AbsEnabled;
                VehicleTunerHook.MarkCustom();
            }

            GUI.backgroundColor = p.TcsEnabled ? new Color(0.1f, 0.7f, 0.3f) : new Color(0.6f, 0.2f, 0.2f);
            if (GUILayout.Button(p.TcsEnabled ? "TCS [ON]" : "TCS [OFF]", GUILayout.Width(85), GUILayout.Height(24)))
            {
                p.TcsEnabled = !p.TcsEnabled;
                VehicleTunerHook.MarkCustom();
            }

            GUI.backgroundColor = p.EspEnabled ? new Color(0.1f, 0.7f, 0.3f) : new Color(0.6f, 0.2f, 0.2f);
            if (GUILayout.Button(p.EspEnabled ? "ESP [ON]" : "ESP [OFF]", GUILayout.Width(85), GUILayout.Height(24)))
            {
                p.EspEnabled = !p.EspEnabled;
                VehicleTunerHook.MarkCustom();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            // 5. Chassis Curb Weight
            float stockMass = stock?.ChassisMass ?? 3500f;
            p.ChassisMassMultiplier = DrawSliderWithNudge("Chassis Curb Weight", p.ChassisMassMultiplier, 0.5f, 2.5f, "F2", "x", 0.05f,
                $"{stockMass * p.ChassisMassMultiplier:N0} kg (Lightweight Track <--> Heavy Hauler)");

            GUILayout.Space(8);

            // 6. Center of Gravity Y
            p.CenterOfMassYOffset = DrawSliderWithNudge("Center of Gravity Height", p.CenterOfMassYOffset, -0.60f, 0.40f, "F2", " m", 0.05f,
                p.CenterOfMassYOffset < -0.1f ? "Lowered CoG (Prevents Rollover)" : p.CenterOfMassYOffset > 0.1f ? "Raised CoG (High Center)" : "Stock CoG");

            GUILayout.Space(8);

            // 7. Aerodynamic Downforce
            p.DownforceMultiplier = DrawSliderWithNudge("Aerodynamic Downforce", p.DownforceMultiplier, 0.0f, 3.0f, "F1", "x", 0.1f,
                p.DownforceMultiplier > 1.5f ? "Formula 1 Suction (Glued at 150+ km/h)" : p.DownforceMultiplier > 0.1f ? "Moderate Track Downforce" : "0 (No Aero Force)");
        }

        // ==========================================
        // TAB 6: TELEMETRY & GAUGES
        // ==========================================
        private static void DrawTelemetryTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><color=#00E5FF>Real-Time Live Telemetry & Dynamics:</color></b>");
            GUILayout.Space(4);

            if (v == null)
            {
                GUILayout.Label("<color=#FF8800>No active player vehicle detected. Enter a vehicle to inspect live telemetry.</color>");
                return;
            }

            string vehRoot = v.transform.root != null ? v.transform.root.name : v.gameObject.name;
            var rb = v.GetComponent<Rigidbody>();
            float massKg = rb != null ? rb.mass : 0f;

            float speedKmh = Mathf.Abs(v.speed * 3.6f);
            float speedMph = Mathf.Abs(v.speed * 2.23694f);
            float rpm = 0f;
            int gear = 0;
            try
            {
                if (v.data != null)
                {
                    rpm = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.EngineRpm) / 1000f;
                    gear = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.GearboxGear);
                }
            }
            catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            float maxRpm = v.engine != null ? v.engine.maxRpm : 3500f;
            float torque = v.engine != null ? v.engine.peakRpmTorque : 0f;
            float estHp = torque * (rpm * 0.1047f) / 745.7f;
            if (estHp < 0) estHp = 0;

            GUILayout.BeginHorizontal();
            GUILayout.Box($"<b><size=18><color=#00E5FF>{speedKmh:F0}</color></size></b> km/h\n<size=11>{speedMph:F0} mph</size>", GUILayout.Width(130), GUILayout.Height(55));
            GUILayout.Box($"<b><size=18><color=#FFD700>{rpm:F0}</color></size></b> RPM\n<size=11>Redline: {maxRpm:F0}</size>", GUILayout.Width(130), GUILayout.Height(55));
            GUILayout.Box($"<b><size=18><color=#00FF88>{(gear == 0 ? "N" : gear == -1 ? "R" : $"D{gear}")}</color></size></b>\n<size=11>Active Gear</size>", GUILayout.Width(100), GUILayout.Height(55));
            GUILayout.Box($"<b><size=18><color=#FF8800>{torque:N0}</color></size></b> Nm\n<size=11>~{estHp:N0} Horsepower</size>", GUILayout.Width(150), GUILayout.Height(55));
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label($"<b>Vehicle Model:</b> <color=#FFD700>{vehRoot}</color> | <b>Weight:</b> {massKg:N0} kg");
            GUILayout.Label($"<b>Steering Angle:</b> {(v.steering != null ? v.steering.maxSteerAngle : 0):F1} deg | <b>Toe:</b> {(v.steering != null ? v.steering.toeAngle : 0):F1} deg");
            GUILayout.Label($"<b>Service Brakes:</b> {(v.brakes != null ? v.brakes.maxBrakeTorque : 0):N0} Nm | <b>Brake Bias:</b> {(v.brakes != null ? v.brakes.brakeBias * 100f : 60):F0}% Front");
            GUILayout.Label($"<b>Differential Lock:</b> Power {(v.differential != null ? v.differential.powerRatio * 100f : 50):F0}% / Coast {(v.differential != null ? v.differential.coastRatio * 100f : 30):F0}%");

            int wheelCount = stock?.Wheels.Count ?? 0;
            GUILayout.Label($"<b>Wheels Detected:</b> {wheelCount} active wheel collider units");

            GUILayout.Space(10);
            GUILayout.Box("<b><color=#00E5FF>Forza Drag & Acceleration Benchmark Stopwatch</color></b>", GUILayout.ExpandWidth(true));
            
            string stateBadge = DragTelemetryBenchmark.State switch
            {
                DragBenchmarkState.Armed => "<color=#00FF88>[ARMED: LAUNCH WHEN READY]</color>",
                DragBenchmarkState.Running => "<color=#FFD700>[RUNNING: TIMING RUN...]</color>",
                DragBenchmarkState.Completed => "<color=#00E5FF>[RUN COMPLETE]</color>",
                _ => "<color=#888888>[STOP VEHICLE TO ARM]</color>"
            };
            GUILayout.Label($"<b>Status:</b> {stateBadge} | Live Time: <b>{DragTelemetryBenchmark.CurrentRunTime:F2}s</b> | Distance: <b>{DragTelemetryBenchmark.CurrentDistanceMeters:F1} m</b>");

            GUILayout.BeginHorizontal();
            GUILayout.Box($"<b>0-60 mph</b>\n<size=16><color=#00E5FF>{(DragTelemetryBenchmark.Last0to60Mph > 0 ? $"{DragTelemetryBenchmark.Last0to60Mph:F2}s" : "--")}</color></size>\n<size=10>Best: {(DragTelemetryBenchmark.Best0to60Mph < 50 ? $"{DragTelemetryBenchmark.Best0to60Mph:F2}s" : "--")}</size>", GUILayout.Width(130), GUILayout.Height(55));
            GUILayout.Box($"<b>0-100 mph</b>\n<size=16><color=#00E5FF>{(DragTelemetryBenchmark.Last0to100Mph > 0 ? $"{DragTelemetryBenchmark.Last0to100Mph:F2}s" : "--")}</color></size>\n<size=10>Best: {(DragTelemetryBenchmark.Best0to100Mph < 50 ? $"{DragTelemetryBenchmark.Best0to100Mph:F2}s" : "--")}</size>", GUILayout.Width(130), GUILayout.Height(55));
            GUILayout.Box($"<b>1/4 Mile ET</b>\n<size=16><color=#FFD700>{(DragTelemetryBenchmark.LastQuarterMileTime > 0 ? $"{DragTelemetryBenchmark.LastQuarterMileTime:F2}s" : "--")}</color></size>\n<size=10>Trap: {DragTelemetryBenchmark.LastQuarterMileTrapSpeedKmh:F0} km/h</size>", GUILayout.Width(150), GUILayout.Height(55));
            GUILayout.Box($"<b>60-0 Braking</b>\n<size=16><color=#FF5555>{(DragTelemetryBenchmark.Last60to0BrakingDistance > 0 ? $"{DragTelemetryBenchmark.Last60to0BrakingDistance:F1}m" : "--")}</color></size>\n<size=10>Emergency Stop</size>", GUILayout.Width(130), GUILayout.Height(55));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Peak Lateral G:</b> <color=#00E5FF>{DragTelemetryBenchmark.PeakLateralG:F2} G</color> | <b>Peak Launch G:</b> <color=#FF8800>{DragTelemetryBenchmark.PeakLongitudinalG:F2} G</color>");
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.45f);
            if (GUILayout.Button("[Reset Records]", GUILayout.Width(110), GUILayout.Height(22)))
            {
                DragTelemetryBenchmark.ResetBests();
            }
            GUILayout.EndHorizontal();
        }

        // ==========================================
        // TAB 7: CUSTOM WHIPS & BODY SWAP GALLERY
        // ==========================================
        private static void DrawWhipsTab()
        {
            var v = VehicleTunerHook.CurrentVehicle;
            var activeSkin = VehicleReskinManager.GetActiveSkin(v);

            // Active Vehicle Header
            GUILayout.BeginHorizontal();
            if (activeSkin != null)
            {
                GUILayout.Label($"<b>Mounted Body:</b> <color=#00E5FF>{activeSkin.DisplayName}</color> <color=#FFD700>[{activeSkin.Category}]</color>");
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                if (GUILayout.Button("[Restore OEM Body]", GUILayout.Width(140), GUILayout.Height(24)))
                {
                    if (v != null) VehicleReskinManager.RestoreStockSkin(v);
                }
            }
            else
            {
                GUILayout.Label("<b>Mounted Body:</b> <color=#AAAAAA>FACTORY OEM STOCK (No Custom Skin)</color>");
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            if (activeSkin != null && v != null)
            {
                var skinState = VehicleReskinManager.GetState(v);

                // --- SHOWROOM & INSPECTION CONTROLS ---
                GUILayout.Box("<b><color=#00E5FF>1. Interactive Showroom & Inspection Mode</color></b>", GUILayout.ExpandWidth(true));
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = (skinState != null && skinState.IsHoodOpen) ? new Color(0.2f, 0.8f, 0.4f) : new Color(0.25f, 0.30f, 0.38f);
                if (GUILayout.Button(skinState != null && skinState.IsHoodOpen ? "[HOOD: OPEN]" : "[POP HOOD]", GUILayout.Height(28)))
                {
                    VehicleReskinManager.ToggleHood(v);
                }

                GUI.backgroundColor = (skinState != null && skinState.AreDoorsOpen) ? new Color(0.2f, 0.8f, 0.4f) : new Color(0.25f, 0.30f, 0.38f);
                if (GUILayout.Button(skinState != null && skinState.AreDoorsOpen ? "[DOORS: OPEN]" : "[OPEN DOORS]", GUILayout.Height(28)))
                {
                    VehicleReskinManager.ToggleDoors(v);
                }

                GUI.backgroundColor = (skinState != null && skinState.IsTrunkOpen) ? new Color(0.2f, 0.8f, 0.4f) : new Color(0.25f, 0.30f, 0.38f);
                if (GUILayout.Button(skinState != null && skinState.IsTrunkOpen ? "[TRUNK: OPEN]" : "[OPEN TRUNK]", GUILayout.Height(28)))
                {
                    VehicleReskinManager.ToggleTrunk(v);
                }

                GUI.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
                if (GUILayout.Button("[CLOSE ALL]", GUILayout.Height(28)))
                {
                    VehicleReskinManager.CloseAllPanels(v);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(6);

                // --- IN-GAME PAINT & CUSTOMIZATION STUDIO ---
                GUILayout.Box("<b><color=#00E5FF>2. In-Game Paint & Customization Studio</color></b>", GUILayout.ExpandWidth(true));
                int paintableCount = skinState?.LoadedAsset?.BodyMaterials.Count ?? 0;
                if (paintableCount == 0)
                {
                    GUILayout.Label("<color=#FF8800><b>No paintable body material detected on this model.</b> Colors cannot be applied.</color>");
                }
                else
                {
                    GUILayout.Label($"<size=11><color=#AAAAAA>Paintable body materials: {paintableCount}</color></size>");
                }
                GUILayout.Label("<b>Iconic Factory OEM Colors:</b>");
                
                // Color Palette Row 1
                GUILayout.BeginHorizontal();
                DrawColorButton(v, "Bayside Blue", new Color(0.08f, 0.25f, 0.85f), 0.85f, 0.95f, false);
                DrawColorButton(v, "Mid. Purple II", new Color(0.24f, 0.08f, 0.35f), 0.90f, 0.95f, false);
                DrawColorButton(v, "Mil. Jade", new Color(0.42f, 0.49f, 0.43f), 0.85f, 0.92f, false);
                DrawColorButton(v, "Tor-Red", new Color(0.95f, 0.28f, 0.05f), 0.80f, 0.92f, false);
                DrawColorButton(v, "Plum Crazy", new Color(0.40f, 0.08f, 0.58f), 0.85f, 0.92f, false);
                DrawColorButton(v, "Sublime Green", new Color(0.35f, 0.78f, 0.10f), 0.80f, 0.90f, false);
                GUILayout.EndHorizontal();

                // Color Palette Row 2
                GUILayout.BeginHorizontal();
                DrawColorButton(v, "Alpine White", new Color(0.96f, 0.96f, 0.98f), 0.80f, 0.95f, false);
                DrawColorButton(v, "Isle of Man", new Color(0.08f, 0.38f, 0.22f), 0.90f, 0.95f, false);
                DrawColorButton(v, "Marina Bay", new Color(0.10f, 0.30f, 0.75f), 0.90f, 0.95f, false);
                DrawColorButton(v, "Code Orange", new Color(0.95f, 0.35f, 0.04f), 0.85f, 0.92f, false);
                DrawColorButton(v, "Avalanche Grey", new Color(0.68f, 0.70f, 0.72f), 0.75f, 0.88f, false);
                DrawColorButton(v, "Velocity Blue", new Color(0.12f, 0.38f, 0.82f), 0.88f, 0.92f, false);
                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                // Finish Options
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Finish:</b>", GUILayout.Width(55));
                GUI.backgroundColor = new Color(0.25f, 0.30f, 0.38f);
                if (GUILayout.Button("Factory Gloss", GUILayout.Height(22)))
                {
                    Color cur = skinState?.CurrentPaintColor ?? Color.white;
                    VehicleReskinManager.ApplyPaint(v, cur, 0.85f, 0.95f, false);
                }
                if (GUILayout.Button("Matte / Satin", GUILayout.Height(22)))
                {
                    Color cur = skinState?.CurrentPaintColor ?? Color.white;
                    VehicleReskinManager.ApplyPaint(v, cur, 0.20f, 0.40f, false);
                }
                if (GUILayout.Button("Metallic Flake", GUILayout.Height(22)))
                {
                    Color cur = skinState?.CurrentPaintColor ?? Color.white;
                    VehicleReskinManager.ApplyPaint(v, cur, 0.98f, 0.90f, false);
                }
                if (GUILayout.Button("Carbon Fiber", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyPaint(v, new Color(0.12f, 0.12f, 0.12f), 0.45f, 0.78f, true);
                }
                GUILayout.EndHorizontal();

                // Window Tint Slider
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                float currentTint = skinState?.WindowTint ?? 0.35f;
                GUILayout.Label($"<b>Window Tint:</b> <color=#00E5FF>{currentTint * 100f:F0}%</color>", GUILayout.Width(170));
                float newTint = GUILayout.HorizontalSlider(currentTint, 0.05f, 0.95f);
                if (Mathf.Abs(newTint - currentTint) > 0.02f)
                {
                    VehicleReskinManager.ApplyWindowTint(v, newTint);
                }
                GUILayout.EndHorizontal();

                // Wheel Rim Finishes
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Wheels:</b>", GUILayout.Width(55));
                GUI.backgroundColor = new Color(0.25f, 0.30f, 0.38f);
                if (GUILayout.Button("Chrome Mirror", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyWheelFinish(v, new Color(0.96f, 0.96f, 0.98f), 0.98f, 0.98f);
                }
                if (GUILayout.Button("Satin Bronze", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyWheelFinish(v, new Color(0.65f, 0.45f, 0.25f), 0.85f, 0.65f);
                }
                if (GUILayout.Button("Gloss Black", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyWheelFinish(v, new Color(0.08f, 0.08f, 0.08f), 0.70f, 0.95f);
                }
                if (GUILayout.Button("Gunmetal Silver", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyWheelFinish(v, new Color(0.42f, 0.44f, 0.46f), 0.90f, 0.85f);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(6);

                // --- ICONIC LIVERIES & DECAL PRESETS ---
                GUILayout.Label("<b>Iconic Liveries & Decal Specs:</b>");
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.20f, 0.35f, 0.50f);
                if (GUILayout.Button("F&F R34 Spec", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyLivery(v, "R34_FastFurious");
                }
                if (GUILayout.Button("HEMI Billboard", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyLivery(v, "Cuda_HemiBillboard");
                }
                if (GUILayout.Button("M Motorsport", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyLivery(v, "BMW_Motorsport");
                }
                if (GUILayout.Button("Fox Baja Trophy", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyLivery(v, "Raptor_FoxBaja");
                }
                if (GUILayout.Button("Clean Stock", GUILayout.Height(22)))
                {
                    VehicleReskinManager.ApplyLivery(v, "Clean_OEM");
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(6);

                // --- AIR RIDE & STANCE HYDRAULICS ---
                GUILayout.Label("<b>Air Ride / Stance Hydraulics:</b>");
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.20f, 0.45f, 0.60f);
                if (GUILayout.Button("[SLAMMED / STANCE]", GUILayout.Height(24)))
                {
                    VehicleReskinManager.SetAirRideStance(v, VehicleReskinManager.AirRideStance.Slammed);
                }
                if (GUILayout.Button("[STREET / TRACK]", GUILayout.Height(24)))
                {
                    VehicleReskinManager.SetAirRideStance(v, VehicleReskinManager.AirRideStance.Street);
                }
                if (GUILayout.Button("[OVERLAND / LIFTED]", GUILayout.Height(24)))
                {
                    VehicleReskinManager.SetAirRideStance(v, VehicleReskinManager.AirRideStance.Lifted);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(6);

                // --- WORKHORSE TOWING & WASH SERVICE ---
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.85f, 0.50f, 0.10f);
                if (GUILayout.Button("[ENGAGE HEAVY TOW MODE]", GUILayout.Height(26)))
                {
                    VehicleReskinManager.SetTowMode(v, true);
                }
                GUI.backgroundColor = new Color(0.15f, 0.65f, 0.85f);
                if (GUILayout.Button("[PRESSURE WASH & DETAIL]", GUILayout.Height(26)))
                {
                    VehicleReskinManager.WashActiveVehicle(v);
                }
                GUI.backgroundColor = new Color(0.25f, 0.75f, 0.35f);
                if (GUILayout.Button("[QUICK REPAIR & DETAIL]", GUILayout.Height(26)))
                {
                    VehicleReskinManager.QuickRepairActiveVehicle();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(6);

                // --- EMERGENCY STROBES & SIREN ---
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = CustomEmergencyLighting.IsStrobesActive ? new Color(1.0f, 0.65f, 0.0f) : new Color(0.3f, 0.35f, 0.4f);
                if (GUILayout.Button(CustomEmergencyLighting.IsStrobesActive ? "[ðŸš¨ ESCORT STROBES: ON [J]]" : "[ðŸš¨ ESCORT STROBES: OFF [J]]", GUILayout.Height(26)))
                {
                    CustomEmergencyLighting.ToggleStrobes();
                }
                GUI.backgroundColor = CustomEmergencyLighting.IsSirenActive ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.3f, 0.35f, 0.4f);
                if (GUILayout.Button(CustomEmergencyLighting.IsSirenActive ? "[ðŸ”Š SIREN: ON [H]]" : "[ðŸ”Š SIREN: OFF [H]]", GUILayout.Height(26)))
                {
                    CustomEmergencyLighting.ToggleSiren();
                }
                GUI.backgroundColor = new Color(0.0f, 0.85f, 1.0f);
                if (GUILayout.Button("[ðŸ“¸ PHOTO MODE [F9]]", GUILayout.Height(26)))
                {
                    PhotoModeFreecam.TogglePhotoMode();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(6);

                // --- IN-CABIN BLUETOOTH STEREO ---
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = InCabinStereo.IsPlaying ? new Color(0.0f, 0.9f, 0.6f) : new Color(0.3f, 0.35f, 0.4f);
                if (GUILayout.Button(InCabinStereo.IsPlaying ? "[â™« STEREO: PLAYING]" : "[â™« STEREO: PAUSED]", GUILayout.Width(170), GUILayout.Height(24)))
                {
                    InCabinStereo.TogglePlayback();
                }
                GUILayout.Label($"<b>Vol:</b> {InCabinStereo.Volume * 100f:F0}%", GUILayout.Width(65));
                InCabinStereo.Volume = GUILayout.HorizontalSlider(InCabinStereo.Volume, 0f, 1.0f);
                GUILayout.EndHorizontal();
                GUILayout.Space(6);

                // --- FORZA SHARE CODES ---
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.2f, 0.45f, 0.7f);
                if (GUILayout.Button("[Generate Share Code]", GUILayout.Width(170), GUILayout.Height(24)))
                {
                    string code = TuningPresetCodeSystem.GenerateShareCode(VehicleTunerHook.Profile);
                    _shareCodeInput = code;
                    ShowToast("SHARE CODE", $"Generated Tune Code: {code}");
                }
                try
                {
                    _shareCodeInput = GUILayout.TextField(_shareCodeInput, GUILayout.Width(160), GUILayout.Height(24));
                }
                catch (System.Exception ex_)
                {
                    Plugin.LogOnce(ex_);
                    GUILayout.Label(_shareCodeInput, GUILayout.Width(160), GUILayout.Height(24));
                }
                GUI.backgroundColor = new Color(0.1f, 0.7f, 0.4f);
                if (GUILayout.Button("[Apply Code]", GUILayout.Width(110), GUILayout.Height(24)))
                {
                    bool ok = TuningPresetCodeSystem.ApplyShareCode(_shareCodeInput, VehicleTunerHook.Profile);
                    ShowToast("APPLY CODE", ok ? $"Loaded Tune: {_shareCodeInput}" : "Invalid share code!");
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(8);
            }

            GUILayout.Box("<b><color=#00E5FF>3. Vehicle Body Swap Gallery</color></b>", GUILayout.ExpandWidth(true));
            GUILayout.Label("<size=11><color=#888888>Select any vehicle below to swap the current vehicle's body and wheels in real-time. Full Forza tuning applies to all models.</color></size>");
            GUILayout.Space(8);

            // Group by category
            var categories = new[]
            {
                (VehicleTypeCategory.MuscleCar, "CARS & MUSCLE"),
                (VehicleTypeCategory.SportsCar, "CARS & MUSCLE"),
                (VehicleTypeCategory.Supercar, "CARS & MUSCLE"),
                (VehicleTypeCategory.Truck, "TRUCKS & 4x4s"),
                (VehicleTypeCategory.Offroad4x4, "TRUCKS & 4x4s"),
                (VehicleTypeCategory.Motorcycle, "MOTORCYCLES")
            };

            var renderedCats = new HashSet<string>();

            foreach (var groupName in new[] { "CARS & MUSCLE", "TRUCKS & 4x4s", "MOTORCYCLES" })
            {
                GUILayout.Box($"<b><color=#00E5FF>// {groupName}</color></b>", GUILayout.ExpandWidth(true));
                GUILayout.Space(3);

                var modelsInGroup = GlbVehicleLoader.Catalog.Where(m =>
                {
                    if (groupName == "CARS & MUSCLE") return m.Category == VehicleTypeCategory.MuscleCar || m.Category == VehicleTypeCategory.SportsCar || m.Category == VehicleTypeCategory.Supercar;
                    if (groupName == "TRUCKS & 4x4s") return m.Category == VehicleTypeCategory.Truck || m.Category == VehicleTypeCategory.Offroad4x4;
                    return m.Category == VehicleTypeCategory.Motorcycle;
                }).ToList();

                foreach (var model in modelsInGroup)
                {
                    bool isCurrent = activeSkin != null && activeSkin.Id == model.Id;
                    
                    GUILayout.BeginHorizontal();
                    
                    // Category Badge
                    string badgeColor = model.Category switch
                    {
                        VehicleTypeCategory.MuscleCar => "#FF8800",
                        VehicleTypeCategory.SportsCar => "#00E5FF",
                        VehicleTypeCategory.Supercar => "#FF00FF",
                        VehicleTypeCategory.Truck => "#FFAA00",
                        VehicleTypeCategory.Offroad4x4 => "#00FF88",
                        VehicleTypeCategory.Motorcycle => "#FF3366",
                        _ => "#AAAAAA"
                    };
                    string badge = model.Category switch
                    {
                        VehicleTypeCategory.MuscleCar => "MUSCLE",
                        VehicleTypeCategory.SportsCar => "SPORT",
                        VehicleTypeCategory.Supercar => "HYPER",
                        VehicleTypeCategory.Truck => "TRUCK",
                        VehicleTypeCategory.Offroad4x4 => "4x4",
                        VehicleTypeCategory.Motorcycle => "BIKE",
                        _ => "CAR"
                    };

                    GUILayout.Label($"<color={badgeColor}>[{badge}]</color> <b>{model.DisplayName}</b>\n<size=11><color=#888888>{model.Subtitle}</color></size>", GUILayout.Width(440));
                    GUILayout.FlexibleSpace();

                    GUI.backgroundColor = isCurrent ? new Color(0.2f, 0.7f, 0.4f, 1.0f) : new Color(0.0f, 0.55f, 0.85f, 1.0f);
                    string btnText = isCurrent ? "[CURRENT]" : "[SWAP BODY]";

                    if (GUILayout.Button(btnText, GUILayout.Width(110), GUILayout.Height(32)))
                    {
                        if (v != null)
                        {
                            VehicleReskinManager.ApplySkin(v, model);
                        }
                        else
                        {
                            ShowToast("NO VEHICLE", "Enter or look at a vehicle before applying a body swap.");
                        }
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                }

                GUILayout.Space(6);
            }
        }

        // ==========================================
        // SLIDER HELPER WITH QUICK NUDGE BUTTONS
        // ==========================================
        private static float DrawSliderWithNudge(string title, float val, float min, float max, string format, string unit, float step, string infoText)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{title}:</b> <color=#00E5FF>{val.ToString(format)}{unit}</color>", GUILayout.Width(240));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<size=11><color=#AAAAAA>{infoText}</color></size>");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            float newVal = GUILayout.HorizontalSlider(val, min, max);

            GUI.backgroundColor = new Color(0.18f, 0.22f, 0.28f);
            if (GUILayout.Button("-", GUILayout.Width(28), GUILayout.Height(18)))
            {
                newVal = Mathf.Max(min, val - step);
            }
            if (GUILayout.Button("+", GUILayout.Width(28), GUILayout.Height(18)))
            {
                newVal = Mathf.Min(max, val + step);
            }
            GUILayout.EndHorizontal();

            if (Mathf.Abs(newVal - val) > (step * 0.05f))
            {
                VehicleTunerHook.MarkCustom();
                return newVal;
            }
            return val;
        }

        private static void DrawColorButton(VPVehicleController v, string name, Color color, float metallic, float smoothness, bool isCarbon)
        {
            GUI.backgroundColor = color;
            if (GUILayout.Button(name, GUILayout.Height(24)))
            {
                VehicleReskinManager.ApplyPaint(v, color, metallic, smoothness, isCarbon);
            }
        }
    }
}
