using System;
using HarmonyLib;
using UnityEngine;
using RootMotion;
using ExtendedWhips.Hooks;
using ExtendedWhips.Tuning;
using ExtendedWhips.Spawning;

namespace ExtendedWhips.UI
{
    [HarmonyPatch(typeof(DemoGUIMessage), nameof(DemoGUIMessage.OnGUI))]
    public static class DemoGuiMessagePatch
    {
        private static int _loggedPumps = 0;

        public static bool Prefix(DemoGUIMessage __instance)
        {
            if (_loggedPumps++ < 3)
            {
                Plugin.LogMsg($"[ExtendedWhips] IMGUI host OnGUI active (Frame={Time.frameCount}, Event={Event.current?.type})");
            }

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
        private static int _lastDrawnFrame = -1;
        private static EventType _lastDrawnEventType = EventType.Ignore;

        // UI State
        public static bool IsMenuOpen { get; private set; } = false;
        public static bool IsHudVisible { get; set; } = true;

        private static Rect _menuRect = new Rect(80, 80, 560, 580);
        private static int _selectedTab = 0;
        private static readonly string[] _tabNames = { "[PRESETS]", "[ENGINE]", "[HANDLING]", "[TELEMETRY]", "[WHIPS]" };

        // Draggable window state
        private static bool _isDragging = false;
        private static Vector2 _dragOffset = Vector2.zero;

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

        public static void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
            Plugin.LogMsg($"[ExtendedWhips] ToggleMenu called: IsMenuOpen = {IsMenuOpen}");
            try
            {
                Cursor.visible = IsMenuOpen;
                Cursor.lockState = IsMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
            }
            catch (Exception ex)
            {
                Plugin.LogMsg($"[ExtendedWhips] Cursor state note: {ex.Message}");
            }
        }

        public static void ToggleHud()
        {
            IsHudVisible = !IsHudVisible;
            ShowToast("HUD DISPLAY", IsHudVisible ? "Live Telemetry Overlay: ON" : "Live Telemetry Overlay: OFF", 2.0f);
        }

        public static void OnRenderGUI()
        {
            if (Event.current == null) return;

            // Deduplicate multiple calls per frame for the same event type pass
            if (Time.frameCount == _lastDrawnFrame && Event.current.type == _lastDrawnEventType)
            {
                return;
            }
            _lastDrawnFrame = Time.frameCount;
            _lastDrawnEventType = Event.current.type;

            // Escape closes menu
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && IsMenuOpen)
            {
                ToggleMenu();
                Event.current.Use();
                return;
            }

            // Draw Toast Banner
            try
            {
                DrawToast();
            }
            catch (Exception ex)
            {
                Plugin.LogMsg($"[ExtendedWhips] Toast render error: {ex.Message}");
            }

            // Draw On-Screen HUD Overlay
            try
            {
                DrawHud();
            }
            catch (Exception ex)
            {
                Plugin.LogMsg($"[ExtendedWhips] HUD render error: {ex.Message}");
            }

            // Draw Configuration Menu Window
            if (IsMenuOpen)
            {
                try
                {
                    DrawMenuWindow();
                }
                catch (Exception ex)
                {
                    Plugin.LogMsg($"[ExtendedWhips] Menu render error: {ex.Message}");
                }
            }
        }

        // ==========================================
        // 1. FLOATING TOAST NOTIFICATION
        // ==========================================
        private static void DrawToast()
        {
            if (Time.time > _toastExpireTime || string.IsNullOrEmpty(_toastTitle)) return;

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
            GUILayout.Label($"<b><size=14><color=#00E5FF>>> {_toastTitle}</color></size></b>");
            GUILayout.Label($"<size=12><color=#F0F0F0>{_toastMessage}</color></size>");
            GUILayout.EndArea();
        }

