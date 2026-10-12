using System;
using UnityEngine;
using ExtendedWhips.Hooks;

namespace ExtendedWhips.Cockpit
{
    public static class PhotoModeFreecam
    {
        public static bool IsPhotoModeActive { get; private set; } = false;
        public static bool IsUiHidden { get; set; } = false;

        private static GameObject? _droneCamObject;
        private static UnityEngine.Camera? _droneCamera;
        private static float _fov = 60f;
        private static float _roll = 0f;
        private static float _moveSpeed = 6.0f;
        private static float _priorTimeScale = 1.0f;
        private static float _yaw = 0f;
        private static float _pitch = 0f;

        public static void TogglePhotoMode()
        {
            IsPhotoModeActive = !IsPhotoModeActive;

            if (IsPhotoModeActive)
            {
                _priorTimeScale = Time.timeScale;
                Time.timeScale = 0f; // Freeze time

                var mainCam = UnityEngine.Camera.main;
                Vector3 spawnPos = mainCam != null ? mainCam.transform.position : (VehicleTunerHook.CurrentVehicle != null ? VehicleTunerHook.CurrentVehicle.transform.position + Vector3.up * 2f : Vector3.zero);
                Quaternion spawnRot = mainCam != null ? mainCam.transform.rotation : Quaternion.identity;

                _droneCamObject = new GameObject("ExtendedWhips_PhotoDroneCamera");
                _droneCamObject.transform.position = spawnPos;
                _droneCamObject.transform.rotation = spawnRot;

                _droneCamera = _droneCamObject.AddComponent<UnityEngine.Camera>();
                _droneCamera.fieldOfView = _fov;
                _droneCamera.depth = 999; // Render on top of game cameras

                Vector3 euler = spawnRot.eulerAngles;
                _pitch = euler.x;
                _yaw = euler.y;
                _roll = 0f;
            }
            else
            {
                Time.timeScale = _priorTimeScale > 0f ? _priorTimeScale : 1.0f;
                if (_droneCamObject != null)
                {
                    UnityEngine.Object.Destroy(_droneCamObject);
                    _droneCamObject = null;
                    _droneCamera = null;
                }
            }
        }

        public static void UpdateFreecam()
        {
            if (Input.GetKeyDown(KeyCode.F9)) TogglePhotoMode();
            if (!IsPhotoModeActive || _droneCamObject == null) return;

            if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Tab))
            {
                IsUiHidden = !IsUiHidden;
            }

            // Real-time camera translation (ignores Time.timeScale = 0 using unscaledDeltaTime)
            float dt = Time.unscaledDeltaTime;
            float speed = _moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? 2.5f : 1.0f);

            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += _droneCamObject.transform.forward;
            if (Input.GetKey(KeyCode.S)) move -= _droneCamObject.transform.forward;
            if (Input.GetKey(KeyCode.D)) move += _droneCamObject.transform.right;
            if (Input.GetKey(KeyCode.A)) move -= _droneCamObject.transform.right;
            if (Input.GetKey(KeyCode.E)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;

            _droneCamObject.transform.position += move * speed * dt;

            // Mouse Look (Right Click drag or direct look)
            if (Input.GetMouseButton(1))
            {
                _yaw += Input.GetAxis("Mouse X") * 3.5f;
                _pitch -= Input.GetAxis("Mouse Y") * 3.5f;
            }

            // Roll tilt with Z and X
            if (Input.GetKey(KeyCode.Z)) _roll += 25f * dt;
            if (Input.GetKey(KeyCode.X)) _roll -= 25f * dt;

            _droneCamObject.transform.rotation = Quaternion.Euler(_pitch, _yaw, _roll);

            // FOV zoom with Mouse Scrollwheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _fov = Mathf.Clamp(_fov - scroll * 15f, 15f, 110f);
                if (_droneCamera != null) _droneCamera.fieldOfView = _fov;
            }
        }

        public static void DrawPhotoModeGui()
        {
            if (!IsPhotoModeActive || IsUiHidden) return;

            float width = 360f;
            float height = 240f;
            float x = 20f;
            float y = Screen.height - height - 30f;

            GUI.backgroundColor = new Color(0.04f, 0.06f, 0.09f, 0.95f);
            GUI.Box(new Rect(x, y, width, height), "");

            // Top Cyan Header
            GUI.backgroundColor = new Color(0.0f, 0.85f, 1.0f, 1.0f);
            GUI.Box(new Rect(x, y, width, 3), "");

            GUILayout.BeginArea(new Rect(x + 12, y + 8, width - 24, height - 16));
            try
            {
                GUILayout.Label("<b><color=#00E5FF>CINEMATIC PHOTO MODE</color> // <color=#FFD700>[F9 Exit]</color></b>");
                GUILayout.Label("<size=11><color=#AAAAAA>WASD: Move | Q/E: Elevate | Right-Click: Look | Z/X: Roll | [H]: Hide UI</color></size>");
                GUILayout.Space(4);

                // Focal length / FOV slider
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>Focal Length (FOV):</b> {_fov:F0}°", GUILayout.Width(170));
                float newFov = GUILayout.HorizontalSlider(_fov, 15f, 110f);
                if (Mathf.Abs(newFov - _fov) > 0.1f)
                {
                    _fov = newFov;
                    if (_droneCamera != null) _droneCamera.fieldOfView = _fov;
                }
                GUILayout.EndHorizontal();

                // Roll angle slider
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>Camera Roll:</b> {_roll:F1}°", GUILayout.Width(170));
                _roll = GUILayout.HorizontalSlider(_roll, -45f, 45f);
                if (GUILayout.Button("Reset", GUILayout.Width(50))) _roll = 0f;
                GUILayout.EndHorizontal();

                // Flight Speed
                GUILayout.BeginHorizontal();
                GUILayout.Label($"<b>Drone Speed:</b> {_moveSpeed:F1} m/s", GUILayout.Width(170));
                _moveSpeed = GUILayout.HorizontalSlider(_moveSpeed, 1.0f, 25.0f);
                GUILayout.EndHorizontal();

                GUILayout.Space(6);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("[Hide HUD / Tab]", GUILayout.Height(24))) IsUiHidden = true;
                if (GUILayout.Button("[Resume Drive / F9]", GUILayout.Height(24))) TogglePhotoMode();
                GUILayout.EndHorizontal();
            }
            finally
            {
                GUILayout.EndArea();
            }
        }
    }
}

