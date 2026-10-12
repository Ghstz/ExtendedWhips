using System;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class LaunchControlSystem
    {
        public static bool IsLaunchControlActive { get; private set; } = false;
        public static bool IsAntiLagSpitting { get; private set; } = false;

        private static float _antiLagTimer = 0f;
        private static float _launchBoostTimer = 0f;

        public const float LaunchTargetRpm = 4500f;

        public static void UpdateLaunchControl(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            float speedKmh = Mathf.Abs(v.speed * 3.6f);
            float throttle = 0f;
            float brake = 0f;
            float handbrake = 0f;

            try
            {
                if (v.data != null)
                {
                    throttle = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Throttle) / 10000f;
                    brake = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Brake) / 10000f;
                    handbrake = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Handbrake) / 10000f;
                }
            }
            catch { }

            // Handbrake or Spacebar + Full Throttle at low speed
            bool handbrakeHeld = handbrake > 0.4f || Input.GetKey(KeyCode.Space);
            bool throttlePinned = throttle > 0.65f || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

            if (speedKmh < 4.0f && handbrakeHeld && throttlePinned)
            {
                IsLaunchControlActive = true;
                IsAntiLagSpitting = true;

                // Pulsate / cut engine fuel & spark to hold launch target RPM
                if (v.engine != null && v.data != null)
                {
                    float currentRpm = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.EngineRpm) / 1000f;
                    if (currentRpm > LaunchTargetRpm)
                    {
                        // Cut throttle back
                        v.data.Set(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Throttle, 1500); // 15% throttle hold
                    }
                }

                // Rapid-fire anti-lag backfire pops & flames
                _antiLagTimer += Time.deltaTime;
                if (_antiLagTimer >= 0.08f)
                {
                    _antiLagTimer = 0f;
                    CustomVehicleExhaust.TriggerAfterfire(asset);
                }
            }
            else
            {
                if (IsLaunchControlActive && !handbrakeHeld && throttlePinned && speedKmh < 30f)
                {
                    // Slingshot launch active!
                    _launchBoostTimer = 1.2f;
                }

                IsLaunchControlActive = false;
                IsAntiLagSpitting = false;
            }

            if (_launchBoostTimer > 0f)
            {
                _launchBoostTimer -= Time.deltaTime;
                // Add instantaneous slingshot launch boost
                if (v.cachedRigidbody != null)
                {
                    v.cachedRigidbody.AddForce(v.transform.forward * 4500f, ForceMode.Force);
                }
            }
        }
    }
}

