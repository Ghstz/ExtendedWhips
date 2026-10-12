using System;
using UnityEngine;

namespace ExtendedWhips.Tuning
{
    public enum DragBenchmarkState
    {
        Idle,
        Armed,
        Running,
        Completed
    }

    public static class DragTelemetryBenchmark
    {
        public static DragBenchmarkState State { get; private set; } = DragBenchmarkState.Idle;

        // Current Run Telemetry
        public static float CurrentRunTime { get; private set; } = 0f;
        public static float CurrentDistanceMeters { get; private set; } = 0f;

        public static float Last0to60Mph { get; private set; } = 0f;
        public static float Last0to100Mph { get; private set; } = 0f;
        public static float LastQuarterMileTime { get; private set; } = 0f;
        public static float LastQuarterMileTrapSpeedKmh { get; private set; } = 0f;
        public static float Last60to0BrakingDistance { get; private set; } = 0f;
        public static float PeakLateralG { get; private set; } = 0f;
        public static float PeakLongitudinalG { get; private set; } = 0f;

        // Personal Bests
        public static float Best0to60Mph { get; private set; } = 99.9f;
        public static float Best0to100Mph { get; private set; } = 99.9f;
        public static float BestQuarterMileTime { get; private set; } = 99.9f;
        public static float BestQuarterMileTrapSpeedKmh { get; private set; } = 0f;

        private static float _runStartTime = 0f;
        private static float _prevLocalVelZ = 0f;
        private static Vector3 _runStartPosition = Vector3.zero;
        private static bool _hit60 = false;
        private static bool _hit100 = false;
        private static bool _hitQuarterMile = false;

        private static bool _wasOver60ForBraking = false;
        private static Vector3 _brakingStartPosition = Vector3.zero;
        private static bool _isBrakingTest = false;

        public static void UpdateBenchmark(VPVehicleController? v)
        {
            if (v == null) return;

            float speedKmh = Mathf.Abs(v.speed * 3.6f);
            float speedMph = Mathf.Abs(v.speed * 2.23694f);

            // Calculate G-Forces
            var rb = v.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float dt = Time.deltaTime;
                float localVelZ = v.transform.InverseTransformDirection(rb.velocity).z;
                float latG = Mathf.Abs(v.speed * (rb.angularVelocity.y)) / 9.81f;
                float longG = dt > 0.0001f ? ((localVelZ - _prevLocalVelZ) / dt) / 9.81f : 0f;
                _prevLocalVelZ = localVelZ;

                if (latG > PeakLateralG) PeakLateralG = latG;
                if (longG > PeakLongitudinalG) PeakLongitudinalG = longG;
            }

            // State Machine
            switch (State)
            {
                case DragBenchmarkState.Idle:
                case DragBenchmarkState.Completed:
                    if (speedKmh < 1.0f)
                    {
                        State = DragBenchmarkState.Armed;
                    }
                    break;

                case DragBenchmarkState.Armed:
                    if (speedKmh > 2.0f)
                    {
                        // Launch detected!
                        State = DragBenchmarkState.Running;
                        _runStartTime = Time.time;
                        _runStartPosition = v.transform.position;
                        CurrentRunTime = 0f;
                        CurrentDistanceMeters = 0f;
                        _hit60 = false;
                        _hit100 = false;
                        _hitQuarterMile = false;
                    }
                    break;

                case DragBenchmarkState.Running:
                    CurrentRunTime = Time.time - _runStartTime;
                    CurrentDistanceMeters = Vector3.Distance(v.transform.position, _runStartPosition);

                    // 0-60 mph (96.56 km/h)
                    if (!_hit60 && speedMph >= 60f)
                    {
                        Last0to60Mph = CurrentRunTime;
                        if (Last0to60Mph < Best0to60Mph) Best0to60Mph = Last0to60Mph;
                        _hit60 = true;
                    }

                    // 0-100 mph (160.9 km/h)
                    if (!_hit100 && speedMph >= 100f)
                    {
                        Last0to100Mph = CurrentRunTime;
                        if (Last0to100Mph < Best0to100Mph) Best0to100Mph = Last0to100Mph;
                        _hit100 = true;
                    }

                    // 1/4 Mile (402.336 meters)
                    if (!_hitQuarterMile && CurrentDistanceMeters >= 402.34f)
                    {
                        LastQuarterMileTime = CurrentRunTime;
                        LastQuarterMileTrapSpeedKmh = speedKmh;
                        if (LastQuarterMileTime < BestQuarterMileTime)
                        {
                            BestQuarterMileTime = LastQuarterMileTime;
                            BestQuarterMileTrapSpeedKmh = speedKmh;
                        }
                        _hitQuarterMile = true;
                        State = DragBenchmarkState.Completed;
                    }

                    // If stopped during run, finish
                    if (CurrentRunTime > 2.0f && speedKmh < 2.0f)
                    {
                        State = DragBenchmarkState.Completed;
                    }
                    break;
            }

            // 60-0 mph Braking Test
            if (speedMph >= 60f)
            {
                _wasOver60ForBraking = true;
            }
            if (_wasOver60ForBraking && !_isBrakingTest && speedMph < 59f)
            {
                _isBrakingTest = true;
                _brakingStartPosition = v.transform.position;
            }
            if (_isBrakingTest && speedMph < 2.0f)
            {
                Last60to0BrakingDistance = Vector3.Distance(v.transform.position, _brakingStartPosition);
                _isBrakingTest = false;
                _wasOver60ForBraking = false;
            }
        }

        public static void ResetBests()
        {
            Best0to60Mph = 99.9f;
            Best0to100Mph = 99.9f;
            BestQuarterMileTime = 99.9f;
            BestQuarterMileTrapSpeedKmh = 0f;
            PeakLateralG = 0f;
            PeakLongitudinalG = 0f;
            State = DragBenchmarkState.Idle;
        }
    }
}

