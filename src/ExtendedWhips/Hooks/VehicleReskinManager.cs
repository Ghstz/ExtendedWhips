using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedWhips.Models;
using ExtendedWhips.Tuning;
using ExtendedWhips.UI;
using ExtendedWhips.Effects;
using ExtendedWhips.Cockpit;
using UnityEngine;

namespace ExtendedWhips.Hooks
{
    public class VehicleSkinState
    {
        public GlbVehicleModel? ActiveModel { get; set; }
        public LoadedVehicleAsset? LoadedAsset { get; set; }
        public List<Renderer> HiddenOemRenderers { get; set; } = new();
        public Vector3 StockDriverLocalPosition { get; set; } = Vector3.zero;
        public bool HasStockDriverPosition { get; set; } = false;

        // Cached component lookups (avoid per-frame IL2CPP GetComponents* allocations)
        public Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<VPWheelCollider>? WheelCache { get; set; }
        public Wb.Camera.WbMachineFirstPersonCameraBehaviour? FpCam { get; set; }

        // Baseline wheel hub Y positions for suspension delta tracking
        public float BaseHubY_FL { get; set; } = 0f;
        public float BaseHubY_FR { get; set; } = 0f;
        public float BaseHubY_RL { get; set; } = 0f;
        public float BaseHubY_RR { get; set; } = 0f;
        public bool HasBaseHubY { get; set; } = false;

        // Motorcycle Dynamics
        public float CurrentLeanAngle { get; set; } = 0f;

        // Interactive Showroom / Inspection
        public bool IsHoodOpen { get; set; } = false;
        public bool AreDoorsOpen { get; set; } = false;
        public bool IsTrunkOpen { get; set; } = false;

        // Paint & Finishes
        public Color CurrentPaintColor { get; set; } = Color.white;
        public float CurrentMetallic { get; set; } = 0.85f;
        public float CurrentSmoothness { get; set; } = 0.92f;
        public bool IsCarbon { get; set; } = false;
        public float WindowTint { get; set; } = 0.35f;
        public Color CurrentWheelColor { get; set; } = new Color(0.85f, 0.85f, 0.85f);
        public float CurrentWheelMetallic { get; set; } = 0.90f;
        public float CurrentWheelSmoothness { get; set; } = 0.90f;
    }

    public static class VehicleReskinManager
    {
        private static readonly Dictionary<int, VehicleSkinState> _skinsByInstance = new();

        public static GlbVehicleModel? GetActiveSkin(VPVehicleController? v)
        {
            if (v == null) return null;
            int id = v.GetInstanceID();
            return _skinsByInstance.TryGetValue(id, out var state) ? state.ActiveModel : null;
        }

        public static VehicleSkinState? GetState(VPVehicleController? v)
        {
            if (v == null) return null;
            int id = v.GetInstanceID();
            return _skinsByInstance.TryGetValue(id, out var state) ? state : null;
        }

        public static bool IsSkinActive(VPVehicleController? v)
        {
            return GetActiveSkin(v) != null;
        }

