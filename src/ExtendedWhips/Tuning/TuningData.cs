using System;

namespace ExtendedWhips.Tuning
{
    public enum TuningPreset
    {
        Stock,
        StreetPerformance,
        OffroadLifted,
        DriftSpec,
        HyperInsane,
        Custom
    }

    public class StockVehicleSpecs
    {
        public float PeakTorque;
        public float IdleTorque;
        public float MaxRpm;
        public float RpmLimiter;
        public float MaxSteerAngle;
        public float MaxBrakeTorque;
        public float HandbrakeTorque;
    }

    [Serializable]
    public class TuningProfile
    {
        public string VehicleName { get; set; } = "DefaultVehicle";
        public TuningPreset ActivePreset { get; set; } = TuningPreset.Stock;

        // Engine & Transmission
        public float TorqueMultiplier { get; set; } = 1.0f;
        public float IdleTorqueMultiplier { get; set; } = 1.0f;
        public float MaxRpmMultiplier { get; set; } = 1.0f;
        public float TopSpeedMultiplier { get; set; } = 1.0f;
        public bool EnableAllWheelDrive { get; set; } = true;

        // Steering & Brakes
        public float CustomSteerAngle { get; set; } = 0.0f; // 0 = use multiplier or stock
        public float SteerAngleMultiplier { get; set; } = 1.0f;
        public float BrakeTorqueMultiplier { get; set; } = 1.0f;
        public float HandbrakeTorqueMultiplier { get; set; } = 1.0f;
        public float SteeringSpeedMultiplier { get; set; } = 1.0f;

        // Suspension & Stance
        public float RideHeightOffset { get; set; } = 0.0f;
        public float SpringStiffnessMultiplier { get; set; } = 1.0f;
        public float DamperMultiplier { get; set; } = 1.0f;

        // Tires & Handling
        public float TireForwardGripMultiplier { get; set; } = 1.0f;
        public float TireDriftSlipMultiplier { get; set; } = 1.0f;

        public void ResetToStock()
        {
            ActivePreset = TuningPreset.Stock;
            TorqueMultiplier = 1.0f;
            IdleTorqueMultiplier = 1.0f;
            MaxRpmMultiplier = 1.0f;
            TopSpeedMultiplier = 1.0f;
            EnableAllWheelDrive = true;
            CustomSteerAngle = 0.0f;
            SteerAngleMultiplier = 1.0f;
            BrakeTorqueMultiplier = 1.0f;
            HandbrakeTorqueMultiplier = 1.0f;
            SteeringSpeedMultiplier = 1.0f;
            RideHeightOffset = 0.0f;
            SpringStiffnessMultiplier = 1.0f;
            DamperMultiplier = 1.0f;
            TireForwardGripMultiplier = 1.0f;
            TireDriftSlipMultiplier = 1.0f;
        }
    }
}