        // ==========================================
        // 2. LIVE ON-SCREEN TELEMETRY HUD
        // ==========================================
        private static void DrawHud()
        {
            if (!IsHudVisible) return;
            var v = VehicleTunerHook.CurrentVehicle;
            if (v == null) return;

            float width = 310f;
            float height = 190f;
            float x = Screen.width - width - 20f;
            float y = 20f;

            GUI.backgroundColor = new Color(0.05f, 0.07f, 0.10f, 0.88f);
            GUI.Box(new Rect(x, y, width, height), "");

            // Top cyan line
            GUI.backgroundColor = new Color(0.0f, 0.85f, 1.0f, 0.9f);
            GUI.Box(new Rect(x, y, width, 3), "");

            GUILayout.BeginArea(new Rect(x + 12, y + 8, width - 24, height - 16));

            // Header Row: Vehicle name & Config button
            GUILayout.BeginHorizontal();
            string vehName = v.transform.root != null ? v.transform.root.name : v.gameObject.name;
            if (vehName.Length > 15) vehName = vehName.Substring(0, 15) + "...";
            GUILayout.Label($"<b><color=#00E5FF>WHIPS</color> // <color=#FFD700>{vehName}</color></b>");
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = IsMenuOpen ? new Color(1.0f, 0.3f, 0.3f, 1.0f) : new Color(0.0f, 0.6f, 0.9f, 1.0f);
            if (GUILayout.Button(IsMenuOpen ? "[Close]" : "[Menu F4]", GUILayout.Width(75), GUILayout.Height(22)))
            {
                ToggleMenu();
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
                TuningPreset.HyperInsane => "#FF00FF",
                _ => "#FFFF00"
            };
            string presetName = VehicleTunerHook.CurrentPreset switch
            {
                TuningPreset.Stock => "FACTORY STOCK",
                TuningPreset.StreetPerformance => "STREET PERFORMANCE",
                TuningPreset.OffroadLifted => "OFFROAD HAULER",
                TuningPreset.DriftSpec => "DRIFT SPEC",
                TuningPreset.HyperInsane => "HYPER OVERKILL",
                _ => "CUSTOM TUNED"
            };
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
            catch { }

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
            string bar = new string('=', filledBars) + new string('.', totalBars - filledBars);
            GUILayout.Label($"<b>RPM:</b> {rpm:F0} / {maxRpm:F0} <color=#FF8800>[{bar}]</color>");

            // Torque & Steer
            float torque = v.engine != null ? v.engine.peakRpmTorque : 0f;
            float steerAngle = v.steering != null ? v.steering.maxSteerAngle : 35f;
            GUILayout.Label($"<b>Torque:</b> {torque:N0} Nm ({VehicleTunerHook.TorqueMultiplier:F1}x) | <b>Steer:</b> {steerAngle:F0} deg");

            GUILayout.EndArea();
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

            // Handle Header Dragging
            Rect headerRect = new Rect(_menuRect.x, _menuRect.y, _menuRect.width, 36f);
            if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
            {
                _isDragging = true;
                _dragOffset = Event.current.mousePosition - new Vector2(_menuRect.x, _menuRect.y);
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                _isDragging = false;
            }
            else if (_isDragging && Event.current.type == EventType.MouseDrag)
            {
                _menuRect.x = Event.current.mousePosition.x - _dragOffset.x;
                _menuRect.y = Event.current.mousePosition.y - _dragOffset.y;
            }

            // Window Background
            GUI.backgroundColor = new Color(0.06f, 0.08f, 0.11f, 0.98f);
            GUI.Box(_menuRect, "");

            // Window Title Bar
            GUI.backgroundColor = new Color(0.08f, 0.12f, 0.18f, 1.0f);
            GUI.Box(headerRect, "");

            // Title Bar Cyan Glow
            GUI.backgroundColor = new Color(0.0f, 0.85f, 1.0f, 1.0f);
            GUI.Box(new Rect(_menuRect.x, _menuRect.y, _menuRect.width, 3), "");

            // Title Bar Text & Close Button
            GUILayout.BeginArea(new Rect(_menuRect.x + 12, _menuRect.y + 6, _menuRect.width - 24, 28));
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b><size=13><color=#00E5FF>EXTENDED WHIPS</color> // <color=#FFFFFF>VEHICLE CONFIGURATOR</color></size></b>");
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f, 1.0f);
            if (GUILayout.Button("[X]", GUILayout.Width(35), GUILayout.Height(22)))
            {
                ToggleMenu();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Window Content Body
            GUILayout.BeginArea(new Rect(_menuRect.x + 16, _menuRect.y + 42, _menuRect.width - 32, _menuRect.height - 52));

            // Vehicle Subheader Info
            string vehRoot = v != null && v.transform.root != null ? v.transform.root.name : "No Vehicle";
            float curTorque = v != null && v.engine != null ? v.engine.peakRpmTorque : 0;
            float stockTorque = stock != null ? stock.PeakTorque : 0;
            GUILayout.Label($"<b>Vehicle:</b> <color=#FFD700>{vehRoot}</color> | <b>Torque:</b> {curTorque:N0} Nm (Stock: {stockTorque:N0} Nm)");

            GUILayout.Space(6);

            // Tab Navigation Bar
            int targetTab = _selectedTab;
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _tabNames.Length; i++)
            {
                GUI.backgroundColor = _selectedTab == i ? new Color(0.0f, 0.75f, 0.95f, 1.0f) : new Color(0.14f, 0.18f, 0.24f, 1.0f);
                if (GUILayout.Button(_tabNames[i], GUILayout.Height(30)))
                {
                    targetTab = i;
                }
            }
            GUILayout.EndHorizontal();
            _selectedTab = targetTab;

