using System;
using System.Collections.Generic;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Cockpit
{
    public static class CockpitGaugeSync
    {
        public static void UpdateGauges(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            float speedKmh = Mathf.Abs(v.speed * 3.6f);
            float rpm = 0f;
            try
            {
                if (v.data != null)
                {
                    rpm = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.EngineRpm) / 1000f;
                }
            }
            catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            // Sweeping rotation angles (0 to 240 degrees)
            float speedAngle = Mathf.Clamp(speedKmh * 1.0f, 0f, 240f);
            float rpmAngle = Mathf.Clamp((rpm / 8000f) * 240f, 0f, 240f);

            // Rotate Speedometer Needles
            foreach (var t in asset.SpeedometerNeedles)
            {
                if (t == null) continue;
                if (!asset.GaugeRestRotations.TryGetValue(t, out var restRot))
                {
                    restRot = t.localRotation;
                    asset.GaugeRestRotations[t] = restRot;
                }
                t.localRotation = restRot * Quaternion.Euler(0f, 0f, -speedAngle);
            }

            // Rotate Tachometer Needles
            foreach (var t in asset.TachometerNeedles)
            {
                if (t == null) continue;
                if (!asset.GaugeRestRotations.TryGetValue(t, out var restRot))
                {
                    restRot = t.localRotation;
                    asset.GaugeRestRotations[t] = restRot;
                }
                t.localRotation = restRot * Quaternion.Euler(0f, 0f, -rpmAngle);
            }
        }
    }
}

