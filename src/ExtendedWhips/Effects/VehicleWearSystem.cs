using System;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class VehicleWearSystem
    {
        public static float WearPercentage { get; private set; } = 0f; // 0% (Mint) to 100% (Heavily Battle Scuffed)
        private static Vector3 _lastVelocity = Vector3.zero;

        private static int _lastVehicleId = 0;
        private static float _lastImpactTime = -10f;

        public static void UpdateWear(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || v.cachedRigidbody == null || asset == null) return;
            if (Time.timeScale <= 0f) return;

            Vector3 currentVel = v.cachedRigidbody.velocity;
            int id = v.GetInstanceID();
            if (id != _lastVehicleId)
            {
                // New vehicle: re-baseline so we don't register a fake impact
                _lastVehicleId = id;
                _lastVelocity = currentVel;
                return;
            }

            Vector3 deltaVel = currentVel - _lastVelocity;
            _lastVelocity = currentVel;

            // Detect heavy impact / collision impulse (ignore gravity)
            deltaVel.y = 0f;
            float impactMagnitude = deltaVel.magnitude;

            // One collision spans several physics steps; only count it once
            if (impactMagnitude > 4.5f && Time.time - _lastImpactTime > 0.5f) // Hard hit against obstacle or equipment
            {
                _lastImpactTime = Time.time;
                float damage = Mathf.Clamp(impactMagnitude * 3.5f, 5.0f, 25.0f);
                WearPercentage = Mathf.Clamp(WearPercentage + damage, 0f, 100f);
                ApplyWearVisuals(asset);
            }
        }

        private static void ApplyWearVisuals(LoadedVehicleAsset asset)
        {
            if (asset.BodyMaterials.Count == 0) return;

            float scuffRatio = WearPercentage / 100f;
            foreach (var mat in asset.BodyMaterials)
            {
                if (mat == null) continue;

                // Modulate material roughness / glossiness if standard shader
                float gloss = Mathf.Lerp(0.85f, 0.15f, scuffRatio);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", gloss);
                if (mat.HasProperty("_Metallic"))
                {
                    mat.SetFloat("_Metallic", Mathf.Lerp(0.5f, 0.05f, scuffRatio));
                }
            }
        }

        public static void QuickRepairAndDetail(LoadedVehicleAsset asset)
        {
            WearPercentage = 0f;
            if (asset != null)
            {
                foreach (var mat in asset.BodyMaterials)
                {
                    if (mat == null) continue;
                    if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.90f);
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.90f);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.65f);
                }
            }
        }
    }
}