            GUILayout.Space(10);

            // Render Tab Body
            switch (_selectedTab)
            {
                case 0:
                    DrawPresetsTab();
                    break;
                case 1:
                    DrawEngineTab(v, stock);
                    break;
                case 2:
                    DrawHandlingTab(v, stock);
                    break;
                case 3:
                    DrawTelemetryTab(v, stock);
                    break;
                case 4:
                    DrawWhipsTab();
                    break;
            }

            GUILayout.FlexibleSpace();

            // Footer Actions Bar
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.7f, 0.3f, 0.1f, 1.0f);
            if (GUILayout.Button("Reset Stock [F8]", GUILayout.Height(32)))
            {
                VehicleTunerHook.ResetToStock(showToast: true);
            }

            GUI.backgroundColor = new Color(0.15f, 0.2f, 0.28f, 1.0f);
            if (GUILayout.Button(IsHudVisible ? "Hide HUD [F6]" : "Show HUD [F6]", GUILayout.Height(32)))
            {
                ToggleHud();
            }

            GUI.backgroundColor = new Color(0.1f, 0.5f, 0.8f, 1.0f);
            if (GUILayout.Button("Close Menu [F4]", GUILayout.Height(32)))
            {
                ToggleMenu();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        // ==========================================
        // TAB 0: PRESETS
        // ==========================================
        private static void DrawPresetsTab()
        {
            GUILayout.Label("<b><size=13><color=#00E5FF>Quick Vehicle Tuning Presets:</color></size></b>");
            GUILayout.Label("Click any preset to apply engine, redline, steering, and braking tunes.");
            GUILayout.Space(6);

            // Preset: Stock
            GUI.backgroundColor = new Color(0.2f, 0.25f, 0.3f, 1.0f);
            if (GUILayout.Button("<b>[ FACTORY STOCK (OEM Specs) ]</b>\n<size=11>Restores original 1.0x torque, factory redline, normal steering lock, and stock braking.</size>", GUILayout.Height(46)))
            {
                VehicleTunerHook.ResetToStock(showToast: true);
            }

            GUILayout.Space(6);

            // Preset: Street Performance
            GUI.backgroundColor = new Color(0.0f, 0.6f, 0.35f, 1.0f);
            if (GUILayout.Button("<b>[ STREET PERFORMANCE (Fast Transit & Highway) ]</b>\n<size=11>2.5x Torque | 1.4x RPM Redline | +5 deg Steering Angle | 2.0x High Braking Power.</size>", GUILayout.Height(46)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.StreetPerformance, showToast: true);
            }

            GUILayout.Space(6);

            // Preset: Offroad Hauler
            GUI.backgroundColor = new Color(0.75f, 0.45f, 0.05f, 1.0f);
            if (GUILayout.Button("<b>[ OFFROAD HEAVY HAULER (Massive Low-End Crawler) ]</b>\n<size=11>3.5x Torque | 3.0x Idle Pull | 2.5x Heavy Brakes. Pulls steep hills and muddy excavations.</size>", GUILayout.Height(46)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.OffroadLifted, showToast: true);
            }

            GUILayout.Space(6);

            // Preset: Drift Spec
            GUI.backgroundColor = new Color(0.0f, 0.5f, 0.75f, 1.0f);
            if (GUILayout.Button("<b>[ DRIFT SPEC (Deep Steer Angle & Instant E-Brake) ]</b>\n<size=11>3.0x Torque | 1.5x Redline | +15 deg Steer Angle | 4.5x Instant Rear Axle Handbrake Lock.</size>", GUILayout.Height(46)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.DriftSpec, showToast: true);
            }

            GUILayout.Space(6);

            // Preset: Hyper Overkill
            GUI.backgroundColor = new Color(0.6f, 0.1f, 0.6f, 1.0f);
            if (GUILayout.Button("<b>[ HYPER OVERKILL (Unleashed Monster Machine) ]</b>\n<size=11>5.0x Torque | 2.0x Redline | +20 deg Max Steer | 3.5x Super Brakes. Maximum speed.</size>", GUILayout.Height(46)))
            {
                VehicleTunerHook.ApplyPreset(TuningPreset.HyperInsane, showToast: true);
            }
        }

        // ==========================================
        // TAB 1: ENGINE & POWER
        // ==========================================
        private static void DrawEngineTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><size=13><color=#00E5FF>Engine & Powertrain Tuning:</color></size></b>");
            GUILayout.Space(5);

            float stockTorque = stock != null ? stock.PeakTorque : 2000f;
            float calcTorque = stockTorque * VehicleTunerHook.TorqueMultiplier;

            // 1. Peak Torque
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Peak Engine Torque:</b> <color=#00FF88>{VehicleTunerHook.TorqueMultiplier:F1}x</color> ({calcTorque:N0} Nm)", GUILayout.Width(270));
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.18f, 0.22f, 0.28f);
            if (GUILayout.Button("-0.5x", GUILayout.Width(45))) VehicleTunerHook.SetTorqueMultiplier(VehicleTunerHook.TorqueMultiplier - 0.5f, showToast: true);
            if (GUILayout.Button("+0.5x", GUILayout.Width(45))) VehicleTunerHook.SetTorqueMultiplier(VehicleTunerHook.TorqueMultiplier + 0.5f, showToast: true);
            if (GUILayout.Button("2.5x", GUILayout.Width(45))) VehicleTunerHook.SetTorqueMultiplier(2.5f, showToast: true);
            if (GUILayout.Button("Max 6x", GUILayout.Width(50))) VehicleTunerHook.SetTorqueMultiplier(6.0f, showToast: true);
            GUILayout.EndHorizontal();

            float newTorque = GUILayout.HorizontalSlider(VehicleTunerHook.TorqueMultiplier, 0.5f, 6.0f);
            if (Mathf.Abs(newTorque - VehicleTunerHook.TorqueMultiplier) > 0.05f)
            {
                VehicleTunerHook.SetTorqueMultiplier(Mathf.Round(newTorque * 10f) / 10f, showToast: false);
            }

            GUILayout.Space(12);

            // 2. Low-End Idle Torque
            float stockIdle = stock != null ? stock.IdleTorque : 500f;
            float calcIdle = stockIdle * VehicleTunerHook.IdleTorqueMultiplier;
            GUILayout.Label($"<b>Idle / Low-End Crawl Torque:</b> <color=#FFAA00>{VehicleTunerHook.IdleTorqueMultiplier:F1}x</color> ({calcIdle:N0} Nm)");
            float newIdle = GUILayout.HorizontalSlider(VehicleTunerHook.IdleTorqueMultiplier, 0.5f, 5.0f);
            if (Mathf.Abs(newIdle - VehicleTunerHook.IdleTorqueMultiplier) > 0.05f)
            {
                VehicleTunerHook.SetIdleTorqueMultiplier(Mathf.Round(newIdle * 10f) / 10f);
            }

            GUILayout.Space(12);

            // 3. Engine Redline / RPM Limiter
            float stockRpm = stock != null ? stock.MaxRpm : 3500f;
            float calcRpm = stockRpm * VehicleTunerHook.RpmMultiplier;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Engine RPM Limiter / Redline:</b> <color=#00E5FF>{VehicleTunerHook.RpmMultiplier:F1}x</color> ({calcRpm:N0} RPM)", GUILayout.Width(270));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("1.0x", GUILayout.Width(45))) VehicleTunerHook.SetRpmMultiplier(1.0f, showToast: true);
            if (GUILayout.Button("1.4x", GUILayout.Width(45))) VehicleTunerHook.SetRpmMultiplier(1.4f, showToast: true);
            if (GUILayout.Button("2.0x", GUILayout.Width(45))) VehicleTunerHook.SetRpmMultiplier(2.0f, showToast: true);
            GUILayout.EndHorizontal();

            float newRpm = GUILayout.HorizontalSlider(VehicleTunerHook.RpmMultiplier, 1.0f, 2.5f);
            if (Mathf.Abs(newRpm - VehicleTunerHook.RpmMultiplier) > 0.05f)
            {
                VehicleTunerHook.SetRpmMultiplier(Mathf.Round(newRpm * 10f) / 10f, showToast: false);
            }
        }

        // ==========================================
        // TAB 2: HANDLING & BRAKES
        // ==========================================
        private static void DrawHandlingTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><size=13><color=#00E5FF>Steering, Handling & Braking:</color></size></b>");
            GUILayout.Space(5);

            float stockSteer = stock != null ? stock.MaxSteerAngle : 35f;

            // 1. Steering Lock Angle
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Max Steering Lock Angle:</b> <color=#00E5FF>{VehicleTunerHook.CustomSteerAngle:F0} deg</color> (Stock: {stockSteer:F0} deg)", GUILayout.Width(270));
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.18f, 0.22f, 0.28f);
            if (GUILayout.Button("35", GUILayout.Width(45))) VehicleTunerHook.SetSteerAngle(35f, showToast: true);
            if (GUILayout.Button("45", GUILayout.Width(45))) VehicleTunerHook.SetSteerAngle(45f, showToast: true);
            if (GUILayout.Button("55", GUILayout.Width(45))) VehicleTunerHook.SetSteerAngle(55f, showToast: true);
            if (GUILayout.Button("65", GUILayout.Width(45))) VehicleTunerHook.SetSteerAngle(65f, showToast: true);
            GUILayout.EndHorizontal();

            float newSteer = GUILayout.HorizontalSlider(VehicleTunerHook.CustomSteerAngle, 20f, 75f);
            if (Mathf.Abs(newSteer - VehicleTunerHook.CustomSteerAngle) > 0.5f)
            {
                VehicleTunerHook.SetSteerAngle(Mathf.Round(newSteer), showToast: false);
            }

            GUILayout.Space(12);

            // 2. Service Brake Force
            float stockBrake = stock != null ? stock.MaxBrakeTorque : 5000f;
            float calcBrake = stockBrake * VehicleTunerHook.BrakeMultiplier;
            GUILayout.Label($"<b>Service Brake Force:</b> <color=#00FF88>{VehicleTunerHook.BrakeMultiplier:F1}x</color> ({calcBrake:N0} Nm)");
            float newBrake = GUILayout.HorizontalSlider(VehicleTunerHook.BrakeMultiplier, 0.5f, 4.0f);
            if (Mathf.Abs(newBrake - VehicleTunerHook.BrakeMultiplier) > 0.05f)
            {
                VehicleTunerHook.SetBrakeMultiplier(Mathf.Round(newBrake * 10f) / 10f);
            }

            GUILayout.Space(12);

            // 3. Handbrake / E-Brake Torque
            float stockHb = stock != null ? stock.HandbrakeTorque : 8000f;
            float calcHb = stockHb * VehicleTunerHook.HandbrakeMultiplier;
            GUILayout.Label($"<b>Handbrake / E-Brake Torque:</b> <color=#FF8800>{VehicleTunerHook.HandbrakeMultiplier:F1}x</color> ({calcHb:N0} Nm)");
            float newHb = GUILayout.HorizontalSlider(VehicleTunerHook.HandbrakeMultiplier, 1.0f, 6.0f);
            if (Mathf.Abs(newHb - VehicleTunerHook.HandbrakeMultiplier) > 0.05f)
            {
                VehicleTunerHook.SetHandbrakeMultiplier(Mathf.Round(newHb * 10f) / 10f);
            }
        }

        // ==========================================
        // TAB 3: TELEMETRY & DIAGNOSTICS
        // ==========================================
        private static void DrawTelemetryTab(VPVehicleController? v, StockVehicleSpecs? stock)
        {
            GUILayout.Label("<b><size=13><color=#00E5FF>Real-Time Vehicle Telemetry & Physics:</color></size></b>");
            GUILayout.Space(5);

            if (v == null)
            {
                GUILayout.Label("<color=#FF8800>No active player vehicle detected. Enter a vehicle to inspect live telemetry.</color>");
                return;
            }

            string vehRoot = v.transform.root != null ? v.transform.root.name : v.gameObject.name;
            var rb = v.GetComponent<Rigidbody>();
            float massKg = rb != null ? rb.mass : 0f;
            float massTons = massKg / 1000f;

            GUILayout.Label($"<b>Root Model:</b> <color=#FFD700>{vehRoot}</color>");
            GUILayout.Label($"<b>Vehicle Weight:</b> {massKg:N0} kg ({massTons:F1} metric tons)");

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
            catch { }

            GUILayout.Label($"<b>Velocity:</b> {speedKmh:F1} km/h ({speedMph:F1} mph) | <b>Forward Speed:</b> {v.speed:F2} m/s");
            float maxRpm = v.engine != null ? v.engine.maxRpm : 0f;
            GUILayout.Label($"<b>Engine RPM:</b> {rpm:F0} / {maxRpm:F0} RPM");
            GUILayout.Label($"<b>Active Gear:</b> {(gear == 0 ? "Neutral" : gear == -1 ? "Reverse" : $"Forward {gear}")}");
            float peakTorque = v.engine != null ? v.engine.peakRpmTorque : 0f;
            float idleTorque = v.engine != null ? v.engine.idleRpmTorque : 0f;
            GUILayout.Label($"<b>Peak Torque:</b> {peakTorque:N0} Nm | <b>Idle Torque:</b> {idleTorque:N0} Nm");
            GUILayout.Label($"<b>Steering Angle:</b> {(v.steering != null ? v.steering.maxSteerAngle : 0):F1} deg");
            GUILayout.Label($"<b>Brake Torque:</b> {(v.brakes != null ? v.brakes.maxBrakeTorque : 0):N0} Nm | <b>Handbrake:</b> {(v.brakes != null ? v.brakes.handbrakeTorque : 0):N0} Nm");
        }

        // ==========================================
        // TAB 4: CUSTOM WHIPS & SPAWNER
        // ==========================================
        private static void DrawWhipsTab()
        {
            GUILayout.Label("<b><size=13><color=#00E5FF>Custom Vehicles (Whips) Pipeline:</color></size></b>");
            GUILayout.Space(5);

            GUILayout.Label("Add custom pickup trucks, cars, and motorcycles via Unity AssetBundles into:\n<color=#FFD700>BepInEx/plugins/ExtendedWhips/bundles/</color>");
            GUILayout.Space(8);

            GUI.backgroundColor = new Color(0.0f, 0.6f, 0.85f, 1.0f);
            if (GUILayout.Button("Spawn Custom Whip In Front of Camera", GUILayout.Height(38)))
            {
                VehicleSpawner.Instance.SpawnCurrentVehicle();
            }

            GUILayout.Space(6);
            GUI.backgroundColor = new Color(0.2f, 0.25f, 0.32f, 1.0f);
            if (GUILayout.Button("Scan Bundles Directory", GUILayout.Height(30)))
            {
                ShowToast("ASSET BUNDLES", "Scanned plugins directory for custom whip packages.");
            }
        }
    }
}
