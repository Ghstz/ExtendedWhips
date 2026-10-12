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

            if (_synthwaveClip == null) _synthwaveClip = GenerateChillSynthwaveClip();
            if (_synthwaveClip != null && _synthwaveClip.samples > 0)
            {
                _audioSource.clip = _synthwaveClip;
            }
        }

        public static void TogglePlayback()
        {
            if (_audioSource == null) return;
            IsPlaying = !IsPlaying;
            if (IsPlaying)
            {
                if (_audioSource.clip != null && _audioSource.clip.samples > 0)
                {
                    try { _audioSource.Play(); }
                    catch (Exception ex) { Plugin.LogOnce(ex); IsPlaying = false; }
                }
                else
                {
                    IsPlaying = false;
                }
            }
            else
            {
                try { _audioSource.Pause(); }
                catch (Exception ex) { Plugin.LogOnce(ex); }
            }
        }

        public static void UpdateStereo(VPVehicleController v, bool isCockpitView)
        {
            if (_audioSource == null) return;

            // Spatial Acoustic Simulation:
            // Inside cockpit: direct cabin sound (full volume, lower spatial blend)
            // Outside in third person: muffled/attenuated sound leaking through windows (lower volume, full 3D blend)
            float targetVol = isCockpitView ? Volume : (Volume * 0.35f);
            float targetBlend = isCockpitView ? 0.2f : 1.0f;

            _audioSource.volume = Mathf.Lerp(_audioSource.volume, targetVol, Time.deltaTime * 6f);
            _audioSource.spatialBlend = Mathf.Lerp(_audioSource.spatialBlend, targetBlend, Time.deltaTime * 6f);
        }

        private static AudioClip? GenerateChillSynthwaveClip()
        {
            try
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
                if (clip == null) return null;
                if (!clip.SetData(samples, 0)) return null;
                if (clip.samples <= 0) return null;
                return clip;
            }
            catch (Exception ex)
            {
                Plugin.LogOnce(ex);
                return null;
            }
        }

        private static class Paths
        {
            public static string? PluginPath => BepInEx.Paths.PluginPath;
        }
    }
}

