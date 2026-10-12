using System;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class CustomVehicleAudio
    {
        private static AudioSource? _audioSource;
        private static AudioClip? _superchargerClip;
        private static AudioClip? _turboBovClip;
        private static AudioClip? _jakeBrakeClip;

        private static float _prevThrottle = 0f;
        private static float _prevRpm = 0f;
        private static float _boostPressure = 0f; // 0 to 1.8 bar
        private static float _lastBovTime = 0f;
        private static float _lastJakeTime = 0f;

        public static void InitializeAudio(LoadedVehicleAsset asset)
        {
            if (asset == null || asset.RootObject == null) return;

            if (_audioSource != null)
            {
                UnityEngine.Object.Destroy(_audioSource.gameObject);
            }

            var audioObj = new GameObject("CustomEngineAudioSynthesizer");
            audioObj.transform.SetParent(asset.RootObject.transform, false);
            audioObj.transform.localPosition = new Vector3(0f, 0.7f, 1.2f); // Engine bay

            _audioSource = audioObj.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1.0f;
            _audioSource.minDistance = 3.0f;
            _audioSource.maxDistance = 50.0f;
            _audioSource.volume = 0.85f;

            if (_superchargerClip == null) _superchargerClip = GenerateSuperchargerClip();
            if (_turboBovClip == null) _turboBovClip = GenerateBovFlutterClip();
            if (_jakeBrakeClip == null) _jakeBrakeClip = GenerateJakeBrakeClip();
        }

        public static void UpdateAudio(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null || _audioSource == null) return;

            float rpm = 0f;
            float throttle = 0f;
            int gear = 0;

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

            var spec = asset.Spec;
            string modelId = spec.Id;

            // 1. Supercharger Whine (Cuda 426 Hemi & Raptor R Supercharged V8)
            if (modelId == "cuda71" || modelId == "raptor_r")
            {
                if (throttle > 0.15f && rpm > 2200f && UnityEngine.Random.value < 0.20f)
                {
                    float whinePitch = Mathf.Clamp(1.0f + (rpm - 2000f) / 3500f, 1.0f, 2.4f);
                    float whineVol = Mathf.Clamp01(throttle * 0.45f);
                    _audioSource.pitch = whinePitch;
                    _audioSource.PlayOneShot(_superchargerClip, whineVol);
                }
            }

            // 2. Twin-Turbo Boost & Blow-Off Valve (R34 GT-R & BMW M4 GT3)
            if (modelId == "r34" || modelId == "bmw_m4")
            {
                // Accumulate boost under throttle
                if (throttle > 0.25f && rpm > 2500f)
                {
                    _boostPressure = Mathf.MoveTowards(_boostPressure, throttle * 1.6f, Time.deltaTime * 1.8f);
                }
                else
                {
                    _boostPressure = Mathf.MoveTowards(_boostPressure, 0f, Time.deltaTime * 3.5f);
                }

                // Rapid throttle lift-off dump
                if (_prevThrottle > 0.45f && throttle < 0.10f && _boostPressure > 0.35f && (Time.time - _lastBovTime > 0.35f))
                {
                    _audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.15f);
                    _audioSource.PlayOneShot(_turboBovClip, Mathf.Clamp01(_boostPressure * 0.75f));
                    _boostPressure = 0f;
                    _lastBovTime = Time.time;
                }
            }

            // 3. Diesel Engine Jake Brake (Sierra 2500HD Commercial Hauler)
            if (modelId == "sierra_hd")
            {
                if (throttle < 0.05f && rpm > 2400f && gear > 1 && (Time.time - _lastJakeTime > 0.25f))
                {
                    _audioSource.pitch = Mathf.Clamp(0.85f + (rpm / 3000f) * 0.3f, 0.85f, 1.3f);
                    _audioSource.PlayOneShot(_jakeBrakeClip, 0.65f);
                    _lastJakeTime = Time.time;
                }
            }

            _prevThrottle = throttle;
            _prevRpm = rpm;
        }

        // Procedural Supercharger whine synthesizer
        private static AudioClip GenerateSuperchargerClip()
        {
            int sampleRate = 44100;
            int count = (int)(sampleRate * 0.08f);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin((float)i / count * Mathf.PI);
                float f1 = Mathf.Sin(2f * Mathf.PI * 1400f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 2800f * t) * 0.4f;
                samples[i] = (f1 + f2) * env * 0.5f;
            }

            var clip = AudioClip.Create("SuperchargerWhine", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Procedural Turbo Blow-Off Valve (BOV) flutter & sneeze
        private static AudioClip GenerateBovFlutterClip()
        {
            int sampleRate = 44100;
            int count = (int)(sampleRate * 0.28f);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 12f);
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float flutter = Mathf.Sin(2f * Mathf.PI * 24f * t); // Flutter modulation (24 Hz)
                samples[i] = noise * env * (0.6f + 0.4f * flutter) * 0.65f;
            }

            var clip = AudioClip.Create("TurboBovFlutter", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Procedural Jake Brake compression staccato
        private static AudioClip GenerateJakeBrakeClip()
        {
            int sampleRate = 44100;
            int count = (int)(sampleRate * 0.15f);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 28f);
                float chug = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.8f + (UnityEngine.Random.value * 2f - 1f) * 0.4f;
                samples[i] = chug * env * 0.8f;
            }

            var clip = AudioClip.Create("JakeBrakeChug", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

