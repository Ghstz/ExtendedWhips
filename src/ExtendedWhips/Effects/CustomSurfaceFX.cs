using System;
using System.Collections.Generic;
using UnityEngine;
using ExtendedWhips.Models;

namespace ExtendedWhips.Effects
{
    public static class CustomSurfaceFX
    {
        private static List<ParticleSystem> _burnoutSmokeSystems = new();
        private static List<ParticleSystem> _roostertailSystems = new();
        private static float _smokeCarry = 0f;
        private static float _roostCarry = 0f;

        public static void InitializeSurfaceFX(LoadedVehicleAsset asset)
        {
            if (asset == null || asset.RootObject == null) return;

            foreach (var ps in _burnoutSmokeSystems) if (ps != null) UnityEngine.Object.Destroy(ps.gameObject);
            foreach (var ps in _roostertailSystems) if (ps != null) UnityEngine.Object.Destroy(ps.gameObject);

            _burnoutSmokeSystems.Clear();
            _roostertailSystems.Clear();

            bool isBike = asset.Spec.Category == VehicleTypeCategory.Motorcycle;
            float rearZ = isBike ? -0.80f : -1.80f;
            float rearX = isBike ? 0.0f : 0.70f;
            float contactY = 0.05f;

            if (isBike)
            {
                CreateWheelFX(asset.RootObject.transform, new Vector3(0f, contactY, rearZ));
            }
            else
            {
                CreateWheelFX(asset.RootObject.transform, new Vector3(-rearX, contactY, rearZ));
                CreateWheelFX(asset.RootObject.transform, new Vector3(+rearX, contactY, rearZ));
            }
        }

        private static void CreateWheelFX(Transform parent, Vector3 localPos)
        {
            // 1. Burnout Rubber Smoke Emitter
            var smokeObj = new GameObject("BurnoutSmokeEmitter");
            smokeObj.transform.SetParent(parent, false);
            smokeObj.transform.localPosition = localPos;

            var smokePs = smokeObj.AddComponent<ParticleSystem>();
            var sMain = smokePs.main;
            sMain.startLifetime = 1.4f;
            sMain.startSpeed = 2.5f;
            sMain.startSize = 0.85f;
            sMain.startColor = new Color(0.90f, 0.90f, 0.92f, 0.45f);
            sMain.simulationSpace = ParticleSystemSimulationSpace.World;
            sMain.maxParticles = 80;

            var sEmission = smokePs.emission;
            sEmission.rateOverTime = 0f;
            _burnoutSmokeSystems.Add(smokePs);

            // 2. Roostertail Dirt & Gravel Spray Emitter
            var roostObj = new GameObject("RoostertailDebrisEmitter");
            roostObj.transform.SetParent(parent, false);
            roostObj.transform.localPosition = localPos;
            roostObj.transform.localRotation = Quaternion.Euler(-25f, 180f, 0f); // Spray backwards and up

            var roostPs = roostObj.AddComponent<ParticleSystem>();
            var rMain = roostPs.main;
            rMain.startLifetime = 0.45f;
            rMain.startSpeed = 9.0f;
            rMain.startSize = 0.12f;
            rMain.startColor = new Color(0.48f, 0.38f, 0.28f, 0.85f); // Construction dirt brown
            rMain.simulationSpace = ParticleSystemSimulationSpace.World;
            rMain.maxParticles = 60;

            var rEmission = roostPs.emission;
            rEmission.rateOverTime = 0f;
            _roostertailSystems.Add(roostPs);
        }

        public static void UpdateSurfaceFX(VPVehicleController v, LoadedVehicleAsset asset)
        {
            if (v == null || asset == null) return;

            float speed = Mathf.Abs(v.speed);
            float throttle = 0f;
            try
            {
                if (v.data != null)
                {
                    throttle = v.data.Get(VehiclePhysics.Channel.Input, VehiclePhysics.InputData.Throttle) / 10000f;
                }
            }
            catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

            // Check Burnout condition (high throttle, low ground speed)
            bool isBurnout = (throttle > 0.45f && speed < 8.0f);
            if (isBurnout)
            {
                _smokeCarry += 300f * Time.deltaTime; // ~5 particles/frame @60fps
                int n = (int)_smokeCarry; _smokeCarry -= n;
                if (n > 0)
                {
                    for (int i = 0; i < _burnoutSmokeSystems.Count; i++)
                    {
                        var ps = _burnoutSmokeSystems[i];
                        if (ps != null) ps.Emit(n);
                    }
                }
            }

            // Check Roostertail condition (high torque launch on construction ground)
            bool isRoosting = (throttle > 0.50f && speed > 2.0f && speed < 22.0f);
            if (isRoosting)
            {
                _roostCarry += 210f * Time.deltaTime; // ~3.5 particles/frame @60fps
                int n = (int)_roostCarry; _roostCarry -= n;
                if (n > 0)
                {
                    for (int i = 0; i < _roostertailSystems.Count; i++)
                    {
                        var ps = _roostertailSystems[i];
                        if (ps != null) ps.Emit(n);
                    }
                }
            }
        }
    }
}