        public static bool ApplySkin(VPVehicleController v, GlbVehicleModel modelSpec)
        {
            if (v == null || modelSpec == null) return false;
            int id = v.GetInstanceID();

            try
            {
                if (_skinsByInstance.TryGetValue(id, out var existingState))
                {
                    if (existingState.LoadedAsset != null && existingState.LoadedAsset.RootObject != null)
                    {
                        UnityEngine.Object.Destroy(existingState.LoadedAsset.RootObject);
                    }
                }
                else
                {
                    existingState = new VehicleSkinState();
                    _skinsByInstance[id] = existingState;
                }

                var driverComp = v.GetComponentInChildren<Wb.WbDriverComponent>();
                Renderer? driverRend = null;
                try
                {
                    if (driverComp != null) driverRend = driverComp.rend;
                }
                catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

                if (existingState.HiddenOemRenderers.Count == 0)
                {
                    var renderers = v.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        if (r == null || !r.enabled) continue;

                        if (r == driverRend || r is SkinnedMeshRenderer)
                        {
                            Plugin.LogMsg($"[VehicleReskinManager] Preserved driver avatar mesh: {r.name}");
                            continue;
                        }

                        existingState.HiddenOemRenderers.Add(r);
                    }
                }

                foreach (var r in existingState.HiddenOemRenderers)
                {
                    if (r != null) r.enabled = false;
                }

                if (driverComp != null)
                {
                    try
                    {
                        driverComp.ShowDriver(true);
                        if (driverComp.rend != null) driverComp.rend.enabled = true;

                        if (!existingState.HasStockDriverPosition)
                        {
                            existingState.StockDriverLocalPosition = driverComp.transform.localPosition;
                            existingState.HasStockDriverPosition = true;
                        }

                        var stockPos = existingState.StockDriverLocalPosition;
                        driverComp.transform.localPosition = new Vector3(stockPos.x, stockPos.y + modelSpec.DriverYOffset, stockPos.z);
                    }
                    catch (System.Exception ex_) { Plugin.LogOnce(ex_); }
                }

                Material? sampleMat = null;
                foreach (var r in existingState.HiddenOemRenderers)
                {
                    if (r != null && r.sharedMaterial != null)
                    {
                        sampleMat = r.sharedMaterial;
                        break;
                    }
                }

                var asset = GlbVehicleLoader.LoadGlbModel(modelSpec, sampleMat);
                if (asset == null || asset.RootObject == null)
                {
                    Plugin.LogMsg($"[VehicleReskinManager] Failed to load 3D model: {modelSpec.DisplayName}");
                    foreach (var r in existingState.HiddenOemRenderers)
                    {
                        if (r != null) r.enabled = true;
                    }
                    return false;
                }

                asset.RootObject.transform.SetParent(v.transform, false);
                asset.RootObject.transform.localPosition = new Vector3(modelSpec.CenterOffsetX, modelSpec.HeightOffset, modelSpec.CenterOffsetZ);
                asset.RootObject.transform.localRotation = Quaternion.Euler(0f, modelSpec.RotationY, 0f);

                existingState.ActiveModel = modelSpec;
                existingState.LoadedAsset = asset;
                existingState.HasBaseHubY = false;
                existingState.CurrentLeanAngle = 0f;
                existingState.IsHoodOpen = false;
                existingState.AreDoorsOpen = false;
                existingState.IsTrunkOpen = false;

                DynamicSkidmarks.InitializeSkidmarks(v, asset);

                CockpitCameraAligner.ApplyCockpitOffset(v, modelSpec);

                var profile = VehicleTunerHook.Profile;
                profile.TireSizeMultiplier = modelSpec.WheelRadiusScale;
                
                if (modelSpec.Category == VehicleTypeCategory.Motorcycle)
                {
                    profile.AckermanSteering = false;
                    profile.CustomSteerAngle = 45f;
                    profile.TireForwardGripMultiplier = 1.6f;
                    profile.TireDriftSlipMultiplier = 1.4f;
                    profile.RideHeightOffset = -0.04f;
                }
                else if (modelSpec.Category == VehicleTypeCategory.Truck || modelSpec.Category == VehicleTypeCategory.Offroad4x4)
                {
                    profile.RideHeightOffset = +0.06f;
                }
                else
                {
                    profile.RideHeightOffset = -0.03f;
                }

                if (Enum.TryParse<TuningPreset>(modelSpec.RecommendedPreset, true, out var preset))
                {
                    VehicleTunerHook.ApplyPreset(preset, false);
                }

                VehicleTunerHook.ApplyTuningToInstance(v);
                ExtendedWhipsUI.ShowToast("BODY SWAP COMPLETE", $"Mounted {modelSpec.DisplayName} ({modelSpec.Category})");
                Plugin.LogMsg($"[VehicleReskinManager] Successfully applied body swap '{modelSpec.DisplayName}' to {v.name}");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.LogMsg($"[VehicleReskinManager] Error applying body swap: {ex}");
                return false;
            }
        }

