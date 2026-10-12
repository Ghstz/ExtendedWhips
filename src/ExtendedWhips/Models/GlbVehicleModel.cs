using System;

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
        public float WheelRadiusScale { get; set; } = 1.0f;
        public float TrackWidthScale { get; set; } = 1.0f;
        public string RecommendedPreset { get; set; } = "Street";

        public GlbVehicleModel(string id, string name, string file, VehicleTypeCategory cat, string subtitle, float scale = 1.0f, float heightOffset = 0.0f, float wheelScale = 1.0f, string preset = "Street", float trackScale = 1.0f)
        {
            Id = id;
            DisplayName = name;
            FileName = file;
            Category = cat;
            Subtitle = subtitle;
            ModelScale = scale;
            HeightOffset = heightOffset;
            WheelRadiusScale = wheelScale;
            RecommendedPreset = preset;
            TrackWidthScale = trackScale;
        }
    }
}
