# ExtendedWhips - Construction Simulator (2022) Vehicle & Tuning Mod

**ExtendedWhips** is a modding framework and toolset for **Construction Simulator (2022)** that allows you to:
1. **Tune any vehicle in-game:** Adjust engine torque, top speed limiter, suspension lift kits or slammed stance, spring stiffness, tire forward grip, drift slip factor, and toggle All-Wheel Drive (AWD).
2. **Add & Spawn custom civilian vehicles:** Import custom 3D pickup trucks, passenger cars, sports cars, and motorcycles via Unity 2020.3 AssetBundles.

---

## Project Structure

```
D:\VSC Projects\ExtendedWhips\
├── src\
│   └── ExtendedWhips\
│       ├── ExtendedWhips.csproj        # .NET 6.0 plugin project
│       ├── Plugin.cs                  # BepInEx 6 IL2CPP mod entry point
│       ├── UI\
│       │   └── TuningMenu.cs          # Interactive in-game GUI (F7)
│       ├── Tuning\
│       │   ├── TuningData.cs          # Vehicle tuning parameters data model
│       │   └── VehicleTuningManager.cs# Physics, suspension & wheel modifier
│       └── Spawning\
│           └── VehicleSpawner.cs      # External AssetBundle vehicle loader
├── tools\
│   ├── Install-BepInEx.ps1            # Automated BepInEx 6 IL2CPP installer
│   └── Build-And-Deploy.ps1           # Builds and deploys DLL to ConSim
├── unity\
│   └── VEHICLE_PIPELINE.md            # Guide for rigging & exporting 3D whips in Unity
└── README.md
```

---

## Quick Start (3 Steps)

### Step 1: Install BepInEx 6 into Construction Simulator
Run the included PowerShell installer script:
```powershell
powershell -ExecutionPolicy Bypass -File "D:\VSC Projects\ExtendedWhips\tools\Install-BepInEx.ps1"
```
> **Important:** After running this script, launch **Construction Simulator** once and close it. BepInEx will hook the game, unhollow the IL2CPP assemblies, and populate `BepInEx\interop`.

### Step 2: Build & Deploy the Mod
Compile and copy the mod directly into your game's plugins directory:
```powershell
powershell -ExecutionPolicy Bypass -File "D:\VSC Projects\ExtendedWhips\tools\Build-And-Deploy.ps1"
```

### Step 3: Jump In-Game
* Enter any vehicle (the pickup truck, flatbeds, or sports car).
* Press **[F7]** to open the **ExtendedWhips Tuner Menu**.
* Adjust sliders (Torque, Lift/Drop stance, Grip, Drift factor) or pick presets (**Street**, **Offroad Lifted**, **Drift Spec**).
* Press **[F8]** anytime to reset your active vehicle back to stock factory settings.

---

## Adding Custom Vehicles (Cars, Pickups, Motorcycles)
Follow the instructions in [`unity/VEHICLE_PIPELINE.md`](./unity/VEHICLE_PIPELINE.md) to package your 3D models into `.bundle` files using Unity 2020.3.
Drop your `.bundle` files into:
```
D:\SteamLibrary\steamapps\common\Construction Simulator\BepInEx\plugins\ExtendedWhips\Vehicles\
```
Open the in-game menu (**F7**) > **Spawner** tab to spawn your custom whips directly in front of you.

