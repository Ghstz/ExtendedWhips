using System;
using System.Collections.Generic;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class DynamicSkidmarks
    {
        private class WheelSkidInfo
        {
            public VPWheelCollider Wheel = null!;
            public TrailRenderer Trail = null!;
        }

        private static readonly List<WheelSkidInfo> _activeTrails = new();
        private static Material? _skidMaterial;

        public static void InitializeSkidmarks(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            foreach (var info in _activeTrails)
            {
                if (info.Trail != null) UnityEngine.Object.Destroy(info.Trail.gameObject);
            }
            _activeTrails.Clear();

            var wheels = v.GetComponentsInChildren<VPWheelCollider>();
            if (wheels == null || wheels.Length == 0) return;

            if (_skidMaterial == null)
            {
                var baseMat = CustomSurfaceFX.GetParticleMaterial();
                if (baseMat != null)
                {
                    _skidMaterial = new Material(baseMat)
                    {
                        color = new Color(0.12f, 0.12f, 0.12f, 0.75f)
                    };
                }
                else
                {
                    var shader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
                    if (shader != null)
                    {
                        _skidMaterial = new Material(shader)
                        {
                            color = new Color(0.12f, 0.12f, 0.12f, 0.75f)
                        };
                    }
                }
            }

            foreach (var w in wheels)
            {
                var trailObj = new GameObject("TireSkidTrail");
                trailObj.transform.SetParent(w.transform, false);
                trailObj.transform.localPosition = new Vector3(0f, -w.radius + 0.02f, 0f);

                var tr = trailObj.AddComponent<TrailRenderer>();
                tr.material = _skidMaterial;
                tr.startWidth = 0.28f * asset.Spec.WheelRadiusScale;
                tr.endWidth = 0.26f * asset.Spec.WheelRadiusScale;
                tr.time = 25.0f; // Skidmark persists on the road for 25 seconds
                tr.startColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
                tr.endColor = new Color(0.08f, 0.08f, 0.08f, 0.0f);
                tr.emitting = false;
                tr.alignment = LineAlignment.TransformZ;

                _activeTrails.Add(new WheelSkidInfo { Wheel = w, Trail = tr });
            }
        }

        public static void UpdateSkidmarks(VPVehicleController v)
        {
            if (v == null || _activeTrails.Count == 0) return;

            float speedKmh = Mathf.Abs(v.speed * 3.6f);
            float driftAngle = 0f;
            if (v.cachedRigidbody != null && v.cachedRigidbody.velocity.magnitude > 2.0f)
            {
                driftAngle = Vector3.Angle(v.transform.forward, v.cachedRigidbody.velocity);
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

            for (int i = 0; i < _activeTrails.Count; i++)
            {
                var info = _activeTrails[i];
                if (info.Wheel == null || info.Trail == null) continue;

                float angularVel = Mathf.Abs(info.Wheel.angularVelocity);

                // High slip condition: burnout, brake lockup, or lateral drift
                bool isSlipping = (throttle > 0.45f && speedKmh < 22f && angularVel > 22.0f) ||
                                  (brake > 0.65f && speedKmh > 12f && angularVel < 6.0f) ||
                                  (driftAngle > 9.0f && speedKmh > 14f);

                info.Trail.emitting = isSlipping;
            }
        }
    }
}

