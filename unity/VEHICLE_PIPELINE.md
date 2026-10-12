# ExtendedWhips - 3D Vehicle Creation & AssetBundle Pipeline

This guide explains how to prepare, rig, and export custom civilian vehicles (pickups, cars, motorcycles) for **Construction Simulator (2022)**.

---

## 1. Engine Compatibility
* **Target Unity Version:** `Unity 2020.3.49f1` (or any `2020.3.x` LTS release).
* **Asset Format:** Unity AssetBundle (`.bundle`) built for `StandaloneWindows64`.

---

## 2. 3D Model Preparation (Blender)

### Orientation & Scale
* Scale: `1.0` (Metric, meters).
* Forward axis: `+Z`
* Up axis: `+Y`

### Object Hierarchy
Separate your model into individual child meshes with correctly placed origins:

```
[Root Vehicle] (Origin at 0, 0, 0)
├── Body / Chassis (Cabin, frame, interior, glass)
├── SteeringWheel (Origin centered on the steering column pivot)
├── Wheel_FrontLeft (Origin centered in the wheel hub)
├── Wheel_FrontRight
├── Wheel_RearLeft
└── Wheel_RearRight
```

---

## 3. Unity Prefab Setup

1. **Import the FBX** into your Unity 2020.3 project.
2. Create a Prefab and add the following components:

### A. Root GameObject
* **Rigidbody**:
  * Mass: `1800` (for passenger car / pickup) or `250` (for motorcycle)
  * Drag: `0.05`
  * Angular Drag: `0.5`
  * Interpolate: `Interpolate`
* **Box / Mesh Colliders**:
  * Add primitive Box Colliders for the cabin, hood, and undercarriage. Avoid complex concave Mesh Colliders on moving bodies.

### B. Wheel Colliders
Create 4 empty child GameObjects named `WC_FrontLeft`, `WC_FrontRight`, etc.:
* Add component: `WheelCollider`
* Configure:
  * **Radius**: Match your 3D wheel radius (e.g., `0.35m`).
  * **Suspension Distance**: `0.2m`
  * **Spring**: `35000`
  * **Damper**: `4500`
  * **Target Position**: `0.5`

### C. Helper Anchor Transforms
Create empty GameObjects as reference points:
* `Camera_DriverView`: Placed at the driver head position.
* `Camera_ChaseView`: Placed 4m behind and 1.5m above the car.
* `Light_Headlight_L` / `Light_Headlight_R`
* `Exhaust_Point`: Position for particle exhaust effects.

---

## 4. Special Note: Motorcycles (2-Wheeled Whips)
Because ConSim's physics engine expects a stable base:
* Add two **invisible helper wheels** (outriggers) at the sides with zero friction and narrow suspension, OR
* Add a simple upright balancing torque script to the root Rigidbody.

---

## 5. Building the AssetBundle

1. Select your vehicle prefab in the Project tab.
2. In the Inspector, at the very bottom, set **AssetBundle** to: `mypickup` (or your vehicle name).
3. Create an Editor script `Assets/Editor/BuildAssetBundles.cs`:

```csharp
using UnityEditor;
using System.IO;

public class BuildAssetBundles
{
    [MenuItem("ExtendedWhips/Build AssetBundles")]
    static void BuildAllBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(
            assetBundleDirectory, 
            BuildAssetBundleOptions.None, 
            BuildTarget.StandaloneWindows64
        );
    }
}
```

4. Click **ExtendedWhips > Build AssetBundles** in Unity's top menu.
5. Take the resulting `mypickup.bundle` file and place it into:
   ```
   D:\SteamLibrary\steamapps\common\Construction Simulator\BepInEx\plugins\ExtendedWhips\Vehicles\
   ```
6. Launch the game, press **[F7]**, open the **Spawner** tab, and spawn your whip!

