using System;
using System.IO;
using UnityEngine;
using ExtendedWhips.Hooks;

namespace ExtendedWhips.Tuning
{
    public static class TuningPresetCodeSystem
    {
        public static string LastGeneratedCode { get; private set; } = "";
        public static string StatusMessage { get; set; } = "";

        public static string GenerateShareCode(TuningProfile profile)
        {
            if (profile == null) return "WHIP-0000";

            // Encode key parameters: Torque, Suspension Height, Grip, Steer Angle, Downforce
            int t = Mathf.RoundToInt(Mathf.Clamp(profile.TorqueMultiplier, 0.5f, 10f) * 10f);
            int h = Mathf.RoundToInt((Mathf.Clamp(profile.RideHeightOffset, -0.2f, 0.5f) + 0.2f) * 100f);
            int g = Mathf.RoundToInt(Mathf.Clamp(profile.TireForwardGripMultiplier, 0.5f, 3.0f) * 10f);
            int s = Mathf.RoundToInt(Mathf.Clamp(profile.CustomSteerAngle, 20f, 70f));
            int d = Mathf.RoundToInt(Mathf.Clamp(profile.DownforceMultiplier, 0f, 5.0f) * 10f);

            string code = $"WHIP-{t:X2}{h:X2}-{g:X2}{s:X2}{d:X2}";
            LastGeneratedCode = code;
            StatusMessage = $"Generated Code: {code}";
            return code;
        }

        public static bool ApplyShareCode(string code, TuningProfile profile)
        {
            if (string.IsNullOrEmpty(code) || profile == null) return false;

            try
            {
                code = code.Trim().ToUpperInvariant();
                if (!code.StartsWith("WHIP-")) return false;

                string clean = code.Replace("WHIP-", "").Replace("-", "");
                if (clean.Length < 10) return false;

                int t = Convert.ToInt32(clean.Substring(0, 2), 16);
                int h = Convert.ToInt32(clean.Substring(2, 2), 16);
                int g = Convert.ToInt32(clean.Substring(4, 2), 16);
                int s = Convert.ToInt32(clean.Substring(6, 2), 16);
                int d = Convert.ToInt32(clean.Substring(8, 2), 16);

                profile.TorqueMultiplier = Mathf.Clamp((float)t / 10f, 0.5f, 10f);
                profile.RideHeightOffset = Mathf.Clamp(((float)h / 100f) - 0.2f, -0.2f, 0.5f);
                profile.TireForwardGripMultiplier = Mathf.Clamp((float)g / 10f, 0.5f, 3.0f);
                profile.CustomSteerAngle = Mathf.Clamp((float)s, 20f, 70f);
                profile.DownforceMultiplier = Mathf.Clamp((float)d / 10f, 0f, 5.0f);

                StatusMessage = "Successfully applied share code tune!";
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Invalid code: {ex.Message}";
                return false;
            }
        }
    }
}

