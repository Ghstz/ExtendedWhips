using System;
using System.IO;
using UnityEngine;

namespace ExtendedWhips.Spawning
{
    public class VehicleSpawner
    {
        private static VehicleSpawner? _instance;
        public static VehicleSpawner Instance => _instance ??= new VehicleSpawner();

        private AssetBundle? _loadedBundle;
        private GameObject? _vehiclePrefab;

        public void LoadVehicleBundle(string bundlePath, string prefabName)
        {
            if (!File.Exists(bundlePath))
            {
                Debug.LogWarning($"[ExtendedWhips] Vehicle bundle not found at: {bundlePath}");
                return;
            }

            try
            {
                _loadedBundle = AssetBundle.LoadFromFile(bundlePath);
                if (_loadedBundle != null)
                {
                    var loadedObj = _loadedBundle.LoadAsset(prefabName);
                    if (loadedObj != null)
                    {
                        _vehiclePrefab = loadedObj.TryCast<GameObject>();
                        Debug.Log($"[ExtendedWhips] Successfully loaded vehicle prefab: {prefabName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ExtendedWhips] Error loading vehicle bundle: {ex.Message}");
            }
        }

        public void SpawnCurrentVehicle()
        {
            if (_vehiclePrefab == null)
            {
                Debug.LogWarning("[ExtendedWhips] No custom vehicle prefab currently loaded.");
                return;
            }

            // Spawn 5 meters in front of the main camera
            Camera mainCam = Camera.main;
            Vector3 spawnPos = mainCam != null ? mainCam.transform.position + mainCam.transform.forward * 6f : Vector3.zero;
            Quaternion spawnRot = mainCam != null ? Quaternion.Euler(0, mainCam.transform.eulerAngles.y, 0) : Quaternion.identity;

            GameObject spawned = GameObject.Instantiate(_vehiclePrefab, spawnPos, spawnRot);
            Debug.Log($"[ExtendedWhips] Spawned vehicle at: {spawnPos}");
        }

        public static GameObject? FindActivePlayerVehicle()
        {
            // Search for game object hierarchy that has WheelColliders and is near the camera
            Camera mainCam = Camera.main;
            if (mainCam == null) return null;

            // Search through active GameObjects with Rigidbody and WheelColliders
            var rigidbodies = GameObject.FindObjectsOfType<Rigidbody>();
            foreach (var rb in rigidbodies)
            {
                var wheels = rb.GetComponentsInChildren<WheelCollider>();
                if (wheels != null && wheels.Length >= 2)
                {
                    // Check if camera is close to or parented to this vehicle
                    float dist = Vector3.Distance(rb.transform.position, mainCam.transform.position);
                    if (dist < 15f)
                    {
                        return rb.gameObject;
                    }
                }
            }

            return null;
        }
    }
}

