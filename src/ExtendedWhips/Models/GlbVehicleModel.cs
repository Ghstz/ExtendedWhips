using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtendedWhips.Models
{
    public enum VehicleTypeCategory
    {
        MuscleCar,
        SportsCar,
        Supercar,
        Truck,
        Offroad4x4,
        Motorcycle
    }

    public class GlbVehicleModel
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string FileName { get; set; } = "";
        public VehicleTypeCategory Category { get; set; }
        public string Subtitle { get; set; } = "";
        public float ModelScale { get; set; } = 1.0f;
        public float HeightOffset { get; set; } = 0.0f;
        public float CenterOffsetX { get; set; } = 0.0f;
        public float CenterOffsetZ { get; set; } = 0.0f;
        public float DriverYOffset { get; set; } = 0.0f;
        public float RotationY { get; set; } = 0.0f;
        public float WheelRadiusScale { get; set; } = 1.0f;
        public float TrackWidthScale { get; set; } = 1.0f;
        public string RecommendedPreset { get; set; } = "Street";
        
        // Cockpit Camera Eye-line (relative to vehicle body)
        public Vector3 CockpitCameraOffset { get; set; } = Vector3.zero;

        // Exhaust tip coordinates (relative to vehicle root)
        public List<Vector3> ExhaustOffsets { get; set; } = new();

        public GlbVehicleModel(string id, string name, string file, VehicleTypeCategory cat, string subtitle, 
            float scale = 1.0f, float heightOffset = 0.0f, float rotY = 0.0f, float wheelScale = 1.0f, 
            string preset = "Street", float trackScale = 1.0f, float offsetX = 0.0f, float offsetZ = 0.0f, 
            float driverY = 0.0f, Vector3? cockpitCam = null, List<Vector3>? exhausts = null)
        {
            Id = id;
            DisplayName = name;
            FileName = file;
            Category = cat;
            Subtitle = subtitle;
            ModelScale = scale;
            HeightOffset = heightOffset;
            RotationY = rotY;
            WheelRadiusScale = wheelScale;
            RecommendedPreset = preset;
            TrackWidthScale = trackScale;
            CenterOffsetX = offsetX;
            CenterOffsetZ = offsetZ;
            DriverYOffset = driverY;
            if (cockpitCam.HasValue) CockpitCameraOffset = cockpitCam.Value;
            if (exhausts != null) ExhaustOffsets = exhausts;
        }
    }
}
