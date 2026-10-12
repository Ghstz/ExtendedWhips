# Changelog

All notable changes to the **ExtendedWhips** mod will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.1.3] - 2026-10-12

### Visual Alignment, Wheel Classification & HDRP Material Fixes
Comprehensive physics and rendering hotfix resolving missing pink particle textures, frozen wheel rims, vehicle mesh offsets, drift gyro over-correction, and text field errors.

#### Fixed
- **Pink Missing Textures (HDRP Compatibility):** Dynamic particle emitters in `CustomSurfaceFX.cs` (burnout smoke & dirt roostertails), `CustomVehicleExhaust.cs` (exhaust smoke & afterfire flames), and `CustomVehicleNitrous.cs` (CO2 purge jets) now dynamically sample existing scene particle materials or initialize with transparent `HDRP/Lit` materials instead of leaving `ParticleSystemRenderer.sharedMaterial` unassigned.
- **Dynamic Skidmark Shaders:** `DynamicSkidmarks.cs` now clones valid HDRP particle shaders instead of querying unsupported Built-In pipeline shaders (`Sprites/Default` / `Standard`).
- **Wheel Hierarchy & Quadrant Classification:** In `GlbVehicleLoader.cs`, wheel and caliper detection now traverses GLTF visual parent hierarchies. All detected wheels and calipers are geometrically partitioned into `FrontLeft`, `FrontRight`, `RearLeft`, and `RearRight` relative to vehicle-aligned bounding midpoints, resolving the `FL: 0` bug on the Cuda, Blazer, Sierra HD, and Raptor R.
- **Wheel Rotation & Spin Decoupling:** `VehicleReskinManager.UpdateWheelQuadrant` now preserves GLTF rest rotations (`steerRot * restRot * spinRot`) and inverts spin direction on right-side wheels, eliminating wobbling and gimbal lock.
- **Model Offsets & Alignment:**
  - GMC Sierra 2500HD: Authoring Y-rotation corrected from `180°` to `28.68°` with centered root offset (`X: 2.37m`, `Z: -0.37m`), perfectly aligning the body with the chassis colliders.
  - 1978 Chevrolet K5 Blazer: Lowered `heightOffset` from `0.49m` to `0.10m`, eliminating suspension gap.
- **Drift Gyro Damping:** In `DriftAssistSystem.cs`, counter-steer torque is now damped by vehicle yaw angular velocity, preventing violent pendulum snap-oversteer and uncontrolled spinouts.
- **IMGUI TextEditor Guard:** Wrapped `GUILayout.TextField` for tune share codes in a guarded block, gracefully preventing native Unity exceptions caused by stripped IL2CPP `TextEditor` methods.

---

## [2.1.2] - 2026-10-12

### Vehicle Spawn Crash Fix & Audio Fault-Tolerance
Hotfix addressing an unhandled native access violation (`0xC0000005` in `coreclr.dll`) when spawning or reskinning custom vehicles.

#### Fixed
- **Stripped `AudioLowPassFilter` Removal:** Removed `AddComponent<AudioLowPassFilter>()` from `InCabinStereo.cs`. Construction Simulator's IL2CPP build stripped unused DSP filter components from `GameAssembly.dll`, which caused an immediate segfault when instantiated at runtime. Spatial in-cabin vs. exterior audio is now realistically simulated via volume and 3D spatial blend lerping.
- **Fault-Tolerant Procedural AudioClips:** Wrapped synthetic `AudioClip.Create` and `SetData` generators across `CustomVehicleAudio`, `CustomVehicleExhaust`, `CustomVehicleNitrous`, `CustomEmergencyLighting`, `InCabinStereo`, and `EvPowertrainSystem` in null/validity verification (`clip.samples > 0`), gracefully returning null instead of throwing native errors.
- **Safe Audio Playback in EV Powertrain:** Removed unconditional `_evAudioSource.Play()` at spawn in `EvPowertrainSystem.cs`. Audio playback is now gated by active vehicle speed and verified clip handles, preventing FMOD null pointer dereferences.
- **Isolated GLB Effect Initialization:** Wrapped each individual effect subsystem initializer in `GlbVehicleLoader.LoadGlbModel` (`Lights`, `Exhaust`, `Audio`, `Nitrous`, `SurfaceFX`, `EmergencyLighting`, `Stereo`, `EvAudio`) in isolated try-catch boundaries, ensuring that any single subsystem failure will never abort a vehicle spawn or crash the game.

---

## [2.1.1] - 2026-10-12

### Performance, GC Optimization & Stability Audit
Comprehensive audit and hardening pass across all vehicle physics hooks, custom particle FX, UI event pumping, and IL2CPP interop.

