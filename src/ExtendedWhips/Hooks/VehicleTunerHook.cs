using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ExtendedWhips.Tuning;
using ExtendedWhips.UI;

namespace ExtendedWhips.Hooks
{
    [HarmonyPatch(typeof(VPVehicleController), nameof(VPVehicleController.DoUpdateData))]
    public static class VehicleTunerHook
    {
        private static readonly Dictionary<int, StockVehicleSpecs> _stockCache = new();

        public static VPVehicleController? CurrentVehicle { get; private set; }
        public static StockVehicleSpecs? CurrentStockSpecs
        {
            get
            {
                if (CurrentVehicle == null) return null;
                int id = CurrentVehicle.GetInstanceID();
                return _stockCache.TryGetValue(id, out var specs) ? specs : null;
            }
        }

        public static TuningPreset CurrentPreset { get; private set; } = TuningPreset.Stock;
        public static float TorqueMultiplier { get; private set; } = 1.0f;
        public static float IdleTorqueMultiplier { get; private set; } = 1.0f;
        public static float RpmMultiplier { get; private set; } = 1.0f;
        public static float CustomSteerAngle { get; private set; } = 0.0f; // 0 = stock
        public static float BrakeMultiplier { get; private set; } = 1.0f;
        public static float HandbrakeMultiplier { get; private set; } = 1.0f;

        public static void Postfix(VPVehicleController __instance)
        {
            if (__instance == null || __instance.engine == null) return;

            CurrentVehicle = __instance;
            int vehicleId = __instance.GetInstanceID();

            // Cache stock specs on first observation
            if (!_stockCache.ContainsKey(vehicleId))
            {
                _stockCache[vehicleId] = new StockVehicleSpecs
                {
                    PeakTorque = __instance.engine.peakRpmTorque,
                    IdleTorque = __instance.engine.idleRpmTorque,
                    MaxRpm = __instance.engine.maxRpm,
                    RpmLimiter = __instance.engine.rpmLimiterMax,
                    MaxSteerAngle = __instance.steering != null ? __instance.steering.maxSteerAngle : 35f,
                    MaxBrakeTorque = __instance.brakes != null ? __instance.brakes.maxBrakeTorque : 5000f,
                    HandbrakeTorque = __instance.brakes != null ? __instance.brakes.handbrakeTorque : 8000f
                };

                if (CustomSteerAngle <= 0f)
                {
                    CustomSteerAngle = _stockCache[vehicleId].MaxSteerAngle;
                }

                Plugin.LogMsg($"[ExtendedWhips] Cached new vehicle ({__instance.name}): PeakTorque = {_stockCache[vehicleId].PeakTorque} Nm");
            }

            // Continuously enforce tuned parameters if active
            ApplyTuningToInstance(__instance);

            // Handle In-Game Keybinds during vehicle update
            HandleInput(__instance);
        }

        private static void HandleInput(VPVehicleController instance)
        {
            // [F4] or [Insert] or [F2] Toggle Full Vehicle Configuration Menu
            if (Input.GetKeyDown(KeyCode.F4) || Input.GetKeyDown(KeyCode.Insert) || Input.GetKeyDown(KeyCode.F2))
            {
                Plugin.LogMsg("[ExtendedWhips] Menu toggle key pressed (F4/Insert/F2)");
                ExtendedWhipsUI.ToggleMenu();
            }

            // [F6] Toggle Live Telemetry HUD Overlay
            if (Input.GetKeyDown(KeyCode.F6))
            {
                Plugin.LogMsg("[ExtendedWhips] HUD toggle key pressed");
                ExtendedWhipsUI.ToggleHud();
            }

            // [F5] Quick Cycle Presets
            if (Input.GetKeyDown(KeyCode.F5))
            {
                CyclePreset();
            }

            // [PageUp] / [KeypadPlus] Increase Engine Torque (+0.5x)
            if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                SetTorqueMultiplier(TorqueMultiplier + 0.5f, showToast: true);
            }

