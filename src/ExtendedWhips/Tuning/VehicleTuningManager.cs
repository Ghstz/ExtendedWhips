using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UnityEngine;

namespace ExtendedWhips.Tuning
{
    public class VehicleTuningManager
    {
        private static VehicleTuningManager? _instance;
        public static VehicleTuningManager Instance => _instance ??= new VehicleTuningManager();

        public TuningProfile CurrentProfile { get; private set; } = new TuningProfile();
        private GameObject? _lastActiveVehicle;
        private readonly Dictionary<WheelCollider, WheelOriginalData> _originalWheelData = new();
        private readonly string _profilesDirectory;

        private struct WheelOriginalData
        {
            public float SuspensionTargetPosition;
            public float SpringSpring;
            public float SpringDamper;
            public WheelFrictionCurve ForwardFriction;
            public WheelFrictionCurve SidewaysFriction;
            public float BrakeTorque;
        }

        public VehicleTuningManager()
        {
            _profilesDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ExtendedWhips", "TuningProfiles"
            );

            if (!Directory.Exists(_profilesDirectory))
            {
                Directory.CreateDirectory(_profilesDirectory);
            }
        }

        public void ApplyTuningToVehicle(GameObject vehicle)
        {
            if (vehicle == null) return;
            _lastActiveVehicle = vehicle;

            var wheels = vehicle.GetComponentsInChildren<WheelCollider>();
            if (wheels == null || wheels.Length == 0)
            {
                return;
            }

            foreach (var wheel in wheels)
            {
                // Record stock parameters before modifying
                if (!_originalWheelData.ContainsKey(wheel))
                {
                    _originalWheelData[wheel] = new WheelOriginalData
                    {
                        SuspensionTargetPosition = wheel.suspensionSpring.targetPosition,
                        SpringSpring = wheel.suspensionSpring.spring,
                        SpringDamper = wheel.suspensionSpring.damper,
                        ForwardFriction = wheel.forwardFriction,
                        SidewaysFriction = wheel.sidewaysFriction,
                        BrakeTorque = wheel.brakeTorque
                    };
                }

                var orig = _originalWheelData[wheel];

                // 1. Suspension & Stance
                var spring = wheel.suspensionSpring;
                spring.targetPosition = Mathf.Clamp(orig.SuspensionTargetPosition + CurrentProfile.RideHeightOffset, 0f, 1f);
                spring.spring = orig.SpringSpring * CurrentProfile.SpringStiffnessMultiplier;
                spring.damper = orig.SpringDamper * CurrentProfile.DamperMultiplier;
                wheel.suspensionSpring = spring;

                // 2. Forward Grip (Acceleration & Braking Traction)
                var forwardFriction = orig.ForwardFriction;
                forwardFriction.stiffness = orig.ForwardFriction.stiffness * CurrentProfile.TireForwardGripMultiplier;
                wheel.forwardFriction = forwardFriction;

                // 3. Sideways Friction (Drift vs Cornering Grip)
                var sidewaysFriction = orig.SidewaysFriction;
                sidewaysFriction.stiffness = orig.SidewaysFriction.stiffness * (1.0f / CurrentProfile.TireDriftSlipMultiplier);
                wheel.sidewaysFriction = sidewaysFriction;
            }

            // Rigidbody weight & center of mass tweaks
            var rb = vehicle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Slightly lower center of mass if lowered for better stability
                if (CurrentProfile.RideHeightOffset < 0f)
                {
                    Vector3 com = rb.centerOfMass;
                    com.y = Mathf.Min(com.y, -0.3f);
                    rb.centerOfMass = com;
                }
            }
        }

        public void SaveProfile(string profileName)
        {
            try
            {
                string filePath = Path.Combine(_profilesDirectory, $"{profileName}.json");
                string json = JsonSerializer.Serialize(CurrentProfile, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExtendedWhips] Failed to save tuning profile: {ex.Message}");
            }
        }

        public void LoadProfile(string profileName)
        {
            try
            {
                string filePath = Path.Combine(_profilesDirectory, $"{profileName}.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var loaded = JsonSerializer.Deserialize<TuningProfile>(json);
                    if (loaded != null)
                    {
                        CurrentProfile = loaded;
                        if (_lastActiveVehicle != null)
                        {
                            ApplyTuningToVehicle(_lastActiveVehicle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExtendedWhips] Failed to load tuning profile: {ex.Message}");
            }
        }
    }
}

