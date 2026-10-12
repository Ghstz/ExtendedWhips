using System;
using System.Collections.Generic;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class CustomVehicleNitrous
    {
        public static float BottlePsi { get; private set; } = 1000f; // Full 1000 PSI bottle
        public const float MaxPsi = 1000f;
        public static bool IsPurging { get; private set; } = false;
        public static bool IsBoosting { get; private set; } = false;

        private static List<ParticleSystem> _purgeSystems = new();
        private static AudioSource? _audioSource;
        private static AudioClip? _purgeClip;
        private static float _stockFov = 60f;
        private static bool _hasCachedFov = false;

        public static void InitializeNitrous(LoadedVehicleAsset asset)
        {
            if (asset == null || asset.RootObject == null) return;

            foreach (var ps in _purgeSystems) if (ps != null) UnityEngine.Object.Destroy(ps.gameObject);
            _purgeSystems.Clear();

            if (_audioSource != null) UnityEngine.Object.Destroy(_audioSource.gameObject);

            // Create Dual Fender Purge Emitters (Front Left & Right cowl)
            float cowlZ = asset.Spec.Category == VehicleTypeCategory.Motorcycle ? 0.35f : 1.35f;
            float cowlX = asset.Spec.Category == VehicleTypeCategory.Motorcycle ? 0.15f : 0.65f;
            float cowlY = 0.85f;

            CreatePurgeJet(asset.RootObject.transform, new Vector3(-cowlX, cowlY, cowlZ), Quaternion.Euler(-35f, -50f, 0f));
            CreatePurgeJet(asset.RootObject.transform, new Vector3(+cowlX, cowlY, cowlZ), Quaternion.Euler(-35f, +50f, 0f));

            var audioObj = new GameObject("NitrousAudioSource");
            audioObj.transform.SetParent(asset.RootObject.transform, false);
            audioObj.transform.localPosition = new Vector3(0f, cowlY, cowlZ);

            _audioSource = audioObj.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1.0f;
            _audioSource.minDistance = 2.0f;
            _audioSource.maxDistance = 50.0f;

            if (_purgeClip == null) _purgeClip = GeneratePurgeHissClip();
        }

        private static void CreatePurgeJet(Transform parent, Vector3 localPos, Quaternion localRot)
        {
            var jetObj = new GameObject("NitrousPurgeJet");
            jetObj.transform.SetParent(parent, false);
            jetObj.transform.localPosition = localPos;
            jetObj.transform.localRotation = localRot;

            var ps = jetObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.22f;
            main.startSpeed = 12.0f;
            main.startSize = 0.18f;
            main.startColor = new Color(0.95f, 0.98f, 1.0f, 0.85f); // Crisp white CO2 vapor
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 60;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            _purgeSystems.Add(ps);
        }

        public static void UpdateNitrous(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            // 1. Fender CO2 Purge Key [N]
            bool purgeKey = Input.GetKey(KeyCode.N);
            if (purgeKey && BottlePsi > 5f)
            {
                IsPurging = true;
                BottlePsi = Mathf.Max(0f, BottlePsi - 90f * Time.deltaTime);

                foreach (var ps in _purgeSystems)
                {
                    if (ps != null) ps.Emit(3);
                }

                if (_audioSource != null && _purgeClip != null && !_audioSource.isPlaying)
                {
                    _audioSource.pitch = UnityEngine.Random.Range(1.1f, 1.35f);
                    _audioSource.PlayOneShot(_purgeClip, 0.75f);
                }
            }
            else
            {
                IsPurging = false;
            }

            // 2. Nitrous Boost [LeftShift] (when moving forward)
            bool boostKey = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Keypad0)) && v.speed > 1.5f;
            if (boostKey && BottlePsi > 10f)
            {
                IsBoosting = true;
                BottlePsi = Mathf.Max(0f, BottlePsi - 75f * Time.deltaTime);

                // Boost engine torque by +50%
                if (v.engine != null)
                {
                    v.engine.peakRpmTorque *= 1.01f; // High dynamic torque surge
                }

                // Exhaust Nitro flame bursts
                CustomVehicleExhaust.TriggerAfterfire(asset);

                // Camera FOV Warp Punch
                ApplyCameraFovWarp(true);
            }
            else
            {
                IsBoosting = false;
                ApplyCameraFovWarp(false);
            }

            // Passive slow tank refill when idle/parked
            if (!IsPurging && !IsBoosting && BottlePsi < MaxPsi)
            {
                BottlePsi = Mathf.Min(MaxPsi, BottlePsi + 8f * Time.deltaTime);
            }
        }

        private static void ApplyCameraFovWarp(bool active)
        {
            try
            {
                var cam = UnityEngine.Camera.main;
                if (cam != null)
                {
                    if (!_hasCachedFov)
                    {
                        _stockFov = cam.fieldOfView;
                        _hasCachedFov = true;
                    }

                    float targetFov = active ? (_stockFov + 12f) : _stockFov;
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * 6f);
                }
            }
            catch { }
        }

        public static void RefillBottle()
        {
            BottlePsi = MaxPsi;
        }

        private static AudioClip GeneratePurgeHissClip()
        {
            int sampleRate = 44100;
            int count = (int)(sampleRate * 0.18f);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 15f);
                samples[i] = (UnityEngine.Random.value * 2f - 1f) * env * 0.7f;
            }

            var clip = AudioClip.Create("NitrousPurgeHiss", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

