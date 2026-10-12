using System;
using UnityEngine;

namespace ExtendedWhips.Cockpit
{
    public static class ControllerHaptics
    {
        public static bool IsHapticsEnabled { get; set; } = true;
        public static float HapticStrength { get; set; } = 0.75f;

        public static float CurrentLowFrequencyRumble { get; private set; } = 0f;
        public static float CurrentHighFrequencyRumble { get; private set; } = 0f;

        public static void UpdateHaptics(VPVehicleController v)
        {
            if (!IsHapticsEnabled || v == null)
            {
                CurrentLowFrequencyRumble = 0f;
                CurrentHighFrequencyRumble = 0f;
                return;
            }

            float lowRumble = 0f;
            float highRumble = 0f;

            // 1. Engine Redline Vibration
            if (v.data != null && v.engine != null)
            {
                float rpm = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.EngineRpm) / 1000f;
                float maxRpm = v.engine.maxRpm;
                if (rpm > maxRpm * 0.90f)
                {
                    float ratio = (rpm - maxRpm * 0.90f) / (maxRpm * 0.10f);
                    lowRumble += Mathf.Clamp01(ratio) * 0.45f;
                }
            }

            // 2. Tire Slip Vibration (Drifting / Wheelspin)
            if (v.cachedRigidbody != null && v.cachedRigidbody.velocity.magnitude > 2.0f)
            {
                float driftAngle = Vector3.Angle(v.transform.forward, v.cachedRigidbody.velocity);
                if (driftAngle > 9.0f)
                {
                    highRumble += Mathf.Clamp01(driftAngle / 45.0f) * 0.45f;
                }
            }

            CurrentLowFrequencyRumble = Mathf.Clamp01(lowRumble * HapticStrength);
            CurrentHighFrequencyRumble = Mathf.Clamp01(highRumble * HapticStrength);
        }
    }
}

