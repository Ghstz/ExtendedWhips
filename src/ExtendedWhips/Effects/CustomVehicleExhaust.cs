using System;
using System.Collections.Generic;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class CustomVehicleExhaust
    {
        private static float _lastPopTime = 0f;
        private static float _prevRpm = 0f;
        private static float _prevThrottle = 0f;
        private static int _prevGear = 0;
        private static AudioClip? _cachedPopClip = null;

        public static void InitializeExhaust(LoadedVehicleAsset asset)
        {
            if (asset == null || asset.RootObject == null) return;

            var spec = asset.Spec;

            foreach (var ps in asset.ExhaustSmokeSystems) if (ps != null) UnityEngine.Object.Destroy(ps.gameObject);
            foreach (var ps in asset.ExhaustFlameSystems) if (ps != null) UnityEngine.Object.Destroy(ps.gameObject);
            foreach (var l in asset.ExhaustFlashLights) if (l != null) UnityEngine.Object.Destroy(l.gameObject);
            if (asset.ExhaustAudioSource != null) UnityEngine.Object.Destroy(asset.ExhaustAudioSource.gameObject);

            asset.ExhaustSmokeSystems.Clear();
            asset.ExhaustFlameSystems.Clear();
            asset.ExhaustFlashLights.Clear();

            var offsets = spec.ExhaustOffsets;
            if (offsets == null || offsets.Count == 0)
            {
                bool isBike = spec.Category == VehicleTypeCategory.Motorcycle;
                offsets = isBike
                    ? new List<Vector3> { new Vector3(0.35f, 0.32f, -0.80f) }
                    : new List<Vector3> { new Vector3(-0.50f, 0.28f, -2.40f), new Vector3(0.50f, 0.28f, -2.40f) };
            }

            foreach (var offset in offsets)
            {
                var tipObj = new GameObject("ExhaustTip");
                tipObj.transform.SetParent(asset.RootObject.transform, false);
                tipObj.transform.localPosition = offset;
                tipObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                // 1. Smoke Particle System
                var smokePs = tipObj.AddComponent<ParticleSystem>();
                var main = smokePs.main;
                main.startLifetime = 1.0f;
                main.startSpeed = 3.5f;
                main.startSize = 0.15f;
                main.startColor = new Color(0.85f, 0.85f, 0.88f, 0.25f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 50;

                var emission = smokePs.emission;
                emission.rateOverTime = 8f;

                asset.ExhaustSmokeSystems.Add(smokePs);

                // 2. Flame Particle System
                var flameObj = new GameObject("ExhaustFlameBurst");
                flameObj.transform.SetParent(tipObj.transform, false);
                flameObj.transform.localPosition = Vector3.zero;
                flameObj.transform.localRotation = Quaternion.identity;

                var flamePs = flameObj.AddComponent<ParticleSystem>();
                var fMain = flamePs.main;
                fMain.startLifetime = 0.08f;
                fMain.startSpeed = 8.0f;
                fMain.startSize = 0.22f;
                fMain.startColor = new Color(1.0f, 0.45f, 0.05f, 0.95f);
                fMain.simulationSpace = ParticleSystemSimulationSpace.Local;
                fMain.maxParticles = 20;

                var fEmission = flamePs.emission;
                fEmission.rateOverTime = 0f;

                asset.ExhaustFlameSystems.Add(flamePs);

                // 3. Flame Flash Light
                var flashLightObj = new GameObject("ExhaustFlashLight");
                flashLightObj.transform.SetParent(tipObj.transform, false);
                flashLightObj.transform.localPosition = new Vector3(0f, 0f, 0.1f);

                var flashLight = flashLightObj.AddComponent<Light>();
                flashLight.type = LightType.Point;
                flashLight.color = new Color(1.0f, 0.50f, 0.10f);
                flashLight.range = 3.5f;
                flashLight.intensity = 800f;
                flashLight.enabled = false;
                asset.ExhaustFlashLights.Add(flashLight);
            }

            // Audio Source
            var audioObj = new GameObject("ExhaustAudio");
            audioObj.transform.SetParent(asset.RootObject.transform, false);
            audioObj.transform.localPosition = offsets[0];

            var audioSrc = audioObj.AddComponent<AudioSource>();
            audioSrc.spatialBlend = 1.0f;
            audioSrc.minDistance = 3.0f;
            audioSrc.maxDistance = 60.0f;
            audioSrc.volume = 0.85f;
            asset.ExhaustAudioSource = audioSrc;

            if (_cachedPopClip == null)
            {
                _cachedPopClip = GenerateSyntheticBackfireClip();
            }
        }

        public static void UpdateExhaust(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            float rpm = 0f;
            int gear = 0;
            float throttle = 0f;

            try
            {
                if (v.data != null)
                {
                    rpm = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.EngineRpm) / 1000f;
                    gear = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.GearboxGear);
                    throttle = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Throttle) / 10000f;
                }
            }
            catch { }

            float targetSmokeRate = Mathf.Lerp(4f, 40f, Mathf.Clamp01(rpm / 6500f));
            foreach (var ps in asset.ExhaustSmokeSystems)
            {
                if (ps == null) continue;
                var em = ps.emission;
                em.rateOverTime = targetSmokeRate;
            }

            bool liftOff = (_prevThrottle > 0.35f && throttle < 0.05f && _prevRpm > 3600f);
            bool downshiftPop = (gear < _prevGear && gear > 0 && rpm > 3200f);
            bool highRpmOverrun = (throttle < 0.05f && rpm > 4500f && UnityEngine.Random.value < 0.08f);

            if ((liftOff || downshiftPop || highRpmOverrun) && (Time.time - _lastPopTime > 0.18f))
            {
                TriggerAfterfire(asset);
                _lastPopTime = Time.time;
            }

            if (Time.time - _lastPopTime > 0.07f)
            {
                foreach (var l in asset.ExhaustFlashLights)
                {
                    if (l != null && l.enabled) l.enabled = false;
                }
            }

            _prevRpm = rpm;
            _prevThrottle = throttle;
            _prevGear = gear;
        }

        public static void TriggerAfterfire(LoadedVehicleAsset asset)
        {
            if (asset == null) return;

            foreach (var fps in asset.ExhaustFlameSystems)
            {
                if (fps != null)
                {
                    fps.Emit(UnityEngine.Random.Range(4, 9));
                }
            }

            foreach (var l in asset.ExhaustFlashLights)
            {
                if (l != null) l.enabled = true;
            }

            if (asset.ExhaustAudioSource != null && _cachedPopClip != null)
            {
                asset.ExhaustAudioSource.pitch = UnityEngine.Random.Range(0.85f, 1.25f);
                asset.ExhaustAudioSource.PlayOneShot(_cachedPopClip, UnityEngine.Random.Range(0.6f, 0.9f));
            }
        }

        private static AudioClip GenerateSyntheticBackfireClip()
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * 0.12f);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 45f);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.8f;
                float boom = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.6f;
                samples[i] = (noise + boom) * env;
            }

            var clip = AudioClip.Create("CustomExhaustPop", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

