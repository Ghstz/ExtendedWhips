using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class InCabinStereo
    {
        public static bool IsPlaying { get; private set; } = false;
        public static string CurrentTrackTitle { get; private set; } = "Synthwave Cruise (Procedural)";
        public static float Volume { get; set; } = 0.70f;

        private static AudioSource? _audioSource;
        private static AudioLowPassFilter? _lowPassFilter;
        private static AudioClip? _synthwaveClip;
        private static readonly string _musicDir = Path.Combine(Paths.PluginPath ?? "BepInEx/plugins", "ExtendedWhips", "Music");

        public static void InitializeStereo(LoadedVehicleAsset asset)
        {
            if (asset == null || asset.RootObject == null) return;

            if (_audioSource != null) UnityEngine.Object.Destroy(_audioSource.gameObject);

            try
            {
                if (!Directory.Exists(_musicDir)) Directory.CreateDirectory(_musicDir);
            }
            catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            var stereoObj = new GameObject("InCabinStereoAudio");
            stereoObj.transform.SetParent(asset.RootObject.transform, false);
            stereoObj.transform.localPosition = new Vector3(0f, 0.85f, 0.2f); // Cockpit center

            _audioSource = stereoObj.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1.0f;
            _audioSource.minDistance = 3.0f;
            _audioSource.maxDistance = 60.0f;
            _audioSource.loop = true;
            _audioSource.volume = Volume;

            _lowPassFilter = stereoObj.AddComponent<AudioLowPassFilter>();
            _lowPassFilter.cutoffFrequency = 22000f; // Default wide open

            if (_synthwaveClip == null) _synthwaveClip = GenerateChillSynthwaveClip();
            _audioSource.clip = _synthwaveClip;
        }

        public static void TogglePlayback()
        {
            if (_audioSource == null) return;
            IsPlaying = !IsPlaying;
            if (IsPlaying) _audioSource.Play();
            else _audioSource.Pause();
        }

        public static void UpdateStereo(VPVehicleController v, bool isCockpitView)
        {
            if (_audioSource == null) return;
            _audioSource.volume = Volume;

            // Spatial Acoustic Filtering:
            // Inside cockpit: crystal-clear treble & punchy bass (22,000 Hz)
            // Outside in third person: muffled reverberant sound leaking through windows (1,600 Hz)
            if (_lowPassFilter != null)
            {
                float targetCutoff = isCockpitView ? 22000f : 1600f;
                _lowPassFilter.cutoffFrequency = Mathf.Lerp(_lowPassFilter.cutoffFrequency, targetCutoff, Time.deltaTime * 6f);
            }
        }

        private static AudioClip GenerateChillSynthwaveClip()
        {
            int sampleRate = 44100;
            float duration = 12.0f; // 12-second musical phrase
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            // 110 BPM chord progression (Am -> F -> C -> G)
            float[] bassNotes = new float[] { 110f, 87.3f, 130.8f, 98f };

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                int bar = Mathf.FloorToInt((t / duration) * 4) % 4;
                float bassFreq = bassNotes[bar];

                // Warm analog bassline
                float bass = Mathf.Sin(t * Mathf.PI * 2f * bassFreq) * 0.45f;
                bass += Mathf.Sin(t * Mathf.PI * 4f * bassFreq) * 0.2f;

                // Ambient shimmer pad
                float padFreq = bassFreq * 4f;
                float pad = Mathf.Sin(t * Mathf.PI * 2f * padFreq) * 0.12f;

                // Kick pulse on each beat
                float beat = (t * (110f / 60f)) % 1.0f;
                float kick = Mathf.Exp(-beat * 14f) * Mathf.Sin(beat * Mathf.PI * 2f * 60f) * 0.35f;

                samples[i] = Mathf.Clamp((bass + pad + kick) * 0.55f, -1f, 1f);
            }

            var clip = AudioClip.Create("SynthwaveCruise", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static class Paths
        {
            public static string? PluginPath => BepInEx.Paths.PluginPath;
        }
    }
}

