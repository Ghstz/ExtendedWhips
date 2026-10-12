# ExtendedWhips

[![Construction Simulator 2022](https://img.shields.io/badge/Game-Construction%20Simulator%20(2022)-blue.svg)](https://store.steampowered.com/app/1273400/Construction_Simulator/)
[![BepInEx 6 IL2CPP](https://img.shields.io/badge/Framework-BepInEx%206%20IL2CPP-purple.svg)](https://github.com/BepInEx/BepInEx)
[![Target .NET](https://img.shields.io/badge/.NET-6.0-green.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **The ultimate vehicle modification framework for Construction Simulator (2022).**  
> Bring iconic muscle cars, JDM legends, GT3 race cars, trophy trucks, and superbikes into Construction Simulator with a complete **Forza-level vehicle tuning suite**, **runtime 3D GLB mesh swapping**, **procedural engine audio synthesis**, **nitrous oxide purge & boost**, and **real-time drag telemetry**.

---

## 📸 Highlights & Capabilities

* 🏎️ **Forza-Level 7-Tab Tuning Suite:** Modify engine torque, gearing, springs, dampers, sway bars, tire compound, brake bias, downforce, and drivetrain layouts (RWD/FWD/AWD with torque split) in real time.
* 📦 **Direct 3D GLB Model Swapping:** Reskins base game vehicles with custom high-detail 3D models at runtime—no Unity AssetBundle compiler required.
* 🛞 **Native Wheel & Suspension Physics:** Custom rims and tires are parented directly to `VPWheelCollider.wheelTransform`, compressing and rebounding through wheel wells with separated stationary brake calipers.
* 🔊 **Procedural Engine Audio Synthesizer:** Authentic supercharger gear whine (Cuda, Raptor R), twin-turbo spool & blow-off valve flutter (R34 GT-R, M4 GT3), 14,000 RPM superbike screamer (Ninja H2, S1000RR), and diesel Jake brake (Sierra 2500HD).
* 💨 **Nitrous Oxide (NOS) System:** Dual fender CO2 steam purge (`[N]`), nitrous torque boost (`[LeftShift]`), exhaust afterfire flames, camera FOV acceleration punch, and HUD bottle PSI gauge.
* ⏱️ **Drag Telemetry Benchmark:** Automated self-arming stopwatch tracking 0–60 mph, 0–100 mph, 1/4 mile ET & trap speed, 60–0 braking distance, and peak lateral/longitudinal G-forces.
* 🚜 **Heavy Equipment Hauling Mode:** Injects +120% low-end torque, locks differential 100%, and adds dynamic hitch tongue-weight squat for pulling heavy lowboy trailers loaded with excavators and cranes.
* 🏍️ **Motorcycle Counter-Steering Physics:** Realistic lateral-G chassis lean (up to 42°) and automatic kickstand stabilization for superbikes.
* 🎨 **Paint Studio & Iconic Liveries:** OEM colors, gloss/matte/carbon finishes, window tint, and iconic livery presets (*2 Fast 2 Furious* R34, *426 HEMI Billboard*, *BMW M Motorsport*, *Fox Baja Trophy*).
* 🪟 **Active Aero & Interactive Cockpit:** Deployable high-speed airbrake, sweeping gauge needles (speedometer/tachometer), animated steering wheel, and openable hood/doors/tailgate.
* 🧼 **Dynamic Mud & Pressure Wash:** Vehicles accumulate realistic job-site mud that can be detailed back to showroom shine with one click.

---

## 🎮 Controls & Shortcuts

| Hotkey | Action | Notes |
| :---: | :--- | :--- |
| **`[F4]`** or **`[Insert]`** | **Toggle Vehicle Tuning Menu** | Opens the full 7-tab configuration suite |
| **`[F6]`** | **Toggle Live Telemetry HUD** | Shows speed, gear, RPM bar, torque, and NOS PSI |
| **`[F8]`** | **Reset Vehicle to Factory Stock** | Instantly restores default physics and specs |
| **`[N]`** | **Nitrous CO2 Fender Purge** | Shoots twin high-pressure steam plumes from cowl |
| **`[LeftShift]`** | **Nitrous Oxide Boost** | Instant +50% torque surge + blue flame exhaust |
| **`[L]`** | **Vehicle Headlights / High Beams** | Projector light beams and rear taillight bloom |
| **`[C]`** | **Cycle Camera / Cockpit View** | Aligns first-person eye-line inside custom cabin |

---

## 🏎️ Vehicle Roster

ExtendedWhips comes pre-configured for 11 iconic custom vehicle models located in `Vehicles/`:

1. **1971 Plymouth Cuda** — 426 HEMI V8, supercharger scream, shaker hood, classic muscle stance.
2. **1978 Chevrolet K5 Blazer** — 350 Small Block V8, overland lift, off-road mud roostertails, heavy hitch.
3. **1999 Nissan Skyline GT-R R34** — Twin-turbo RB26DETT, BOV flutter, active aero airbrake, F&F livery.
4. **2020 Chevrolet Silverado 1500** — 6.2L EcoTec3 V8, modern daily workhorse, lift & stance presets.
5. **2022 BMW M4 GT3** — S58 Twin-Turbo Inline-6, track airbrake wing, M Motorsport livery, slick tires.
6. **2022 GMC Sierra 1500 AT4X** — 6.2L V8, Multimatic DSSV damper tuning, 35" tires, heavy tow mode.
7. **2023 Ford F-150 Raptor R** — 5.2L Supercharged V8, Fox Baja wrap, long-travel desert runner.
8. **2024 GMC Sierra 2500HD** — 6.6L Duramax Turbo-Diesel, diesel engine brake (Jake brake), dual rear wheels.
9. **2024 Chevrolet Silverado 2500HD** — 6.6L Duramax Turbo-Diesel, heavy lowboy equipment hauler.
10. **2022 Kawasaki Ninja H2** — Supercharged 998cc I4, 14,000 RPM screamer, 42° dynamic lean physics.
11. **2022 BMW S1000RR** — 999cc ShiftCam I4, counter-steering lean, visor cockpit camera.

---

## 🛠️ Installation & Setup

### Prerequisites
* **Construction Simulator (2022)** on Steam.
* [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (for building from source).

### Step 1: Install BepInEx 6 IL2CPP
Run the automated installer script via PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File "tools\Install-BepInEx.ps1"
```
> **Important:** Launch Construction Simulator once and close it after running this script so BepInEx can unhollow the game assemblies into `BepInEx/interop`.

### Step 2: Build & Deploy
Compile and deploy the mod directly into your game directory:
```powershell
powershell -ExecutionPolicy Bypass -File "tools\Build-And-Deploy.ps1" -Configuration Release
```

### Step 3: Vehicle Models
Ensure your `.glb` vehicle models are located in:
```
<Game Directory>\BepInEx\plugins\ExtendedWhips\Vehicles\
```
Jump into any pickup or vehicle in-game, press **[F4]**, open **Tab 7 [WHIPS]**, and select any whip!

---

## 📂 Project Structure

```
ExtendedWhips/
├── src/ExtendedWhips/
│   ├── Plugin.cs                    # BepInEx 6 IL2CPP mod entry point
│   ├── Cockpit/
│   │   └── CockpitGaugeSync.cs      # Sweeping speedometer & tachometer needles
│   ├── Effects/
│   │   ├── CustomVehicleAudio.cs    # Supercharger, BOV, scream, and Jake brake synthesis
│   │   ├── CustomVehicleNitrous.cs  # CO2 purge, torque boost, FOV warp, exhaust flames
│   │   ├── CustomSurfaceFX.cs       # Burnout smoke clouds & dirt/gravel roostertails
│   │   └── MudAccumulationSystem.cs # Dynamic mud accumulation & pressure wash detailing
│   ├── Hooks/
│   │   ├── VehicleTunerHook.cs      # VehiclePhysics Pro engine, diff & suspension hooks
│   │   └── VehicleReskinManager.cs  # Mesh swapper, active aero, stance & livery controller
│   ├── Models/
│   │   ├── GlbVehicleLoader.cs      # GLB importer, wheel parenting & dimension calculator
│   │   └── LoadedVehicleAsset.cs    # Runtime vehicle asset model & transform references
│   ├── Tuning/
│   │   ├── TuningData.cs            # Preset definitions, profiles & slider ranges
│   │   └── DragTelemetryBenchmark.cs# 0-60, 0-100, 1/4 mile, and G-force benchmark
│   └── UI/
│       └── ExtendedWhipsUI.cs       # 7-Tab Forza tuning window, HUD overlay & toast alerts
├── tools/
│   ├── Build-And-Deploy.ps1         # Compiles Release build and copies to ConSim plugins
│   ├── Install-BepInEx.ps1          # Automated BepInEx 6 IL2CPP environment setup
│   └── FixIl2Cpp/                   # Thread-safe 16-byte trampoline hotpatching engine
├── features.md                      # Comprehensive technical feature breakdown
├── changelog.md                     # Semantic versioning changelog
└── README.md                        # Project documentation
```

---

## 📖 Additional Documentation

* **[Full Features Specification](features.md)** — In-depth breakdown of every tuning slider, physics calculation, and simulation system.
* **[Changelog](changelog.md)** — Detailed history of all updates and releases.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