        public static void UpdateWheels(VPVehicleController? v)
        {
            if (v == null) return;
            int id = v.GetInstanceID();
            if (!_skinsByInstance.TryGetValue(id, out var state) || state.LoadedAsset == null || state.ActiveModel == null)
                return;

            var asset = state.LoadedAsset;
            var spec = state.ActiveModel;

            var vpWheels = state.WheelCache;
            if (vpWheels == null || vpWheels.Length == 0 || vpWheels[0] == null)
                vpWheels = state.WheelCache = v.GetComponentsInChildren<VPWheelCollider>();
            VPWheelCollider? wheelFL = null;
            VPWheelCollider? wheelFR = null;
            VPWheelCollider? wheelRL = null;
            VPWheelCollider? wheelRR = null;

            if (vpWheels != null && vpWheels.Length > 0)
            {
                foreach (var w in vpWheels)
                {
                    Vector3 lPos = v.transform.InverseTransformPoint(w.transform.position);
                    if (lPos.z > 0f)
                    {
                        if (lPos.x < 0f && wheelFL == null) wheelFL = w;
                        else if (lPos.x >= 0f && wheelFR == null) wheelFR = w;
                    }
                    else
                    {
                        if (lPos.x < 0f && wheelRL == null) wheelRL = w;
                        else if (lPos.x >= 0f && wheelRR == null) wheelRR = w;
                    }
                }
                wheelFL ??= vpWheels[0];
                wheelFR ??= (vpWheels.Length > 1 ? vpWheels[1] : wheelFL);
                wheelRL ??= (vpWheels.Length > 2 ? vpWheels[2] : wheelFL);
                wheelRR ??= (vpWheels.Length > 3 ? vpWheels[3] : wheelFR);
            }

            if (!state.HasBaseHubY && wheelFL != null && wheelFR != null && wheelRL != null && wheelRR != null)
            {
                state.BaseHubY_FL = v.transform.InverseTransformPoint(wheelFL.wheelTransform != null ? wheelFL.wheelTransform.position : wheelFL.transform.position).y;
                state.BaseHubY_FR = v.transform.InverseTransformPoint(wheelFR.wheelTransform != null ? wheelFR.wheelTransform.position : wheelFR.transform.position).y;
                state.BaseHubY_RL = v.transform.InverseTransformPoint(wheelRL.wheelTransform != null ? wheelRL.wheelTransform.position : wheelRL.transform.position).y;
                state.BaseHubY_RR = v.transform.InverseTransformPoint(wheelRR.wheelTransform != null ? wheelRR.wheelTransform.position : wheelRR.transform.position).y;
                state.HasBaseHubY = true;
            }

            float deltaY_FL = 0f, deltaY_FR = 0f, deltaY_RL = 0f, deltaY_RR = 0f;
            if (state.HasBaseHubY)
            {
                if (wheelFL?.wheelTransform != null) deltaY_FL = v.transform.InverseTransformPoint(wheelFL.wheelTransform.position).y - state.BaseHubY_FL;
                if (wheelFR?.wheelTransform != null) deltaY_FR = v.transform.InverseTransformPoint(wheelFR.wheelTransform.position).y - state.BaseHubY_FR;
                if (wheelRL?.wheelTransform != null) deltaY_RL = v.transform.InverseTransformPoint(wheelRL.wheelTransform.position).y - state.BaseHubY_RL;
                if (wheelRR?.wheelTransform != null) deltaY_RR = v.transform.InverseTransformPoint(wheelRR.wheelTransform.position).y - state.BaseHubY_RR;
            }

            float throttle = 0f;
            float brake = 0f;
            try
            {
                if (v.data != null)
                {
                    throttle = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Throttle) / 10000f;
                    brake = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Brake) / 10000f;
                }
            }
            catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            float squatRear = -Mathf.Clamp(throttle * 0.035f, 0f, 0.045f);
            float liftFront = +Mathf.Clamp(throttle * 0.015f, 0f, 0.020f);
            float diveFront = -Mathf.Clamp(brake * 0.030f, 0f, 0.040f);
            float liftRear = +Mathf.Clamp(brake * 0.020f, 0f, 0.025f);

            float totalFL_Y = deltaY_FL + liftFront + diveFront;
            float totalFR_Y = deltaY_FR + liftFront + diveFront;
            float totalRL_Y = deltaY_RL + squatRear + liftRear;
            float totalRR_Y = deltaY_RR + squatRear + liftRear;

            float steerFL = wheelFL != null ? wheelFL.steerAngle : 0f;
            float steerFR = wheelFR != null ? wheelFR.steerAngle : 0f;
            float spinFL = wheelFL != null ? wheelFL.angularPosition * Mathf.Rad2Deg : 0f;
            float spinFR = wheelFR != null ? wheelFR.angularPosition * Mathf.Rad2Deg : 0f;
            float spinRL = wheelRL != null ? wheelRL.angularPosition * Mathf.Rad2Deg : 0f;
            float spinRR = wheelRR != null ? wheelRR.angularPosition * Mathf.Rad2Deg : 0f;

            // 1. Spinning Rims & Tires (Spin + Steer + Dynamic Suspension Travel)
            UpdateWheelQuadrant(asset.FrontLeftWheels, asset.WheelRestLocalPositions, totalFL_Y, spinFL, steerFL);
            UpdateWheelQuadrant(asset.FrontRightWheels, asset.WheelRestLocalPositions, totalFR_Y, spinFR, steerFR);
            UpdateWheelQuadrant(asset.RearLeftWheels, asset.WheelRestLocalPositions, totalRL_Y, spinRL, 0f);
            UpdateWheelQuadrant(asset.RearRightWheels, asset.WheelRestLocalPositions, totalRR_Y, spinRR, 0f);

            // 2. Fixed Brake Calipers (Steer only, NO spin!)
            UpdateWheelQuadrant(asset.FrontLeftCalipers, asset.WheelRestLocalPositions, totalFL_Y, 0f, steerFL);
            UpdateWheelQuadrant(asset.FrontRightCalipers, asset.WheelRestLocalPositions, totalFR_Y, 0f, steerFR);
            UpdateWheelQuadrant(asset.RearLeftCalipers, asset.WheelRestLocalPositions, totalRL_Y, 0f, 0f);
            UpdateWheelQuadrant(asset.RearRightCalipers, asset.WheelRestLocalPositions, totalRR_Y, 0f, 0f);

            // 3. Motorcycle Lean Physics (Feature 5)
            if (spec.Category == VehicleTypeCategory.Motorcycle)
            {
                float speed = v.speed;
                var rb = v.GetComponent<Rigidbody>();
                float yawRate = rb != null ? rb.angularVelocity.y : 0f;
                float targetLean = 0f;

                if (Mathf.Abs(speed) > 0.8f)
                {
                    float latG = speed * yawRate;
                    targetLean = -Mathf.Clamp(latG * 3.8f, -42.0f, 42.0f);
                }
                else
                {
                    targetLean = -6.5f;
                }

                state.CurrentLeanAngle = Mathf.Lerp(state.CurrentLeanAngle, targetLean, Time.deltaTime * 6.5f);
                asset.RootObject.transform.localRotation = Quaternion.Euler(0f, spec.RotationY, state.CurrentLeanAngle);
            }

            // Smooth Air Ride Stance Height transition (Feature 9)
            float targetRootY = spec.HeightOffset + asset.AirRideHeightOffset;
            var curPos = asset.RootObject.transform.localPosition;
            asset.RootObject.transform.localPosition = new Vector3(
                spec.CenterOffsetX, 
                Mathf.MoveTowards(curPos.y, targetRootY, Time.deltaTime * 0.25f), 
                spec.CenterOffsetZ
            );

            // 4. Interior Steering Wheel Rotation Sync (Feature 3)
            CockpitCameraAligner.UpdateSteeringWheel(v, asset);

            // 5. Lighting Integration (Feature 2)
            CustomVehicleLights.UpdateLights(v, asset);

            // 6. Exhaust Smoke & Afterfire Pops (Feature 4)
            CustomVehicleExhaust.UpdateExhaust(v, asset);

            // 7. Engine Sound Synthesizer (Feature 1: BOV, Supercharger, Jake Brake)
            CustomVehicleAudio.UpdateAudio(v, asset);

            // 8. Nitrous Oxide & Fender Steam Purge (Feature 2)
            CustomVehicleNitrous.UpdateNitrous(v, asset);

            // 9. Forza Drag Telemetry Stopwatch (Feature 3)
            DragTelemetryBenchmark.UpdateBenchmark(v);

            // 10. Burnout Smoke & Roostertails (Feature 4)
            CustomSurfaceFX.UpdateSurfaceFX(v, asset);

            // 11. Cockpit Gauge Needles (Feature 6)
            CockpitGaugeSync.UpdateGauges(v, asset);

            // 12. Active Aerodynamics & Airbrake Spoiler (Feature 7)
            UpdateActiveAerodynamics(v, state, asset);

            // 13. Dynamic Construction Mud Accumulation (Feature 10)
            MudAccumulationSystem.UpdateMud(v, asset);

            // 14. Showroom & Inspection Panel Animations
            UpdateShowroomPanels(state, asset);

            // 15. Emergency & Escort Strobes + Sirens
            CustomEmergencyLighting.UpdateEmergencyLights(v, asset);

            // 16. 2-Step Launch Control & Anti-Lag
            LaunchControlSystem.UpdateLaunchControl(v, asset);

            // 17. Dynamic Skidmarks on Asphalt
            DynamicSkidmarks.UpdateSkidmarks(v);

            // 18. Formula Drift 70Â° Angle Kit & Gyro Assist
            DriftAssistSystem.UpdateDriftAssist(v);

            // 19. In-Cabin Stereo with Acoustic Filtering
            if (state.FpCam == null) state.FpCam = v.GetComponentInChildren<Wb.Camera.WbMachineFirstPersonCameraBehaviour>(true);
            bool isCockpit = state.FpCam != null && state.FpCam.enabled;
            InCabinStereo.UpdateStereo(v, isCockpit);

            // 20. Instant EV Powertrain & Cyber Sound
            EvPowertrainSystem.UpdateEvPowertrain(v);

            // 21. Controller Haptic Feedback
            ControllerHaptics.UpdateHaptics(v);

            // 22. Visual Wear & Scratches
            VehicleWearSystem.UpdateWear(v, asset);
        }

        private static void UpdateActiveAerodynamics(VPVehicleController v, VehicleSkinState state, LoadedVehicleAsset asset)
        {
            if (asset.SpoilerTransforms.Count == 0) return;

            float speedKmh = Mathf.Abs(v.speed * 3.6f);
            float brake = 0f;
            try
            {
                if (v.data != null)
                {
                    brake = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Brake) / 10000f;
                }
            }
            catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            bool isAirbrake = (speedKmh > 75f && brake > 0.25f);
            float targetAngle = isAirbrake ? -28f : 0f;

            foreach (var t in asset.SpoilerTransforms)
            {
                if (t == null) continue;
                if (asset.SpoilerRestRotations.TryGetValue(t, out var restRot))
                {
                    t.localRotation = Quaternion.Slerp(t.localRotation, restRot * Quaternion.Euler(targetAngle, 0f, 0f), Time.deltaTime * 7f);
                }
            }
        }

        private static void UpdateWheelQuadrant(List<Transform> wheelTransforms, Dictionary<Transform, Vector3> restPositions, float deltaY, float spinAngle, float steerAngle)
        {
            for (int i = 0; i < wheelTransforms.Count; i++)
            {
                var t = wheelTransforms[i];
                if (t == null) continue;

                if (restPositions.TryGetValue(t, out var restPos))
                {
                    t.localPosition = new Vector3(restPos.x, restPos.y + deltaY, restPos.z);
                }
                t.localRotation = Quaternion.Euler(spinAngle, steerAngle, 0f);
            }
        }

        private static void UpdateShowroomPanels(VehicleSkinState state, LoadedVehicleAsset asset)
        {
            float targetHoodAngle = state.IsHoodOpen ? -48f : 0f;
            foreach (var t in asset.HoodTransforms)
            {
                if (t == null) continue;
                if (asset.HoodRestRotations.TryGetValue(t, out var restRot))
                {
                    t.localRotation = Quaternion.Slerp(t.localRotation, restRot * Quaternion.Euler(targetHoodAngle, 0f, 0f), Time.deltaTime * 5f);
                }
            }

            float targetDoorL = state.AreDoorsOpen ? +55f : 0f;
            foreach (var t in asset.LeftDoorTransforms)
            {
                if (t == null) continue;
                if (asset.LeftDoorRestRotations.TryGetValue(t, out var restRot))
                {
                    t.localRotation = Quaternion.Slerp(t.localRotation, restRot * Quaternion.Euler(0f, targetDoorL, 0f), Time.deltaTime * 5f);
                }
            }

            float targetDoorR = state.AreDoorsOpen ? -55f : 0f;
            foreach (var t in asset.RightDoorTransforms)
            {
                if (t == null) continue;
                if (asset.RightDoorRestRotations.TryGetValue(t, out var restRot))
                {
                    t.localRotation = Quaternion.Slerp(t.localRotation, restRot * Quaternion.Euler(0f, targetDoorR, 0f), Time.deltaTime * 5f);
                }
            }

            float targetTrunk = state.IsTrunkOpen ? -45f : 0f;
            foreach (var t in asset.TrunkTransforms)
            {
                if (t == null) continue;
                if (asset.TrunkRestRotations.TryGetValue(t, out var restRot))
                {
                    t.localRotation = Quaternion.Slerp(t.localRotation, restRot * Quaternion.Euler(targetTrunk, 0f, 0f), Time.deltaTime * 5f);
                }
            }
        }

        // =========================================================================
        // FEATURE 6: IN-GAME PAINT & CUSTOMIZATION STUDIO API
        // =========================================================================
        public static void ApplyPaint(VPVehicleController v, Color color, float metallic, float smoothness, bool isCarbon)
        {
            var state = GetState(v);
            if (state == null || state.LoadedAsset == null) return;

            state.CurrentPaintColor = color;
            state.CurrentMetallic = metallic;
            state.CurrentSmoothness = smoothness;
            state.IsCarbon = isCarbon;

            foreach (var mat in state.LoadedAsset.BodyMaterials)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            }
            MudAccumulationSystem.MarkVisualsDirty();
            ExtendedWhipsUI.ShowToast("PAINT SHOP", "Applied custom body finish to vehicle.");
        }

        public static void ApplyWindowTint(VPVehicleController v, float alpha)
        {
            var state = GetState(v);
            if (state == null || state.LoadedAsset == null) return;

            state.WindowTint = alpha;
            Color tintCol = new Color(0.05f, 0.05f, 0.06f, alpha);

            foreach (var mat in state.LoadedAsset.GlassMaterials)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tintCol);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", tintCol);
            }
        }

        public static void ApplyWheelFinish(VPVehicleController v, Color color, float metallic, float smoothness)
        {
            var state = GetState(v);
            if (state == null || state.LoadedAsset == null) return;

            state.CurrentWheelColor = color;
            state.CurrentWheelMetallic = metallic;
            state.CurrentWheelSmoothness = smoothness;

            foreach (var mat in state.LoadedAsset.WheelMaterials)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            }
            ExtendedWhipsUI.ShowToast("WHEEL FINISH", "Applied custom rim finish.");
        }

        // =========================================================================
        // FEATURE 7: INTERACTIVE SHOWROOM / INSPECTION API
        // =========================================================================
        public static void ToggleHood(VPVehicleController v)
        {
            var state = GetState(v);
            if (state == null) return;
            state.IsHoodOpen = !state.IsHoodOpen;
            ExtendedWhipsUI.ShowToast("INSPECTION MODE", state.IsHoodOpen ? "Engine Bay Opened (Hood Popped)" : "Hood Closed");
        }

        public static void ToggleDoors(VPVehicleController v)
        {
            var state = GetState(v);
            if (state == null) return;
            state.AreDoorsOpen = !state.AreDoorsOpen;
            ExtendedWhipsUI.ShowToast("INSPECTION MODE", state.AreDoorsOpen ? "Cabin Doors Opened" : "Doors Closed");
        }

        public static void ToggleTrunk(VPVehicleController v)
        {
            var state = GetState(v);
            if (state == null) return;
            state.IsTrunkOpen = !state.IsTrunkOpen;
            ExtendedWhipsUI.ShowToast("INSPECTION MODE", state.IsTrunkOpen ? "Trunk / Tailgate Opened" : "Trunk Closed");
        }

        public static void CloseAllPanels(VPVehicleController v)
        {
            var state = GetState(v);
            if (state == null) return;
            state.IsHoodOpen = false;
            state.AreDoorsOpen = false;
            state.IsTrunkOpen = false;
            ExtendedWhipsUI.ShowToast("INSPECTION MODE", "All Showroom Panels Closed");
        }

        // =========================================================================
        // FEATURE 8: LIVERY & DECAL PRESETS
        // =========================================================================
        public static void ApplyLivery(VPVehicleController v, string liveryPreset)
        {
            var state = GetState(v);
            if (state == null || state.LoadedAsset == null) return;

            state.LoadedAsset.ActiveLiveryName = liveryPreset;

            switch (liveryPreset)
            {
                case "R34_FastFurious":
                    ApplyPaint(v, new Color(0.85f, 0.88f, 0.92f), 0.95f, 0.95f, false);
                    ApplyWheelFinish(v, new Color(0.95f, 0.95f, 0.98f), 0.98f, 0.98f);
                    ExtendedWhipsUI.ShowToast("LIVERY APPLIED", "Brian O'Conner F&F Silver/Blue GT-R Spec");
                    break;

                case "Cuda_HemiBillboard":
                    ApplyPaint(v, new Color(0.95f, 0.28f, 0.05f), 0.80f, 0.90f, false);
                    ApplyWheelFinish(v, new Color(0.12f, 0.12f, 0.12f), 0.40f, 0.60f);
                    ExtendedWhipsUI.ShowToast("LIVERY APPLIED", "426 HEMI Billboard Muscle Spec");
                    break;

                case "BMW_Motorsport":
                    ApplyPaint(v, new Color(0.96f, 0.96f, 0.98f), 0.85f, 0.95f, false);
                    ApplyWheelFinish(v, new Color(0.20f, 0.20f, 0.22f), 0.85f, 0.90f);
                    ExtendedWhipsUI.ShowToast("LIVERY APPLIED", "BMW M Motorsport Factory Track Livery");
                    break;

                case "Raptor_FoxBaja":
                    ApplyPaint(v, new Color(0.92f, 0.45f, 0.08f), 0.70f, 0.80f, false);
                    ApplyWheelFinish(v, new Color(0.55f, 0.38f, 0.22f), 0.80f, 0.65f);
                    ExtendedWhipsUI.ShowToast("LIVERY APPLIED", "Fox Factory Baja Overland Trophy Spec");
                    break;

                case "Clean_OEM":
                default:
                    Color oemCol = new Color(0.85f, 0.85f, 0.85f);
                    ApplyPaint(v, oemCol, 0.85f, 0.92f, false);
                    ExtendedWhipsUI.ShowToast("LIVERY APPLIED", "Clean Factory Stock Spec");
                    break;
            }
        }

        // =========================================================================
        // FEATURE 9: ON-THE-FLY AIR RIDE / STANCE HYDRAULICS
        // =========================================================================
        public enum AirRideStance
        {
            Slammed,
            Street,
            Lifted
        }

        public static void SetAirRideStance(VPVehicleController v, AirRideStance stance)
        {
            var state = GetState(v);
            if (state == null || state.LoadedAsset == null) return;

            float offset = stance switch
            {
                AirRideStance.Slammed => -0.07f,
                AirRideStance.Street => 0.00f,
                AirRideStance.Lifted => +0.10f,
                _ => 0.00f
            };

            state.LoadedAsset.AirRideHeightOffset = offset;
            ExtendedWhipsUI.ShowToast("AIR RIDE HYDRAULICS", $"Set stance: [{stance.ToString().ToUpper()}]");
        }

        // =========================================================================
        // FEATURE 5: WORKHORSE TOW MODE & HEAVY HAUL
        // =========================================================================
        public static void SetTowMode(VPVehicleController v, bool enabled)
        {
            var profile = VehicleTunerHook.Profile;
            if (enabled)
            {
                profile.TorqueMultiplier = Mathf.Max(profile.TorqueMultiplier, 2.2f);
                profile.DiffPowerLock = 0.85f;
                profile.RideHeightOffset = +0.08f;
                ExtendedWhipsUI.ShowToast("TOW MODE ENGAGED", "Heavy-haul torque (+120%) and locked towing differential active.");
            }
            else
            {
                profile.TorqueMultiplier = 1.0f;
                ExtendedWhipsUI.ShowToast("TOW MODE DISENGAGED", "Returned to standard driving profile.");
            }
            VehicleTunerHook.MarkCustom();
        }

        // =========================================================================
        // FEATURE 10: DETAIL / PRESSURE WASH SERVICE
        // =========================================================================
        public static void WashActiveVehicle(VPVehicleController v)
        {
            var state = GetState(v);
            if (state == null || state.LoadedAsset == null) return;
            MudAccumulationSystem.WashVehicle(state.LoadedAsset, state.CurrentPaintColor, state.CurrentSmoothness);
        }

        public static void RestoreStockSkin(VPVehicleController v)
        {
            if (v == null) return;
            int id = v.GetInstanceID();

            if (_skinsByInstance.TryGetValue(id, out var state))
            {
                if (state.LoadedAsset != null && state.LoadedAsset.RootObject != null)
                {
                    UnityEngine.Object.Destroy(state.LoadedAsset.RootObject);
                }

                foreach (var r in state.HiddenOemRenderers)
                {
                    if (r != null) r.enabled = true;
                }

                if (state.HasStockDriverPosition)
                {
                    try
                    {
                        var driverComp = v.GetComponentInChildren<Wb.WbDriverComponent>();
                        if (driverComp != null)
                        {
                            driverComp.transform.localPosition = state.StockDriverLocalPosition;
                        }
                    }
                    catch (System.Exception ex_) { Plugin.LogOnce(ex_); }
                }

                CockpitCameraAligner.RestoreCockpitCamera(v);

                state.ActiveModel = null;
                state.LoadedAsset = null;
                _skinsByInstance.Remove(id);

                VehicleTunerHook.ApplyPreset(TuningPreset.Stock);
                ExtendedWhipsUI.ShowToast("OEM RESTORED", "Reinstalled factory stock body and OEM visuals.");
            }
        }

        public static void QuickRepairActiveVehicle()
        {
            var v = VehicleTunerHook.CurrentVehicle;
            if (v == null) return;
            int id = v.GetInstanceID();

            if (_skinsByInstance.TryGetValue(id, out var state) && state.LoadedAsset != null)
            {
                VehicleWearSystem.QuickRepairAndDetail(state.LoadedAsset);
                ExtendedWhipsUI.ShowToast("VEHICLE DETAILED", "Body scratches buffed & panels restored to mint condition!");
            }
        }
    }
}