#### Fixed
- Launch control holds its RPM via the engine's own limiter (`LaunchControlSystem.RpmLimitOverride`, applied by the tuner) instead of writing the throttle input channel.
- Hotkeys (`F4/F5/F6/F8`, `PgUp/PgDn`) and the photo-mode camera run from a once-per-frame tick on the IMGUI host, independent of the physics step and working while time is frozen (F9 can always exit). `J`/`H`/`L` are latched and consumed by physics-side code.
- Mud now visibly tints/dulls the body (throttled material writes); paint changes re-apply it, wash clears it.
- Wear no longer registers false hits on first frame / vehicle swap, counts each collision once, and sets URP `_Smoothness`.
- Skidmark setup tolerates a missing shader gracefully.
- Nitrous torque boost now works under the per-frame tuner (exposed as `CustomVehicleNitrous.TorqueBoost`, applied in `ApplyTuningToInstance`); previously the engine write was overwritten every frame (~+1% effective).
- Gyro drift assist no longer applies full counter-torque while reversing.
- Launch control, EV drive/regen and gyro torque are normalised per physics step (FPS-independent, zero while photo mode freezes time).
- Drag benchmark longitudinal G now uses real acceleration instead of velocity/dt.
- Burnout smoke / roostertail emission and supercharger whine are framerate-independent.
- Gameplay hotkeys (`N`, `LeftShift`, `Space`/`W`, `J`, `H`) ignore input while a text field has focus or Photo Mode is active.
- GameAssembly hotpatch is now written with a single atomic 8-byte compare-exchange and flushes the instruction cache.
- Nitrous FOV punch no longer touches `Camera.main` while idle.
- Plugin and assembly versions synced to 2.1.1.

#### Performance
- Cached `VPWheelCollider[]`, `WheelCollider[]`, `Rigidbody`, and the first-person camera behaviour (previously re-queried every frame).
- Replaced 29 silent `catch { }` blocks with a log-once helper (`Plugin.LogOnce`).

#### Docs
- `FixIl2Cpp` is documented as an interop inspector (takes the interop path as an argument); aero airbrake thresholds match code (25% brake, 28°).

---

## [2.1.0] - 2026-10-12

### The Pro Driver & Logistics Expansion
This update introduces heavy equipment transport escort lighting, 2-step launch control, dynamic asphalt skid marks, cinematic drone photo mode, Formula Drift 70° angle geometry, in-cabin Bluetooth stereo with acoustic filtering, 1,500 HP cyber EV swaps, controller haptics, shareable Forza tuning codes, and vehicle scratch/wear detailing.

#### Added
- **Heavy Transport Escort Strobes & Sirens (`CustomEmergencyLighting.cs`):**
  - High-intensity amber roof lightbars and grille flashers for trucks (`[J]`).
  - Red & blue police/pace car strobes for track and sports cars.
  - Electronic dual-tone acoustic wail & yelp siren (`[H]`).
- **2-Step Launch Control & Anti-Lag Flame Spitter (`LaunchControlSystem.cs`):**
  - 4,500 RPM launch limiter when holding handbrake (`[Space]`) and full throttle at 0 mph.
  - Rapid-fire backfire popping audio and staccato blue/orange exhaust fireballs.
  - Slingshot launch forward boost upon handbrake release.
- **Dynamic Black Rubber Skid Marks (`DynamicSkidmarks.cs`):**
  - Persistent tire rubber trails laid down on asphalt during burnouts, high-angle drifts, and locked brakes.
- **Cinematic Photo Mode & 6-DOF Drone Freecam (`PhotoModeFreecam.cs`):**
  - Physics and time freeze toggled with `[F9]`.
  - Full 6-DOF drone movement (`WASD`, `QE` altitude, right-mouse look, `ZX` roll).
  - Configurable focal length / FOV (15mm to 110mm), roll tilt, and `[H]` UI hide for clean 4K captures.
- **Formula Drift 70° Steering Angle Kit & Gyro Assist (`DriftAssistSystem.cs`):**
  - Wisefab 68°–70° extreme steering lock toggle in Tab 4.
  - Gyro counter-steer torque stabilization preventing spinouts during deep reverse entries.
- **In-Cabin Bluetooth Stereo & Spatial Filtering (`InCabinStereo.cs`):**
  - Local music playback from `BepInEx/plugins/ExtendedWhips/Music/` with procedural synthwave backup.
  - Spatial low-pass filter: 22,000 Hz hi-fi in cockpit view vs 1,600 Hz muffled interior bleed in exterior view.
- **Instant 1,500 HP Cyber EV Powertrain Swap (`EvPowertrainSystem.cs`):**
  - Instant flat 1,500 Nm torque from 0 RPM in Tab 1.
  - Cyber electric inverter whine synthesizer scaling with speed.
  - One-pedal regenerative motor braking when off throttle.
- **Controller Haptic Vibration & Feedback (`ControllerHaptics.cs`):**
  - Engine redline vibration and tire break-away slip haptics for gamepads.
- **Shareable Forza-Style Tuning Codes (`TuningPresetCodeSystem.cs`):**
  - 1-Click `WHIP-XXXX-XXXX` code generator and importer in Tab 7.
- **Visual Vehicle Wear, Scratches & Quick-Repair (`VehicleWearSystem.cs`):**
  - Collision impact detection accumulating paint scuffs and dulling metallic clear-coat reflections.
  - `[Quick Repair & Detail]` button in Tab 7 restoring factory mirror finish.

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

