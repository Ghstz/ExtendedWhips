using System;
using System.Collections.Generic;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Cockpit
{
    public static class CockpitCameraAligner
    {
        private static readonly Dictionary<int, Vector3> _stockCameraPositions = new();
        private static readonly Dictionary<int, bool> _hasStockCamera = new();

        public static void ApplyCockpitOffset(VPVehicleController v, GlbVehicleModel spec)
        {
            if (v == null || spec == null) return;
            int id = v.GetInstanceID();

            try
            {
                var fpCam = v.GetComponentInChildren<Wb.Camera.WbMachineFirstPersonCameraBehaviour>();
                if (fpCam != null)
                {
                    if (!_hasStockCamera.ContainsKey(id))
                    {
                        _stockCameraPositions[id] = fpCam.defaultPosition;
                        _hasStockCamera[id] = true;
                    }

                    var stockPos = _stockCameraPositions[id];
                    Vector3 targetPos;

                    if (spec.CockpitCameraOffset != Vector3.zero)
                    {
                        targetPos = stockPos + spec.CockpitCameraOffset;
                    }
                    else
                    {
                        float yOffset = spec.DriverYOffset != 0f ? spec.DriverYOffset : 
                            (spec.Category == VehicleTypeCategory.Motorcycle ? -0.45f : 
                             spec.Category == VehicleTypeCategory.SportsCar || spec.Category == VehicleTypeCategory.MuscleCar ? -0.38f : 0.0f);
                        targetPos = new Vector3(stockPos.x, stockPos.y + yOffset, stockPos.z);
                    }

                    fpCam.defaultPosition = targetPos;
                    fpCam.targetLocalPosition = targetPos;
                }
            }
            catch (Exception ex)
            {
                Plugin.LogMsg($"[CockpitCameraAligner] Error setting cockpit camera: {ex.Message}");
            }
        }

        public static void RestoreCockpitCamera(VPVehicleController v)
        {
            if (v == null) return;
            int id = v.GetInstanceID();

            try
            {
                if (_hasStockCamera.TryGetValue(id, out bool hasStock) && hasStock && _stockCameraPositions.TryGetValue(id, out var stockPos))
                {
                    var fpCam = v.GetComponentInChildren<Wb.Camera.WbMachineFirstPersonCameraBehaviour>();
                    if (fpCam != null)
                    {
                        fpCam.defaultPosition = stockPos;
                        fpCam.targetLocalPosition = stockPos;
                    }
                    _hasStockCamera.Remove(id);
                    _stockCameraPositions.Remove(id);
                }
            }
            catch (System.Exception ex_) { Plugin.LogOnce(ex_); }
        }

        public static void UpdateSteeringWheel(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null || asset.SteeringWheelTransforms.Count == 0) return;

            float steerRotation = 0f;
            try
            {
                var sw = v.GetComponentInChildren<Wb.WbMachineInteriorSteeringWheel>();
                if (sw != null)
                {
                    steerRotation = sw.CurrentRotationValue;
                }
                else
                {
                    var vpWheels = v.GetComponentsInChildren<VPWheelCollider>();
                    if (vpWheels != null && vpWheels.Length > 0)
                    {
                        steerRotation = -vpWheels[0].steerAngle * 12.0f;
                    }
                }
            }
            catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            foreach (var t in asset.SteeringWheelTransforms)
            {
                if (t == null) continue;
                if (!asset.SteeringWheelRestRotations.TryGetValue(t, out var restRot))
                {
                    restRot = t.localRotation;
                    asset.SteeringWheelRestRotations[t] = restRot;
                }

                t.localRotation = restRot * Quaternion.Euler(0f, 0f, steerRotation);
            }
        }
    }
}

