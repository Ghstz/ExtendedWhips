using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtendedWhips.Tuning
{
    public enum TuningPreset
    {
        Stock,
        StreetPerformance,
        OffroadLifted,
        DriftSpec,
        HyperInsane,
        RallyCross,
        Custom
    }

    public class StockWheelSpecs
    {
        public float SuspensionDistance = 0.25f;
        public float SuspensionAnchor = 0.0f;
        public float SpringRate = 45000f;
        public float DamperRate = 4500f;
        public float Radius = 0.45f;
        public float Mass = 30f;
        public float ForwardGrip = 1.0f;
        public float SidewaysGrip = 1.0f;
    }

    public class StockVehicleSpecs
    {
        // 1. Engine & Powertrain
        public float PeakTorque;
        public float IdleTorque;
        public float MaxRpm;
        public float RpmLimiter;
        public float IdleRpm;
        public float Inertia;
        public float FrictionTorque;

        // 2. Gearing & Differential
        public float FinalDriveRatio = 3.5f;
        public float ManualShiftTime = 0.25f;
        public float AutoTransitionTime = 0.35f;
        public float ClutchMaxTorque = 6000f;
        public float DiffPowerRatio = 0.5f;
        public float DiffCoastRatio = 0.3f;
        public float DiffPreload = 50f;

        // 3. Steering & Alignment
        public float MaxSteerAngle = 35f;
        public float ToeAngle = 0.0f;
        public bool Ackerman = true;

        // 4. Brakes
        public float MaxBrakeTorque = 5000f;
        public float BrakeBias = 0.6f;
        public float HandbrakeTorque = 8000f;

        // 5. Driver Assists
        public bool AbsEnabled = true;
        public bool TcsEnabled = true;
        public float TcsRatio = 1.0f;
        public bool EspEnabled = true;

        // 6. Chassis & Body
        public float ChassisMass = 3500f;
        public Vector3 CenterOfMass = Vector3.zero;

        // 7. Wheels & Suspension
        public List<StockWheelSpecs> Wheels = new();

        // Runtime component caches (avoid per-frame GetComponent* allocations on IL2CPP)
        public Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<VPWheelCollider>? VpWheels;
        public Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<WheelCollider>? UnityWheels;
        public Rigidbody? Rb;
    }

    [Serializable]
    public class TuningProfile
    {
        public string VehicleName { get; set; } = "DefaultVehicle";
        public TuningPreset ActivePreset { get; set; } = TuningPreset.Stock;

        // 1. Engine & Powertrain
        public float TorqueMultiplier { get; set; } = 1.0f;          // 0.2x to 8.0x
        public float IdleTorqueMultiplier { get; set; } = 1.0f;      // 0.2x to 5.0x
        public float RpmMultiplier { get; set; } = 1.0f;             // 1.0x to 2.5x
        public float IdleRpmMultiplier { get; set; } = 1.0f;         // 0.5x to 2.5x
        public float FlywheelInertiaMultiplier { get; set; } = 1.0f; // 0.2x (snappy) to 3.0x (heavy diesel)
        public float CompressionBrakingMultiplier { get; set; } = 1.0f; // 0.1x to 4.0x

        // 2. Gearing & Transmission
        public float FinalDriveMultiplier { get; set; } = 1.0f;      // 0.5x (top speed) to 2.5x (acceleration)
        public float ShiftSpeedMultiplier { get; set; } = 1.0f;      // 0.1x (racing dogbox) to 2.0x (slow manual)
        public float ClutchTorqueMultiplier { get; set; } = 1.0f;    // 0.5x to 5.0x
        public float DiffPowerLock { get; set; } = 0.5f;             // LSD Accel Lock: 0.0 (open) to 1.0 (spool)
        public float DiffCoastLock { get; set; } = 0.3f;             // LSD Decel Lock: 0.0 (open) to 1.0 (locked)
        public float DiffPreload { get; set; } = 50f;                // Differential Preload Nm

        // 3. Suspension & Stance
        public float RideHeightOffset { get; set; } = 0.0f;          // -0.20m (slammed) to +0.35m (monster lifted)
        public float SpringStiffnessMultiplier { get; set; } = 1.0f; // 0.2x (soft) to 4.0x (rock solid)
        public float DamperMultiplier { get; set; } = 1.0f;          // 0.2x to 3.5x
        public float TireSizeMultiplier { get; set; } = 1.0f;        // 0.75x (low profile) to 1.5x (giant mud tires)
        public float WheelMassMultiplier { get; set; } = 1.0f;       // 0.5x to 2.5x

        // 4. Steering & Alignment
        public float CustomSteerAngle { get; set; } = 0.0f;          // 0 = stock (20° to 75° drift lock)
        public float SteerToeAngle { get; set; } = 0.0f;             // -5° (toe-out) to +5° (toe-in)
        public bool AckermanSteering { get; set; } = true;

        // 5. Tires & Grip
        public float TireForwardGripMultiplier { get; set; } = 1.0f; // 0.5x (burnout) to 3.0x (racing slicks)
        public float TireDriftSlipMultiplier { get; set; } = 1.0f;   // 0.3x (easy drift) to 2.5x (slot car grip)

        // 6. Brakes & Assists
        public float BrakeTorqueMultiplier { get; set; } = 1.0f;     // 0.5x to 4.0x
        public float BrakeBias { get; set; } = 0.60f;                // 0.20 (rear heavy) to 0.85 (front heavy)
        public float HandbrakeTorqueMultiplier { get; set; } = 1.0f; // 1.0x to 6.0x
        public bool AbsEnabled { get; set; } = true;
        public bool TcsEnabled { get; set; } = true;
        public float TcsSlipRatio { get; set; } = 1.0f;
        public bool EspEnabled { get; set; } = true;

        // 7. Chassis & Aerodynamics
        public float ChassisMassMultiplier { get; set; } = 1.0f;     // 0.5x (lightweight track) to 2.5x (heavy hauler)
        public float CenterOfMassYOffset { get; set; } = 0.0f;       // -0.6m (slam low CoG) to +0.4m
        public float DownforceMultiplier { get; set; } = 0.0f;       // 0.0 to 3.0x aerodynamic downforce

        public void ResetToStock(StockVehicleSpecs? stock = null)
        {
            ActivePreset = TuningPreset.Stock;

            // Engine
            TorqueMultiplier = 1.0f;
            IdleTorqueMultiplier = 1.0f;
            RpmMultiplier = 1.0f;
            IdleRpmMultiplier = 1.0f;
            FlywheelInertiaMultiplier = 1.0f;
            CompressionBrakingMultiplier = 1.0f;

            // Gearing
            FinalDriveMultiplier = 1.0f;
            ShiftSpeedMultiplier = 1.0f;
            ClutchTorqueMultiplier = 1.0f;
            DiffPowerLock = stock != null ? stock.DiffPowerRatio : 0.5f;
            DiffCoastLock = stock != null ? stock.DiffCoastRatio : 0.3f;
            DiffPreload = stock != null ? stock.DiffPreload : 50f;

            // Suspension
            RideHeightOffset = 0.0f;
            SpringStiffnessMultiplier = 1.0f;
            DamperMultiplier = 1.0f;
            TireSizeMultiplier = 1.0f;
            WheelMassMultiplier = 1.0f;

            // Steering & Alignment
            CustomSteerAngle = stock != null ? stock.MaxSteerAngle : 35f;
            SteerToeAngle = stock != null ? stock.ToeAngle : 0.0f;
            AckermanSteering = stock == null || stock.Ackerman;

            // Tires
            TireForwardGripMultiplier = 1.0f;
            TireDriftSlipMultiplier = 1.0f;

            // Brakes
            BrakeTorqueMultiplier = 1.0f;
            BrakeBias = stock != null ? stock.BrakeBias : 0.60f;
            HandbrakeTorqueMultiplier = 1.0f;
            AbsEnabled = stock == null || stock.AbsEnabled;
            TcsEnabled = stock == null || stock.TcsEnabled;
            TcsSlipRatio = stock != null ? stock.TcsRatio : 1.0f;
            EspEnabled = stock == null || stock.EspEnabled;

            // Chassis
            ChassisMassMultiplier = 1.0f;
            CenterOfMassYOffset = 0.0f;
            DownforceMultiplier = 0.0f;
        }
    }
}
