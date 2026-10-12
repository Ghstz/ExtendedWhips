using System;
using System.Collections.Generic;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class CustomEmergencyLighting
    {
        public static bool IsStrobesActive { get; private set; } = false;
        public static bool IsSirenActive { get; private set; } = false;

        private static List<Light> _strobeLights = new();
        private static AudioSource? _sirenAudioSource;
        private static AudioClip? _sirenClip;
        private static float _strobeTimer = 0f;
        private static bool _strobePhase = false;

        public static void InitializeEmergencyLighting(LoadedVehicleAsset asset)
        {
            if (asset == null || asset.RootObject == null) return;

            foreach (var l in _strobeLights) if (l != null) UnityEngine.Object.Destroy(l.gameObject);
            _strobeLights.Clear();

            if (_sirenAudioSource != null) UnityEngine.Object.Destroy(_sirenAudioSource.gameObject);

            bool isTruck = asset.Spec.Category == VehicleTypeCategory.Truck || 
                           asset.Spec.Category == VehicleTypeCategory.Offroad4x4;

            bool isSports = asset.Spec.Category == VehicleTypeCategory.SportsCar || 
                            asset.Spec.Category == VehicleTypeCategory.Supercar;

            // 1. Roof Beacon / Lightbar Lights
            float roofY = isTruck ? 1.95f : 1.35f;
            float roofZ = isTruck ? 0.35f : -0.2f;

            Color colorA = isTruck ? new Color(1.0f, 0.65f, 0.0f) : Color.red;
            Color colorB = isTruck ? new Color(1.0f, 0.65f, 0.0f) : Color.blue;

            // Left & Right roof strobes
            CreateStrobe(asset.RootObject.transform, new Vector3(-0.45f, roofY, roofZ), colorA, 0);
            CreateStrobe(asset.RootObject.transform, new Vector3(+0.45f, roofY, roofZ), colorB, 1);

            // Grille Strobes
            float grilleY = isTruck ? 0.95f : 0.55f;
            float grilleZ = isTruck ? 2.4f : 1.9f;
            CreateStrobe(asset.RootObject.transform, new Vector3(-0.35f, grilleY, grilleZ), colorA, 0);
            CreateStrobe(asset.RootObject.transform, new Vector3(+0.35f, grilleY, grilleZ), colorB, 1);

            // 2. Electronic Siren Audio Source
            var sirenObj = new GameObject("EmergencySirenAudio");
            sirenObj.transform.SetParent(asset.RootObject.transform, false);
            sirenObj.transform.localPosition = new Vector3(0f, grilleY, grilleZ);

            _sirenAudioSource = sirenObj.AddComponent<AudioSource>();
            _sirenAudioSource.spatialBlend = 1.0f;
            _sirenAudioSource.minDistance = 5.0f;
            _sirenAudioSource.maxDistance = 120.0f;
            _sirenAudioSource.loop = true;
            _sirenAudioSource.volume = 0.85f;

            if (_sirenClip == null) _sirenClip = GenerateSirenAudioClip();
            _sirenAudioSource.clip = _sirenClip;
        }

        private static void CreateStrobe(Transform parent, Vector3 localPos, Color color, int phaseIndex)
        {
            var lightObj = new GameObject($"StrobeLight_P{phaseIndex}");
            lightObj.transform.SetParent(parent, false);
            lightObj.transform.localPosition = localPos;

            var l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 14f;
            l.intensity = 0f;
            l.color = color;
            l.enabled = true;

            _strobeLights.Add(l);
        }

        public static void ToggleStrobes()
        {
            IsStrobesActive = !IsStrobesActive;
            if (!IsStrobesActive)
            {
                foreach (var l in _strobeLights) if (l != null) l.intensity = 0f;
            }
        }

        public static void ToggleSiren()
        {
            IsSirenActive = !IsSirenActive;
            if (_sirenAudioSource != null)
            {
                if (IsSirenActive) _sirenAudioSource.Play();
                else _sirenAudioSource.Stop();
            }
        }

        public static void UpdateEmergencyLights(VPVehicleController v, LoadedVehicleAsset asset)
        {
            // Key shortcut [J] toggles Strobes, [H] toggles Siren
            if (Input.GetKeyDown(KeyCode.J)) ToggleStrobes();
            if (Input.GetKeyDown(KeyCode.H)) ToggleSiren();

            if (!IsStrobesActive)
            {
                foreach (var l in _strobeLights) if (l != null && l.intensity > 0f) l.intensity = 0f;
                return;
            }

            _strobeTimer += Time.deltaTime;
            if (_strobeTimer >= 0.12f)
            {
                _strobeTimer = 0f;
                _strobePhase = !_strobePhase;
            }

            for (int i = 0; i < _strobeLights.Count; i++)
            {
                var l = _strobeLights[i];
                if (l == null) continue;

                bool isPhaseA = (i % 2 == 0);
                bool shouldShine = isPhaseA ? _strobePhase : !_strobePhase;
                l.intensity = shouldShine ? 5.5f : 0f;
            }
        }

        private static AudioClip GenerateSirenAudioClip()
        {
            int sampleRate = 44100;
            float duration = 2.0f; // 2 second wail loop
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // Dual tone wail oscillating between 650 Hz and 1150 Hz
                float sweep = (Mathf.Sin(t * Mathf.PI * 1.5f) + 1f) * 0.5f;
                float freq = Mathf.Lerp(650f, 1150f, sweep);
                float val = Mathf.Sin(t * Mathf.PI * 2f * freq);
                // Mild harmonic overtone
                val += 0.35f * Mathf.Sin(t * Mathf.PI * 4f * freq);
                samples[i] = Mathf.Clamp(val * 0.45f, -1f, 1f);
            }

            var clip = AudioClip.Create("EmergencySirenWail", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

