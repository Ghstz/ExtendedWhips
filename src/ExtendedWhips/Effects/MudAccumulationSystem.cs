using System;
using UnityEngine;
using ExtendedWhips.Models;
using ExtendedWhips.UI;

namespace ExtendedWhips.Effects
{
    public static class MudAccumulationSystem
    {
        public static float MudAmount { get; private set; } = 0f; // 0.0 (Clean) to 1.0 (Muddy)

        private static float _appliedMud = 0f;
        private static readonly Color MudColor = new Color(0.36f, 0.27f, 0.18f, 1f);

        /// <summary>Forces the next UpdateMud to re-tint (call after paint changes overwrite the materials).</summary>
        public static void MarkVisualsDirty() { _appliedMud = -1f; }

        public static void UpdateMud(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            float speed = Mathf.Abs(v.speed);
            if (speed > 1.5f && MudAmount < 1.0f)
            {
                // Accumulate dirt slowly as you drive through construction grounds
                MudAmount = Mathf.Min(1.0f, MudAmount + (0.005f * Time.deltaTime));
            }

            // Throttle material writes: only when mud changed visibly (or was marked dirty)
            if (Mathf.Abs(MudAmount - _appliedMud) < 0.02f && _appliedMud >= 0f) return;
            _appliedMud = MudAmount;
            if (MudAmount <= 0f) return; // clean state is restored by WashVehicle / paint

            var state = ExtendedWhips.Hooks.VehicleReskinManager.GetState(v);
            Color basePaint = state != null ? state.CurrentPaintColor : Color.white;
            float baseSmooth = state != null ? state.CurrentSmoothness : 0.85f;
            Color tinted = Color.Lerp(basePaint, MudColor, MudAmount * 0.8f);
            float smooth = Mathf.Lerp(baseSmooth, 0.1f, MudAmount * 0.8f);

            foreach (var mat in asset.BodyMaterials)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tinted);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", tinted);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smooth);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smooth);
            }
        }

        public static void WashVehicle(LoadedVehicleAsset asset, Color originalPaint, float originalSmoothness)
        {
            if (asset == null) return;

            MudAmount = 0f;

            // Restore body materials to clean mirror finish
            foreach (var mat in asset.BodyMaterials)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", originalPaint);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", originalSmoothness);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", originalSmoothness);
            }

            ExtendedWhipsUI.ShowToast("PRESSURE WASH COMPLETE", "Vehicle washed & detailed. Showroom mirror shine restored!");
        }
    }
}

