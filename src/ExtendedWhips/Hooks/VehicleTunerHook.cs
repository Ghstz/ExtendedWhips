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

        public static TuningProfile Profile { get; } = new TuningProfile();

        // Convenience accessors for UI & telemetry
        public static TuningPreset CurrentPreset => Profile.ActivePreset;
        public static float TorqueMultiplier => Profile.TorqueMultiplier;
        public static float IdleTorqueMultiplier => Profile.IdleTorqueMultiplier;
        public static float RpmMultiplier => Profile.RpmMultiplier;
        public static float CustomSteerAngle => Profile.CustomSteerAngle;
        public static float BrakeMultiplier => Profile.BrakeTorqueMultiplier;
        public static float HandbrakeMultiplier => Profile.HandbrakeTorqueMultiplier;

        public static void SetIdleTorqueMultiplier(float mult) { Profile.IdleTorqueMultiplier = mult; MarkCustom(); }
        public static void SetRpmMultiplier(float mult, bool showToast = true) { Profile.RpmMultiplier = mult; MarkCustom(); }
        public static void SetSteerAngle(float angle, bool showToast = true) { Profile.CustomSteerAngle = angle; MarkCustom(); }
        public static void SetBrakeMultiplier(float mult) { Profile.BrakeTorqueMultiplier = mult; MarkCustom(); }
        public static void SetHandbrakeMultiplier(float mult) { Profile.HandbrakeTorqueMultiplier = mult; MarkCustom(); }

        public static void Postfix(VPVehicleController __instance)
        {
            if (__instance == null || __instance.engine == null) return;

            CurrentVehicle = __instance;
            int vehicleId = __instance.GetInstanceID();

            // Cache complete stock specs on first observation
            if (!_stockCache.ContainsKey(vehicleId))
            {
                var stock = new StockVehicleSpecs
                {
                    // Engine
                    PeakTorque = __instance.engine.peakRpmTorque,
                    IdleTorque = __instance.engine.idleRpmTorque,
                    MaxRpm = __instance.engine.maxRpm,
                    RpmLimiter = __instance.engine.rpmLimiterMax,
                    IdleRpm = __instance.engine.idleRpm,
                    Inertia = __instance.engine.inertia,
                    FrictionTorque = __instance.engine.frictionTorque,

                    // Steering
                    MaxSteerAngle = __instance.steering != null ? __instance.steering.maxSteerAngle : 35f,
                    ToeAngle = __instance.steering != null ? __instance.steering.toeAngle : 0.0f,
                    Ackerman = __instance.steering == null || __instance.steering.ackerman,

                    // Brakes
                    MaxBrakeTorque = __instance.brakes != null ? __instance.brakes.maxBrakeTorque : 5000f,
                    BrakeBias = __instance.brakes != null ? __instance.brakes.brakeBias : 0.60f,
                    HandbrakeTorque = __instance.brakes != null ? __instance.brakes.handbrakeTorque : 8000f,

                    // Differential & Transmission
                    FinalDriveRatio = __instance.differential != null ? __instance.differential.gearRatio : 3.5f,
                    DiffPowerRatio = __instance.differential != null ? __instance.differential.powerRatio : 0.5f,
                    DiffCoastRatio = __instance.differential != null ? __instance.differential.coastRatio : 0.3f,
                    DiffPreload = __instance.differential != null ? __instance.differential.preload : 50f,
                    ManualShiftTime = __instance.gearbox != null ? __instance.gearbox.manualShiftTime : 0.25f,
                    AutoTransitionTime = __instance.gearbox != null ? __instance.gearbox.automaticTransitionTime : 0.35f,
                    ClutchMaxTorque = __instance.clutch != null ? __instance.clutch.maxTorqueTransfer : 6000f,

                    // Driver Assists
                    AbsEnabled = __instance.antiLock == null || __instance.antiLock.enabled,
                    TcsEnabled = __instance.tractionControl == null || __instance.tractionControl.enabled,
                    TcsRatio = __instance.tractionControl != null ? __instance.tractionControl.ratio : 1.0f,
                    EspEnabled = __instance.stabilityControl == null || __instance.stabilityControl.enabled,
                };

                // Rigidbody
                var rb = __instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    stock.ChassisMass = rb.mass;
                    stock.CenterOfMass = rb.centerOfMass;
                }

                // Wheel Colliders & Suspension
                var vpWheels = __instance.GetComponentsInChildren<VPWheelCollider>();
                if (vpWheels != null && vpWheels.Length > 0)
                {
                    foreach (var w in vpWheels)
                    {
                        stock.Wheels.Add(new StockWheelSpecs
                        {
                            SuspensionDistance = w.suspensionDistance,
                            SuspensionAnchor = w.suspensionAnchor,
                            SpringRate = w.springRate,
                            DamperRate = w.damperRate,
                            Radius = w.radius,
                            Mass = w.mass
                        });
                    }
                }

                _stockCache[vehicleId] = stock;

                if (Profile.CustomSteerAngle <= 0f)
                {
                    Profile.CustomSteerAngle = stock.MaxSteerAngle;
                }

                Plugin.LogMsg($"[ExtendedWhips] Cached new vehicle ({__instance.name}): PeakTorque = {stock.PeakTorque} Nm, Wheels = {stock.Wheels.Count}");
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
                SetTorqueMultiplier(Profile.TorqueMultiplier + 0.5f, showToast: true);
            }

            // [PageDown] / [KeypadMinus] Decrease Engine Torque (-0.5x)
            if (Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                SetTorqueMultiplier(Profile.TorqueMultiplier - 0.5f, showToast: true);
            }

            // [F8] Reset Vehicle to Stock Factory Specs
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ResetToStock(showToast: true);
            }
        }

        public static void CyclePreset()
        {
            var nextPreset = Profile.ActivePreset switch
            {
                TuningPreset.Stock => TuningPreset.StreetPerformance,
                TuningPreset.StreetPerformance => TuningPreset.DriftSpec,
                TuningPreset.DriftSpec => TuningPreset.OffroadLifted,
                TuningPreset.OffroadLifted => TuningPreset.RallyCross,
                TuningPreset.RallyCross => TuningPreset.HyperInsane,
                _ => TuningPreset.Stock
            };

            ApplyPreset(nextPreset, showToast: true);
        }

        public static void ApplyPreset(TuningPreset preset, bool showToast = true)
        {
            Profile.ActivePreset = preset;
            var stock = CurrentStockSpecs;

            switch (preset)
            {
                case TuningPreset.Stock:
                    Profile.ResetToStock(stock);
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: FACTORY STOCK", "Vehicle restored to default OEM specifications.");
                    }
                    break;

                case TuningPreset.StreetPerformance:
                    Profile.TorqueMultiplier = 2.5f;
                    Profile.IdleTorqueMultiplier = 1.8f;
                    Profile.RpmMultiplier = 1.35f;
                    Profile.FlywheelInertiaMultiplier = 0.70f;
                    Profile.ShiftSpeedMultiplier = 0.50f;
                    Profile.DiffPowerLock = 0.70f;
                    Profile.DiffCoastLock = 0.40f;
                    Profile.RideHeightOffset = -0.05f;
                    Profile.SpringStiffnessMultiplier = 1.8f;
                    Profile.DamperMultiplier = 1.6f;
                    Profile.CustomSteerAngle = (stock?.MaxSteerAngle ?? 35f) + 5f;
                    Profile.TireForwardGripMultiplier = 1.8f;
                    Profile.TireDriftSlipMultiplier = 1.4f;
                    Profile.BrakeTorqueMultiplier = 2.0f;
                    Profile.BrakeBias = 0.65f;
                    Profile.HandbrakeTorqueMultiplier = 2.5f;
                    Profile.DownforceMultiplier = 0.8f;
                    Profile.CenterOfMassYOffset = -0.15f;
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: STREET PERFORMANCE", "2.5x Torque | 1.35x RPM | Lowered Stance | Stiff Springs | 2.0x Brakes");
                    }
                    break;

                case TuningPreset.DriftSpec:
                    Profile.TorqueMultiplier = 3.2f;
                    Profile.IdleTorqueMultiplier = 2.2f;
                    Profile.RpmMultiplier = 1.50f;
                    Profile.FlywheelInertiaMultiplier = 0.40f; // Ultra responsive revs
                    Profile.ShiftSpeedMultiplier = 0.30f;
                    Profile.DiffPowerLock = 1.00f; // 100% Locked Spool for instant slides
                    Profile.DiffCoastLock = 0.70f;
                    Profile.RideHeightOffset = -0.06f;
                    Profile.SpringStiffnessMultiplier = 2.2f;
                    Profile.DamperMultiplier = 1.8f;
                    Profile.CustomSteerAngle = Mathf.Max((stock?.MaxSteerAngle ?? 35f) + 25f, 65f); // 65 deg drift steering angle
                    Profile.TireForwardGripMultiplier = 1.6f;
                    Profile.TireDriftSlipMultiplier = 0.65f; // Slipperier rear tires
                    Profile.BrakeTorqueMultiplier = 1.5f;
                    Profile.BrakeBias = 0.70f;
                    Profile.HandbrakeTorqueMultiplier = 5.0f; // Instant rear lockup
                    Profile.AbsEnabled = false; // Disable ABS for drifting
                    Profile.TcsEnabled = false; // Disable TCS
                    Profile.EspEnabled = false; // Disable ESP
                    Profile.CenterOfMassYOffset = -0.20f;
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: DRIFT SPEC", "3.2x Torque | 65 deg Drift Lock | 100% Spool Diff | 5.0x E-Brake | Assists OFF");
                    }
                    break;

                case TuningPreset.OffroadLifted:
                    Profile.TorqueMultiplier = 3.8f;
                    Profile.IdleTorqueMultiplier = 3.2f; // Heavy crawl pull
                    Profile.RpmMultiplier = 1.20f;
                    Profile.FinalDriveMultiplier = 1.40f; // Shorter crawl gearing
                    Profile.DiffPowerLock = 1.00f; // Fully locked 4x4 differentials
                    Profile.DiffCoastLock = 1.00f;
                    Profile.RideHeightOffset = +0.22f; // Lifted 8 inches
                    Profile.SpringStiffnessMultiplier = 0.65f; // Soft long travel
                    Profile.DamperMultiplier = 1.40f;
                    Profile.TireSizeMultiplier = 1.35f; // Huge mud-terrain tires
                    Profile.CustomSteerAngle = stock?.MaxSteerAngle ?? 35f;
                    Profile.TireForwardGripMultiplier = 2.2f;
                    Profile.TireDriftSlipMultiplier = 1.8f;
                    Profile.BrakeTorqueMultiplier = 2.5f;
                    Profile.BrakeBias = 0.55f;
                    Profile.HandbrakeTorqueMultiplier = 3.5f;
                    Profile.ChassisMassMultiplier = 1.35f;
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: OFFROAD CRAWLER", "3.8x Crawl Torque | +22cm Lifted | 1.35x Giant Tires | Locked 4x4 Diffs");
                    }
                    break;

                case TuningPreset.RallyCross:
                    Profile.TorqueMultiplier = 2.8f;
                    Profile.IdleTorqueMultiplier = 2.0f;
                    Profile.RpmMultiplier = 1.30f;
                    Profile.FlywheelInertiaMultiplier = 0.60f;
                    Profile.ShiftSpeedMultiplier = 0.40f;
                    Profile.DiffPowerLock = 0.75f;
                    Profile.DiffCoastLock = 0.50f;
                    Profile.RideHeightOffset = +0.06f; // Slight rally clearance
                    Profile.SpringStiffnessMultiplier = 1.15f;
                    Profile.DamperMultiplier = 1.70f;
                    Profile.CustomSteerAngle = (stock?.MaxSteerAngle ?? 35f) + 15f;
                    Profile.TireForwardGripMultiplier = 1.6f;
                    Profile.TireDriftSlipMultiplier = 0.90f;
                    Profile.BrakeTorqueMultiplier = 2.2f;
                    Profile.BrakeBias = 0.55f;
                    Profile.HandbrakeTorqueMultiplier = 4.0f;
                    Profile.TcsEnabled = false;
                    Profile.DownforceMultiplier = 0.6f;
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: RALLY CROSS", "2.8x Torque | Rally Clearance | Heavy Dampers | 50 deg Steer | Loose Surface Grip");
                    }
                    break;

                case TuningPreset.HyperInsane:
                    Profile.TorqueMultiplier = 5.5f;
                    Profile.IdleTorqueMultiplier = 4.0f;
                    Profile.RpmMultiplier = 2.00f;
                    Profile.FlywheelInertiaMultiplier = 0.25f; // Formula 1 style rev response
                    Profile.FinalDriveMultiplier = 0.75f; // Tall top-speed highway gearing (300+ km/h)
                    Profile.ShiftSpeedMultiplier = 0.10f; // Instantaneous dual-clutch shift
                    Profile.DiffPowerLock = 0.85f;
                    Profile.DiffCoastLock = 0.60f;
                    Profile.RideHeightOffset = -0.10f; // Slammed to the asphalt
                    Profile.SpringStiffnessMultiplier = 3.5f;
                    Profile.DamperMultiplier = 2.5f;
                    Profile.CustomSteerAngle = (stock?.MaxSteerAngle ?? 35f) + 20f;
                    Profile.TireForwardGripMultiplier = 3.0f; // Super sticky drag slicks
                    Profile.TireDriftSlipMultiplier = 2.2f;
                    Profile.BrakeTorqueMultiplier = 3.5f;
                    Profile.BrakeBias = 0.65f;
                    Profile.HandbrakeTorqueMultiplier = 5.0f;
                    Profile.CenterOfMassYOffset = -0.40f;
                    Profile.DownforceMultiplier = 2.2f; // High speed aero glue
                    if (showToast)
                    {
                        ExtendedWhipsUI.ShowToast("PRESET: HYPER OVERKILL", "5.5x Monster Torque | 2.0x Redline | 300+ km/h Gearing | Slammed Aero Downforce");
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
            Profile.TorqueMultiplier = Mathf.Clamp(mult, 0.2f, 10.0f);
            Profile.ActivePreset = TuningPreset.Custom;
            if (CurrentVehicle != null) ApplyTuningToInstance(CurrentVehicle);

            if (showToast)
            {
                var stock = CurrentStockSpecs;
                float nm = stock != null ? stock.PeakTorque * Profile.TorqueMultiplier : 0;
                ExtendedWhipsUI.ShowToast("TORQUE UPDATED", $"Torque: {Profile.TorqueMultiplier:F1}x ({nm:N0} Nm)");
            }
        }

        public static void ResetToStock(bool showToast = true)
        {
            ApplyPreset(TuningPreset.Stock, showToast);
        }

        public static void MarkCustom()
        {
            Profile.ActivePreset = TuningPreset.Custom;
            if (CurrentVehicle != null) ApplyTuningToInstance(CurrentVehicle);
        }

        private static void ApplyTuningToInstance(VPVehicleController v)
        {
            int vehicleId = v.GetInstanceID();
            if (!_stockCache.TryGetValue(vehicleId, out var stock)) return;

            // 1. Engine & Powertrain
            if (v.engine != null)
            {
                v.engine.peakRpmTorque = stock.PeakTorque * Profile.TorqueMultiplier;
                v.engine.idleRpmTorque = stock.IdleTorque * Profile.IdleTorqueMultiplier;
                v.engine.maxRpm = stock.MaxRpm * Profile.RpmMultiplier;
                v.engine.rpmLimiterMax = stock.RpmLimiter * Profile.RpmMultiplier;
                v.engine.idleRpm = stock.IdleRpm * Profile.IdleRpmMultiplier;
                v.engine.inertia = Mathf.Max(0.05f, stock.Inertia * Profile.FlywheelInertiaMultiplier);
                v.engine.frictionTorque = stock.FrictionTorque * Profile.CompressionBrakingMultiplier;
            }

            // 2. Differential & Transmission
            if (v.differential != null)
            {
                v.differential.gearRatio = stock.FinalDriveRatio * Profile.FinalDriveMultiplier;
                v.differential.powerRatio = Mathf.Clamp01(Profile.DiffPowerLock);
                v.differential.coastRatio = Mathf.Clamp01(Profile.DiffCoastLock);
                v.differential.preload = Profile.DiffPreload;
            }

            if (v.gearbox != null)
            {
                v.gearbox.manualShiftTime = Mathf.Max(0.02f, stock.ManualShiftTime * Profile.ShiftSpeedMultiplier);
                v.gearbox.automaticTransitionTime = Mathf.Max(0.02f, stock.AutoTransitionTime * Profile.ShiftSpeedMultiplier);
            }

            if (v.clutch != null)
            {
                v.clutch.maxTorqueTransfer = stock.ClutchMaxTorque * Profile.ClutchTorqueMultiplier;
            }

            // 3. Steering & Alignment
            if (v.steering != null)
            {
                v.steering.maxSteerAngle = Profile.CustomSteerAngle > 0f ? Profile.CustomSteerAngle : stock.MaxSteerAngle;
                v.steering.toeAngle = Profile.SteerToeAngle;
                v.steering.ackerman = Profile.AckermanSteering;
            }

            // 4. Brakes
            if (v.brakes != null)
            {
                v.brakes.maxBrakeTorque = stock.MaxBrakeTorque * Profile.BrakeTorqueMultiplier;
                v.brakes.brakeBias = Mathf.Clamp01(Profile.BrakeBias);
                v.brakes.handbrakeTorque = stock.HandbrakeTorque * Profile.HandbrakeTorqueMultiplier;
            }

            // 5. Driver Assists
            if (v.antiLock != null)
            {
                v.antiLock.enabled = Profile.AbsEnabled;
            }

            if (v.tractionControl != null)
            {
                v.tractionControl.enabled = Profile.TcsEnabled;
                v.tractionControl.ratio = Profile.TcsSlipRatio;
            }

            if (v.stabilityControl != null)
            {
                v.stabilityControl.enabled = Profile.EspEnabled;
            }

            // 6. Wheels & Suspension (VPWheelCollider)
            var vpWheels = v.GetComponentsInChildren<VPWheelCollider>();
            if (vpWheels != null && vpWheels.Length > 0 && stock.Wheels.Count == vpWheels.Length)
            {
                for (int i = 0; i < vpWheels.Length; i++)
                {
                    var w = vpWheels[i];
                    var sw = stock.Wheels[i];
                    w.suspensionDistance = Mathf.Max(0.05f, sw.SuspensionDistance + Profile.RideHeightOffset);
                    w.suspensionAnchor = sw.SuspensionAnchor + (Profile.RideHeightOffset * 0.5f);
                    w.springRate = sw.SpringRate * Profile.SpringStiffnessMultiplier;
                    w.damperRate = sw.DamperRate * Profile.DamperMultiplier;
                    w.radius = sw.Radius * Profile.TireSizeMultiplier;
                    w.mass = sw.Mass * Profile.WheelMassMultiplier;
                }
            }

            // 7. Tires & Grip (Standard WheelCollider forward/sideways friction)
            var unityWheels = v.GetComponentsInChildren<WheelCollider>();
            if (unityWheels != null)
            {
                foreach (var uw in unityWheels)
                {
                    var ff = uw.forwardFriction;
                    ff.stiffness = Profile.TireForwardGripMultiplier;
                    uw.forwardFriction = ff;

                    var sf = uw.sidewaysFriction;
                    sf.stiffness = 1.0f / Mathf.Max(0.1f, Profile.TireDriftSlipMultiplier);
                    uw.sidewaysFriction = sf;
                }
            }

            // 8. Chassis & Aerodynamics (Rigidbody)
            var rb = v.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = stock.ChassisMass * Profile.ChassisMassMultiplier;
                rb.centerOfMass = new Vector3(stock.CenterOfMass.x, stock.CenterOfMass.y + Profile.CenterOfMassYOffset, stock.CenterOfMass.z);

                // High-speed aerodynamic downforce
                if (Profile.DownforceMultiplier > 0.01f && v.speed > 5f)
                {
                    float force = v.speed * v.speed * Profile.DownforceMultiplier * 18f;
                    rb.AddForce(-v.transform.up * force, ForceMode.Force);
                }
            }
        }
    }
}
