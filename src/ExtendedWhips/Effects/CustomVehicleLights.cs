using System;
using System.Collections.Generic;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class CustomVehicleLights
    {
        private static bool _manualHeadlightsOn = false;

        public static void InitializeLights(LoadedVehicleAsset asset)
        {
            if (asset == null || asset.RootObject == null) return;

            var spec = asset.Spec;
            bool isBike = spec.Category == VehicleTypeCategory.Motorcycle;

            foreach (var l in asset.HeadlightSources) if (l != null) UnityEngine.Object.Destroy(l.gameObject);
            foreach (var l in asset.TaillightSources) if (l != null) UnityEngine.Object.Destroy(l.gameObject);
            foreach (var l in asset.ReverseSources) if (l != null) UnityEngine.Object.Destroy(l.gameObject);
            foreach (var l in asset.IndicatorLeftSources) if (l != null) UnityEngine.Object.Destroy(l.gameObject);
            foreach (var l in asset.IndicatorRightSources) if (l != null) UnityEngine.Object.Destroy(l.gameObject);

            asset.HeadlightSources.Clear();
            asset.TaillightSources.Clear();
            asset.ReverseSources.Clear();
            asset.IndicatorLeftSources.Clear();
            asset.IndicatorRightSources.Clear();

            // 1. Headlights (Unity Spotlights)
            float frontZ = isBike ? 0.95f : 2.25f;
            float frontY = isBike ? 0.85f : 0.65f;
            float frontX = isBike ? 0.0f : 0.65f;

            if (isBike)
            {
                CreateSpotlight(asset.RootObject.transform, new Vector3(0f, frontY, frontZ), asset.HeadlightSources, 65f, 50f);
            }
            else
            {
                CreateSpotlight(asset.RootObject.transform, new Vector3(-frontX, frontY, frontZ), asset.HeadlightSources, 60f, 45f);
                CreateSpotlight(asset.RootObject.transform, new Vector3(+frontX, frontY, frontZ), asset.HeadlightSources, 60f, 45f);
            }

            // 2. Taillights & Brake Lights (Unity Pointlights)
            float rearZ = isBike ? -0.85f : -2.35f;
            float rearY = isBike ? 0.80f : 0.70f;
            float rearX = isBike ? 0.0f : 0.65f;

            if (isBike)
            {
                CreatePointLight(asset.RootObject.transform, new Vector3(0f, rearY, rearZ), new Color(1.0f, 0.05f, 0.02f), 8f, asset.TaillightSources);
            }
            else
            {
                CreatePointLight(asset.RootObject.transform, new Vector3(-rearX, rearY, rearZ), new Color(1.0f, 0.05f, 0.02f), 7f, asset.TaillightSources);
                CreatePointLight(asset.RootObject.transform, new Vector3(+rearX, rearY, rearZ), new Color(1.0f, 0.05f, 0.02f), 7f, asset.TaillightSources);
            }

            // 3. Reverse Lights (White Pointlights)
            if (!isBike)
            {
                CreatePointLight(asset.RootObject.transform, new Vector3(-rearX * 0.7f, rearY, rearZ), Color.white, 9f, asset.ReverseSources);
                CreatePointLight(asset.RootObject.transform, new Vector3(+rearX * 0.7f, rearY, rearZ), Color.white, 9f, asset.ReverseSources);
            }

            // 4. Amber Indicators
            CreatePointLight(asset.RootObject.transform, new Vector3(-frontX, frontY, frontZ * 0.9f), new Color(1.0f, 0.60f, 0.0f), 5f, asset.IndicatorLeftSources);
            CreatePointLight(asset.RootObject.transform, new Vector3(-rearX, rearY, rearZ * 0.9f), new Color(1.0f, 0.60f, 0.0f), 5f, asset.IndicatorLeftSources);

            CreatePointLight(asset.RootObject.transform, new Vector3(+frontX, frontY, frontZ * 0.9f), new Color(1.0f, 0.60f, 0.0f), 5f, asset.IndicatorRightSources);
            CreatePointLight(asset.RootObject.transform, new Vector3(+rearX, rearY, rearZ * 0.9f), new Color(1.0f, 0.60f, 0.0f), 5f, asset.IndicatorRightSources);
        }

        private static void CreateSpotlight(Transform parent, Vector3 localPos, List<Light> list, float spotAngle, float range)
        {
            var go = new GameObject("CustomHeadlight_Spot");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(7.5f, 0f, 0f);

            var l = go.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = new Color(0.96f, 0.96f, 1.0f);
            l.spotAngle = spotAngle;
            l.range = range;
            l.intensity = 1500f;
            l.enabled = false;
            list.Add(l);
        }

        private static void CreatePointLight(Transform parent, Vector3 localPos, Color color, float range, List<Light> list)
        {
            var go = new GameObject("CustomLight_Point");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = 200f;
            l.enabled = false;
            list.Add(l);
        }

        public static void UpdateLights(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            if (Input.GetKeyDown(KeyCode.L))
            {
                _manualHeadlightsOn = !_manualHeadlightsOn;
            }

            bool headlightsOn = _manualHeadlightsOn;
            bool indLeft = false;
            bool indRight = false;

            try
            {
                var gameLights = v.GetComponentInChildren<Wb.WbVehicleLights>();
                if (gameLights != null)
                {
                    if (gameLights.headLights != null && gameLights.headLights.Count > 0)
                    {
                        headlightsOn = gameLights.headLights[0].IsOn || _manualHeadlightsOn;
                    }

                    if (gameLights.indicatorLeft != null && gameLights.indicatorLeft.Count > 0)
                    {
                        indLeft = gameLights.indicatorLeft[0].IsOn;
                    }
                    if (gameLights.indicatorRight != null && gameLights.indicatorRight.Count > 0)
                    {
                        indRight = gameLights.indicatorRight[0].IsOn;
                    }
                }
            }
            catch { }

            // 1. Headlights
            foreach (var l in asset.HeadlightSources)
            {
                if (l != null) l.enabled = headlightsOn;
            }
            Color hlEmission = headlightsOn ? new Color(2.8f, 2.8f, 3.2f) : Color.black;
            foreach (var m in asset.HeadlightMaterials)
            {
                if (m == null) continue;
                if (headlightsOn) m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", hlEmission);
            }

            // 2. Brake & Tail Lights
            bool isBraking = false;
            try
            {
                if (v.data != null)
                {
                    isBraking = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Brake) > 500;
                }
            }
            catch { }

            foreach (var l in asset.TaillightSources)
            {
                if (l == null) continue;
                if (isBraking)
                {
                    l.enabled = true;
                    l.intensity = 500f;
                }
                else if (headlightsOn)
                {
                    l.enabled = true;
                    l.intensity = 70f;
                }
                else
                {
                    l.enabled = false;
                }
            }

            Color tlEmission = isBraking ? new Color(4.5f, 0.05f, 0.05f) : (headlightsOn ? new Color(0.8f, 0.03f, 0.03f) : Color.black);
            foreach (var m in asset.TaillightMaterials)
            {
                if (m == null) continue;
                if (isBraking || headlightsOn) m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", tlEmission);
            }

            // 3. Reverse Lights
            bool isReverse = false;
            try
            {
                if (v.data != null)
                {
                    isReverse = v.data.Get(VehiclePhysics.Channel.Vehicle, VehiclePhysics.VehicleData.GearboxGear) < 0;
                }
            }
            catch { }

            foreach (var l in asset.ReverseSources)
            {
                if (l != null) l.enabled = isReverse;
            }
            Color revEmission = isReverse ? new Color(2.5f, 2.5f, 2.5f) : Color.black;
            foreach (var m in asset.ReverseMaterials)
            {
                if (m == null) continue;
                if (isReverse) m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", revEmission);
            }

            // 4. Turn Indicators
            bool blinkState = (Time.time * 3.5f % 1.0f) < 0.5f;
            foreach (var l in asset.IndicatorLeftSources)
            {
                if (l != null) l.enabled = indLeft && blinkState;
            }
            foreach (var l in asset.IndicatorRightSources)
            {
                if (l != null) l.enabled = indRight && blinkState;
            }

            Color indColor = blinkState ? new Color(3.5f, 1.8f, 0.0f) : Color.black;
            foreach (var m in asset.IndicatorMaterials)
            {
                if (m == null) continue;
                if ((indLeft || indRight) && blinkState) m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", indColor);
            }
        }
    }
}

