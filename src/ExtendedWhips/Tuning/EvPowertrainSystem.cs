using System;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Tuning
{
    public static class EvPowertrainSystem
    {
        public static bool IsEvSwapActive { get; set; } = false;
        public static float EvRegenBrakeStrength { get; set; } = 0.45f; // 45% regenerative braking torque

        private static AudioSource? _evAudioSource;
        private static AudioClip? _inverterWhineClip;

        public static void InitializeEvAudio(LoadedVehicleAsset asset)
        {
            if (asset == null || asset.RootObject == null) return;
            if (_evAudioSource != null) UnityEngine.Object.Destroy(_evAudioSource.gameObject);

            var evObj = new GameObject("EvInverterSound");
            evObj.transform.SetParent(asset.RootObject.transform, false);
            evObj.transform.localPosition = new Vector3(0f, 0.4f, 0.5f);

            _evAudioSource = evObj.AddComponent<AudioSource>();
            _evAudioSource.spatialBlend = 1.0f;
            _evAudioSource.minDistance = 2.0f;
            _evAudioSource.maxDistance = 50.0f;
            _evAudioSource.loop = true;
            _evAudioSource.volume = 0f;

            if (_inverterWhineClip == null) _inverterWhineClip = GenerateInverterWhineClip();
            _evAudioSource.clip = _inverterWhineClip;
            _evAudioSource.Play();
        }

        public static void UpdateEvPowertrain(VPVehicleController v)
        {
            if (v == null) return;

            if (!IsEvSwapActive)
            {
                if (_evAudioSource != null && _evAudioSource.volume > 0f) _evAudioSource.volume = 0f;
                return;
            }

            float speedKmh = Mathf.Abs(v.speed * 3.6f);
            float throttle = 0f;
            try
            {
                if (v.data != null)
                {
                    throttle = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Throttle) / 10000f;
                }
            }
            catch { }

            // 1. Instant Electric Torque Injection
            if (throttle > 0.05f && v.cachedRigidbody != null)
            {
                // Instant flat EV torque delivery
                v.cachedRigidbody.AddForce(v.transform.forward * (throttle * 3800f), ForceMode.Force);
            }

            // 2. One-Pedal Regenerative Braking
            if (throttle < 0.05f && speedKmh > 5.0f && v.cachedRigidbody != null)
            {
                Vector3 brakeDir = -v.cachedRigidbody.velocity.normalized;
                v.cachedRigidbody.AddForce(brakeDir * (EvRegenBrakeStrength * 2800f), ForceMode.Force);
            }

            // 3. Cyber Inverter Sound Modulation
            if (_evAudioSource != null)
            {
                float targetVol = Mathf.Clamp01(speedKmh / 90f) * 0.45f;
                _evAudioSource.volume = Mathf.Lerp(_evAudioSource.volume, targetVol, Time.deltaTime * 6f);
                // Pitch rises up to 3.0x with speed
                _evAudioSource.pitch = Mathf.Clamp(0.5f + (speedKmh / 60f) * 1.5f, 0.4f, 3.2f);
            }
        }

        private static AudioClip GenerateInverterWhineClip()
        {
            int sampleRate = 44100;
            float duration = 1.0f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // High-frequency pure electric carrier wave (3,400 Hz)
                float val = Mathf.Sin(t * Mathf.PI * 2f * 3400f) * 0.35f;
                val += Mathf.Sin(t * Mathf.PI * 2f * 6800f) * 0.15f;
                samples[i] = val;
            }

            var clip = AudioClip.Create("EvInverterWhine", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

