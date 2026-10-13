using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtendedWhips.Models
{
    public class LoadedVehicleAsset
    {
        public GlbVehicleModel Spec { get; set; }
        public GameObject RootObject { get; set; }
        public GameObject? ChassisPivot { get; set; }

        // Wheels (Spinning Rims & Tires)
        public List<Transform> FrontLeftWheels { get; set; } = new();
        public List<Transform> FrontRightWheels { get; set; } = new();
        public List<Transform> RearLeftWheels { get; set; } = new();
        public List<Transform> RearRightWheels { get; set; } = new();

        // Calipers (Fixed to Knuckle - Steer without Spin)
        public List<Transform> FrontLeftCalipers { get; set; } = new();
        public List<Transform> FrontRightCalipers { get; set; } = new();
        public List<Transform> RearLeftCalipers { get; set; } = new();
        public List<Transform> RearRightCalipers { get; set; } = new();

        // Initial Rest Positions & Rotations of Wheels
        public Dictionary<Transform, Vector3> WheelRestLocalPositions { get; set; } = new();
        public Dictionary<Transform, Quaternion> WheelRestLocalRotations { get; set; } = new();

        // Steering Wheel (syncs with in-game driver hands)
        public List<Transform> SteeringWheelTransforms { get; set; } = new();
        public Dictionary<Transform, Quaternion> SteeringWheelRestRotations { get; set; } = new();

        // Active Aerodynamics / Airbrake Spoilers
        public List<Transform> SpoilerTransforms { get; set; } = new();
        public Dictionary<Transform, Quaternion> SpoilerRestRotations { get; set; } = new();

        // Working Cockpit Gauges
        public List<Transform> SpeedometerNeedles { get; set; } = new();
        public List<Transform> TachometerNeedles { get; set; } = new();
        public Dictionary<Transform, Quaternion> GaugeRestRotations { get; set; } = new();

        // Showroom / Inspection Body Panels
        public List<Transform> HoodTransforms { get; set; } = new();
        public Dictionary<Transform, Quaternion> HoodRestRotations { get; set; } = new();

        public List<Transform> LeftDoorTransforms { get; set; } = new();
        public Dictionary<Transform, Quaternion> LeftDoorRestRotations { get; set; } = new();

        public List<Transform> RightDoorTransforms { get; set; } = new();
        public Dictionary<Transform, Quaternion> RightDoorRestRotations { get; set; } = new();

        public List<Transform> TrunkTransforms { get; set; } = new();
        public Dictionary<Transform, Quaternion> TrunkRestRotations { get; set; } = new();

        // Categorized Materials (for real-time Paint Studio)
        public List<Material> BodyMaterials { get; set; } = new();
        public List<Material> WheelMaterials { get; set; } = new();
        public List<Material> GlassMaterials { get; set; } = new();
        public List<Material> HeadlightMaterials { get; set; } = new();
        public List<Material> TaillightMaterials { get; set; } = new();
        public List<Material> ReverseMaterials { get; set; } = new();
        public List<Material> IndicatorMaterials { get; set; } = new();

        // Lighting Components
        public List<Light> HeadlightSources { get; set; } = new();
        public List<Light> TaillightSources { get; set; } = new();
        public List<Light> ReverseSources { get; set; } = new();
        public List<Light> IndicatorLeftSources { get; set; } = new();
        public List<Light> IndicatorRightSources { get; set; } = new();

        // Exhaust FX
        public List<ParticleSystem> ExhaustSmokeSystems { get; set; } = new();
        public List<ParticleSystem> ExhaustFlameSystems { get; set; } = new();
        public List<Light> ExhaustFlashLights { get; set; } = new();
        public AudioSource? ExhaustAudioSource { get; set; }

        // Air Ride Stance Height
        public float AirRideHeightOffset { get; set; } = 0.0f;

        // Livery & Decal Presets
        public string ActiveLiveryName { get; set; } = "None";

        public List<GameObject> WheelObjects { get; set; } = new();
        public List<GameObject> BodyObjects { get; set; } = new();

        public LoadedVehicleAsset(GlbVehicleModel spec, GameObject root)
        {
            Spec = spec;
            RootObject = root;
        }
    }
}