            // [PageDown] / [KeypadMinus] Decrease Engine Torque (-0.5x)
            if (Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                SetTorqueMultiplier(TorqueMultiplier - 0.5f, showToast: true);
            }

            // [F8] Reset Vehicle to Stock Factory Specs
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ResetToStock(showToast: true);
            }
        }

        public static void CyclePreset()
        {
            var nextPreset = CurrentPreset switch
            {
                TuningPreset.Stock => TuningPreset.StreetPerformance,
                TuningPreset.StreetPerformance => TuningPreset.OffroadLifted,
                TuningPreset.OffroadLifted => TuningPreset.DriftSpec,
                TuningPreset.DriftSpec => TuningPreset.HyperInsane,
                _ => TuningPreset.Stock
            };

            ApplyPreset(nextPreset, showToast: true);
        }

        public static void ApplyPreset(TuningPreset preset, bool showToast = true)
        {
            CurrentPreset = preset;
            var stock = CurrentStockSpecs;
            float stockSteer = stock != null ? stock.MaxSteerAngle : 35f;

            switch (preset)
            {
                case TuningPreset.Stock:
                    TorqueMultiplier = 1.0f;
                    IdleTorqueMultiplier = 1.0f;
                    RpmMultiplier = 1.0f;
                    CustomSteerAngle = stockSteer;
                    BrakeMultiplier = 1.0f;
                    HandbrakeMultiplier = 1.0f;
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: FACTORY STOCK", "Vehicle restored to default OEM specifications.");
                    }
                    break;

                case TuningPreset.StreetPerformance:
                    TorqueMultiplier = 2.5f;
                    IdleTorqueMultiplier = 2.0f;
                    RpmMultiplier = 1.4f;
                    CustomSteerAngle = stockSteer + 5f;
                    BrakeMultiplier = 2.0f;
                    HandbrakeMultiplier = 2.5f;
                    if (showToast)
                    {
                        float torque = stock != null ? stock.PeakTorque * 2.5f : 0f;
                        ExtendedWhipsUI.ShowToast("PRESET: STREET PERFORMANCE", $"Torque: 2.5x ({torque:N0} Nm) | Redline: 1.4x | +5 deg Steer | 2.0x Brakes");
                    }
                    break;

                case TuningPreset.OffroadLifted:
                    TorqueMultiplier = 3.5f;
                    IdleTorqueMultiplier = 3.0f;
                    RpmMultiplier = 1.2f;
                    CustomSteerAngle = stockSteer;
                    BrakeMultiplier = 2.5f;
                    HandbrakeMultiplier = 3.0f;
                    if (showToast)
                    {
                        float torque = stock != null ? stock.PeakTorque * 3.5f : 0f;
                        ExtendedWhipsUI.ShowToast("PRESET: OFFROAD HAULER", $"Torque: 3.5x ({torque:N0} Nm) | 3.0x Low-End Pull | 2.5x Heavy Brakes");
                    }
                    break;

                case TuningPreset.DriftSpec:
                    TorqueMultiplier = 3.0f;
                    IdleTorqueMultiplier = 2.5f;
                    RpmMultiplier = 1.5f;
                    CustomSteerAngle = stockSteer + 15f;
                    BrakeMultiplier = 1.5f;
                    HandbrakeMultiplier = 4.5f;
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: DRIFT SPEC", $"Torque: 3.0x | +15 deg Steer Lock | 4.5x Instant Rear Handbrake");
                    }
                    break;

                case TuningPreset.HyperInsane:
                    TorqueMultiplier = 5.0f;
                    IdleTorqueMultiplier = 4.0f;
                    RpmMultiplier = 2.0f;
                    CustomSteerAngle = stockSteer + 20f;
                    BrakeMultiplier = 3.5f;
                    HandbrakeMultiplier = 5.0f;
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: HYPER OVERKILL", $"Torque: 5.0x | 2.0x Redline | +20 deg Max Steer | Super Brakes");
                    }
                    break;

                case TuningPreset.Custom:
                    break;
            }

            if (CurrentVehicle != null)
            {
                ApplyTuningToInstance(CurrentVehicle);
            }
        }

        public static void SetTorqueMultiplier(float mult, bool showToast = true)
        {
            TorqueMultiplier = Mathf.Clamp(mult, 0.5f, 6.0f);
            CurrentPreset = TuningPreset.Custom;
            if (CurrentVehicle != null) ApplyTuningToInstance(CurrentVehicle);

            if (showToast)
            {
                var stock = CurrentStockSpecs;
                float nm = stock != null ? stock.PeakTorque * TorqueMultiplier : 0;
                ExtendedWhipsUI.ShowToast("TORQUE UPDATED", $"Torque: {TorqueMultiplier:F1}x ({nm:N0} Nm)");
            }
        }

        public static void SetIdleTorqueMultiplier(float mult)
        {
            IdleTorqueMultiplier = Mathf.Clamp(mult, 0.5f, 5.0f);
            CurrentPreset = TuningPreset.Custom;
            if (CurrentVehicle != null) ApplyTuningToInstance(CurrentVehicle);
        }

        public static void SetRpmMultiplier(float mult, bool showToast = true)
        {
            RpmMultiplier = Mathf.Clamp(mult, 1.0f, 2.5f);
            CurrentPreset = TuningPreset.Custom;
            if (CurrentVehicle != null) ApplyTuningToInstance(CurrentVehicle);

            if (showToast)
            {
                var stock = CurrentStockSpecs;
                float rpm = stock != null ? stock.MaxRpm * RpmMultiplier : 0;
                ExtendedWhipsUI.ShowToast("REDLINE UPDATED", $"RPM Limiter: {RpmMultiplier:F1}x ({rpm:N0} RPM)");
            }
        }

        public static void SetSteerAngle(float angle, bool showToast = true)
        {
            CustomSteerAngle = Mathf.Clamp(angle, 20f, 75f);
            CurrentPreset = TuningPreset.Custom;
            if (CurrentVehicle != null) ApplyTuningToInstance(CurrentVehicle);

            if (showToast)
            {
                ExtendedWhipsUI.ShowToast("STEERING UPDATED", $"Max Steer Angle: {CustomSteerAngle:F0} deg");
            }
        }

        public static void SetBrakeMultiplier(float mult)
        {
            BrakeMultiplier = Mathf.Clamp(mult, 0.5f, 4.0f);
            CurrentPreset = TuningPreset.Custom;
            if (CurrentVehicle != null) ApplyTuningToInstance(CurrentVehicle);
        }

        public static void SetHandbrakeMultiplier(float mult)
        {
            HandbrakeMultiplier = Mathf.Clamp(mult, 1.0f, 6.0f);
            CurrentPreset = TuningPreset.Custom;
            if (CurrentVehicle != null) ApplyTuningToInstance(CurrentVehicle);
        }

        public static void ResetToStock(bool showToast = true)
        {
            ApplyPreset(TuningPreset.Stock, showToast);
        }

        private static void ApplyTuningToInstance(VPVehicleController v)
        {
            int vehicleId = v.GetInstanceID();
            if (!_stockCache.TryGetValue(vehicleId, out var stock)) return;

            // Apply Engine
            if (v.engine != null)
            {
                v.engine.peakRpmTorque = stock.PeakTorque * TorqueMultiplier;
                v.engine.idleRpmTorque = stock.IdleTorque * IdleTorqueMultiplier;
                v.engine.maxRpm = stock.MaxRpm * RpmMultiplier;
                v.engine.rpmLimiterMax = stock.RpmLimiter * RpmMultiplier;
            }

            // Apply Steering
            if (v.steering != null)
            {
                v.steering.maxSteerAngle = CustomSteerAngle > 0f ? CustomSteerAngle : stock.MaxSteerAngle;
            }

            // Apply Brakes
            if (v.brakes != null)
            {
                v.brakes.maxBrakeTorque = stock.MaxBrakeTorque * BrakeMultiplier;
                v.brakes.handbrakeTorque = stock.HandbrakeTorque * HandbrakeMultiplier;
            }
        }
    }
}
