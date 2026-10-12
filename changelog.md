# Changelog

All notable changes to the **ExtendedWhips** mod will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.0.0] - 2026-10-12

### The Ultimate Simulation Update
This major release transforms ExtendedWhips into a complete automotive simulation framework, introducing procedural audio synthesis, nitrous systems, automated drag telemetry, active aerodynamics, iconic liveries, dynamic mud accumulation, and heavy hauling capabilities.

#### Added
- **Procedural Engine Audio Synthesizer (`CustomVehicleAudio.cs`):**
  - Supercharger gear whine for 1971 Cuda and 2023 Raptor R with real-time RPM scaling.
  - Twin-turbo spool and atmospheric Blow-Off Valve (BOV) flutter on throttle lift for Nissan R34 GT-R and BMW M4 GT3.
  - High-rev engine pitch-shifter up to 14,000 RPM for Kawasaki Ninja H2 and BMW S1000RR superbikes.
  - Compression release engine brake ("Jake Brake") for GMC Sierra 2500HD during in-gear deceleration.
- **Nitrous Oxide (NOS) System (`CustomVehicleNitrous.cs`):**
  - Dedicated CO2 fender purge (`[N]`) shooting dual high-pressure steam plumes from cowl vents.
  - Nitro torque boost (`[LeftShift]`) providing instant +50% torque surge, exhaust afterfire blue flames, and 10% camera FOV punch.
  - Real-time bottle PSI gauge on the live telemetry HUD overlay.
- **Forza-Style Drag Telemetry Benchmark (`DragTelemetryBenchmark.cs`):**
  - Self-arming drag stopwatch tracking 0–60 mph, 0–100 mph, 1/4 mile ET and trap speed, 60–0 mph emergency braking distance, and peak G-forces.
  - Integration into Tab 6 (`[TELEMETRY]`) in the in-game tuner window.
- **Dynamic Surface FX (`CustomSurfaceFX.cs`):**
  - High-density tire rubber smoke clouds generated during burnouts and high-slip maneuvers below 20 km/h.
  - Dynamic terrain roostertails spraying dirt and gravel pellets behind rear wheels on loose construction surfaces.
- **Workhorse Heavy Towing Mode:**
  - One-click toggle in Tab 7 providing +120% low-end torque, locked differential, and hitch suspension squat for hauling 30-ton construction equipment trailers.
- **Working Cockpit Gauges & Driver Synchronization (`CockpitGaugeSync.cs`):**
  - Real-time sweeping speedometer (0–240 km/h) and tachometer (0–8,000 RPM) needles.
  - Synchronized interior steering wheel rotation matching driver input angle.
- **Active Aerodynamics & Deployable Airbrakes:**
  - Automatic +38° spoiler/wing airbrake pitch under heavy braking (>75 km/h) on track-focused cars (BMW M4 GT3, R34 GT-R).
- **Iconic Livery Presets in Paint Studio:**
  - *2 Fast 2 Furious* Silver base with dual blue racing stripes (Nissan R34).
  - *426 HEMI Billboard* quarter-panel graphics with satin black shaker hood (Plymouth Cuda).
  - *BMW M Motorsport* Cyan/Blue/Red tricolor racing graphics (BMW M4 GT3).
  - *Fox Factory Baja Trophy* digital camo wrap (Ford Raptor R).
- **On-The-Fly Air Ride Stance Hydraulics:**
  - 3-way height selector: `[Slammed / Stance]`, `[Street / Track]`, and `[Overland / Lifted]`.
- **Dynamic Construction Mud & Detailing Service (`MudAccumulationSystem.cs`):**
  - Realistic mud and dirt accumulation on vehicle paint during off-road driving.
  - One-click showroom pressure wash detailing service to restore paint finish.

---

## [1.4.0] - 2026-10-12

### Physics Realism & Customization Update

#### Added
- **Direct VPWheelCollider Parenting:**
  - Replaced estimated wheel rotation math with direct parenting to `VPWheelCollider.wheelTransform`.
  - Wheels physically compress and rebound through wheel arches over curbs, dirt mounds, and obstacles.
- **Separated Brake Calipers vs. Spinning Rims:**
  - Calipers remain fixed to the steering knuckle while rims and tires rotate freely.
- **Motorcycle Lean Physics:**
  - Dynamic lateral-G counter-steering lean angles up to 42° for Ninja H2 and S1000RR.
  - Automatic kickstand stabilization when stopped at 0 mph.
- **Cockpit First-Person Camera Alignment:**
  - Dynamic eye-line positioning inside custom vehicle cabins when pressing `[C]`.
- **Full Lighting & Emissive Integration:**
  - Custom headlight beam projectors hooked into `[L]`.
  - Taillight bloom amplification during braking and reverse illumination.
- **In-Game Paint Shop (Tab 7):**
  - Real-time palette with OEM shades (Bayside Blue, Plum Crazy, Tor-Red, Alpine White, Code Orange).
  - Gloss, Matte, Metallic, and Raw Carbon Fiber finish options.
  - Window tint darkness slider (0% to 95%).
- **Interactive Showroom / Inspection Mode:**
  - Toggles for popping hoods, opening driver/passenger doors, and lowering tailgates.

---

## [1.3.0] - 2026-10-12

### Runtime GLB Vehicle Reskinning Engine

#### Added
- Direct runtime loading of `.glb` models without needing Unity AssetBundles.
- Support for 11 distinct custom vehicles across Muscle, Classic 4x4, JDM Sports, Track, Heavy Duty, and Superbike categories.
- Automatic dimension, wheelbase, and wheel hub alignment calculations.
- Seamless body swapping over existing base game vehicles.

---

## [1.2.0] - 2026-10-11

### Forza-Level Vehicle Tuning Suite

#### Added
- 7-tab in-game vehicle engineering GUI (`ExtendedWhipsUI.cs`) toggled with `[F4]` / `[Insert]`.
- Live on-screen telemetry overlay (`[F6]`) showing speed, gear, RPM bar, torque, horsepower, and steering lock.
- Pre-configured tuning presets: `Factory Stock`, `Street Performance`, `Offroad Lifted`, `Drift Spec`, `Rally Cross`, and `Hyper Overkill`.
- Instant factory reset shortcut (`[F8]`).

---

## [1.1.0] - 2026-10-11

### IL2CPP Stability & Hotpatching Engine

#### Fixed
- Replaced destructive byte patching with a thread-safe 16-byte trampoline hotpatch (`FixIl2Cpp`), preserving game execution stability and preventing crash-to-desktop errors.
- Resolved IL2CPP memory protection violations on Windows.

---

## [1.0.0] - 2026-10-10

### Initial Release
- Basic vehicle tuning framework for *Construction Simulator (2022)* using BepInEx 6 IL2CPP.
- Basic torque, speed limiter, and suspension height modifiers.
