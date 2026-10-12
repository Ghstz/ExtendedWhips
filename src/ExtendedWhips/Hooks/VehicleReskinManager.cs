using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedWhips.Models;
using ExtendedWhips.Tuning;
using ExtendedWhips.UI;
using UnityEngine;

namespace ExtendedWhips.Hooks
{
    public class VehicleSkinState
    {
        public GlbVehicleModel? ActiveModel { get; set; }
        public LoadedVehicleAsset? LoadedAsset { get; set; }
        public List<Renderer> HiddenOemRenderers { get; set; } = new();
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
                // Clean up previous custom skin if any
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

                // Collect OEM renderers on first reskin
                if (existingState.HiddenOemRenderers.Count == 0)
                {
                    var renderers = v.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        if (r != null && r.enabled)
                        {
                            existingState.HiddenOemRenderers.Add(r);
                        }
                    }
                }

                // Hide OEM renderers
                foreach (var r in existingState.HiddenOemRenderers)
                {
                    if (r != null) r.enabled = false;
                }

                // Find a sample material from OEM to borrow valid HDRP shaders
                Material? sampleMat = null;
                foreach (var r in existingState.HiddenOemRenderers)
                {
                    if (r != null && r.sharedMaterial != null)
                    {
                        sampleMat = r.sharedMaterial;
                        break;
                    }
                }

                // Load the new 3D model
                var asset = GlbVehicleLoader.LoadGlbModel(modelSpec, sampleMat);
                if (asset == null || asset.RootObject == null)
                {
                    Plugin.LogMsg($"[VehicleReskinManager] Failed to load 3D model: {modelSpec.DisplayName}");
                    // Restore OEM renderers if load failed
                    foreach (var r in existingState.HiddenOemRenderers)
                    {
                        if (r != null) r.enabled = true;
                    }
                    return false;
                }

                // Attach to vehicle root
                asset.RootObject.transform.SetParent(v.transform, false);
                asset.RootObject.transform.localPosition = new Vector3(0f, modelSpec.HeightOffset, 0f);
                asset.RootObject.transform.localRotation = Quaternion.identity;

                existingState.ActiveModel = modelSpec;
                existingState.LoadedAsset = asset;

                // Adjust vehicle physics & track width
                var profile = VehicleTunerHook.Profile;
                profile.TireSizeMultiplier = modelSpec.WheelRadiusScale;
                
                if (modelSpec.Category == VehicleTypeCategory.Motorcycle)
                {
                    // Motorcycle narrow track & stance
                    profile.AckermanSteering = false;
                    profile.CustomSteerAngle = 45f;
                    profile.TireForwardGripMultiplier = 1.6f;
                    profile.TireDriftSlipMultiplier = 1.4f;
                    profile.RideHeightOffset = -0.05f;
                }
                else if (modelSpec.Category == VehicleTypeCategory.Truck || modelSpec.Category == VehicleTypeCategory.Offroad4x4)
                {
                    profile.RideHeightOffset = modelSpec.HeightOffset;
                }

                // Apply recommended preset
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

                state.ActiveModel = null;
                state.LoadedAsset = null;
                _skinsByInstance.Remove(id);

                VehicleTunerHook.ApplyPreset(TuningPreset.Stock);
                ExtendedWhipsUI.ShowToast("OEM RESTORED", "Reinstalled factory stock body and OEM visuals.");
            }
        }
    }
}
