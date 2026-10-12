using System;
using UnityEngine;
using ExtendedWhips.Hooks;

namespace ExtendedWhips.Tuning
{
    public static class DriftAssistSystem
    {
        public static bool IsAngleKitEnabled { get; set; } = false;
        public static bool IsGyroAssistEnabled { get; set; } = true;
        public static float GyroAssistanceStrength { get; set; } = 0.65f; // 65% counter-steer torque stabilization

        public static void UpdateDriftAssist(VPVehicleController v)
        {
            if (v == null || v.cachedRigidbody == null) return;

            // 1. Formula Drift 70° Wisefab Angle Kit
            if (IsAngleKitEnabled)
            {
                if (v.steering != null && v.steering.maxSteerAngle < 65f)
                {
                    v.steering.maxSteerAngle = 68.0f;
                }
            }

            // 2. Gyro Dynamic Counter-Steer Yaw Stabilization
            if (!IsGyroAssistEnabled) return;

            Vector3 velocity = v.cachedRigidbody.velocity;
            float speedKmh = velocity.magnitude * 3.6f;
            if (speedKmh < 12.0f) return;

            Vector3 localVel = v.transform.InverseTransformDirection(velocity);
            // Calculate lateral slip angle (drift angle in degrees)
            float driftAngle = Mathf.Atan2(localVel.x, localVel.z) * Mathf.Rad2Deg;

            if (Mathf.Abs(driftAngle) > 8.0f)
            {
                // Counter-steer stabilizing torque
                float sign = Mathf.Sign(driftAngle);
                float magnitude = Mathf.Clamp01((Mathf.Abs(driftAngle) - 8.0f) / 45.0f);
                float torqueAmount = -sign * magnitude * GyroAssistanceStrength * 1800f;

                v.cachedRigidbody.AddTorque(Vector3.up * torqueAmount, ForceMode.Force);
            }
        }
    }
}

