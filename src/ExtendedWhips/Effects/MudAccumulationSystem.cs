using System;
using UnityEngine;
using ExtendedWhips.Models;
using ExtendedWhips.UI;

namespace ExtendedWhips.Effects
{
    public static class MudAccumulationSystem
    {
        public static float MudAmount { get; private set; } = 0f; // 0.0 (Clean) to 1.0 (Muddy)

        public static void UpdateMud(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            float speed = Mathf.Abs(v.speed);
            if (speed > 1.5f && MudAmount < 1.0f)
            {
                // Accumulate dirt slowly as you drive through construction grounds
                MudAmount = Mathf.Min(1.0f, MudAmount + (0.005f * Time.deltaTime));
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

