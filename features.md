# ExtendedWhips - Comprehensive Feature Specification

**ExtendedWhips** transforms *Construction Simulator (2022)* into a high-performance vehicle sandbox with full Forza Motorsport/Horizon-tier tuning and a native 3D GLB vehicle swapping engine running directly inside Unity's IL2CPP runtime via BepInEx 6.

---

## Table of Contents
1. [Forza-Level In-Game Tuning Suite](#1-forza-level-in-game-tuning-suite)
2. [Runtime 3D GLB Vehicle Reskinning Engine](#2-runtime-3d-glb-vehicle-reskinning-engine)
3. [Procedural Engine Audio Synthesizer](#3-procedural-engine-audio-synthesizer)
4. [Nitrous Oxide (NOS) Injection & CO2 Purge](#4-nitrous-oxide-nos-injection--co2-purge)
5. [Automated Drag Telemetry & Benchmark System](#5-automated-drag-telemetry--benchmark-system)
6. [Dynamic Tire Smoke & Terrain Roostertails](#6-dynamic-tire-smoke--terrain-roostertails)
7. [Heavy Towing & Equipment Hauling Mode](#7-heavy-towing--equipment-hauling-mode)
8. [Cockpit First-Person Sync & Sweeping Gauges](#8-cockpit-first-person-sync--sweeping-gauges)
9. [Active Aerodynamics & High-Speed Airbrake](#9-active-aerodynamics--high-speed-airbrake)
10. [In-Game Paint Studio & Iconic Liveries](#10-in-game-paint-studio--iconic-liveries)
11. [On-The-Fly Air Ride Hydraulics (Stance)](#11-on-the-fly-air-ride-hydraulics-stance)
12. [Dynamic Construction Mud & Pressure Wash Service](#12-dynamic-construction-mud--pressure-wash-service)
13. [Interactive Showroom & Inspection Mode](#13-interactive-showroom--inspection-mode)
14. [Motorcycle Counter-Steering Lean Physics](#14-motorcycle-counter-steering-lean-physics)
15. [Supported Custom Vehicle Roster](#15-supported-custom-vehicle-roster)

---

## 1. Forza-Level In-Game Tuning Suite

Accessible at any time while driving via **[F4]** or **[Insert]**. Organized into 7 specialized engineering tabs:

### Tab 1: Engine & Powertrain
* **Torque Multiplier:** `0.5x` to `10.0x` slider with real-time horsepower and Newton-meter readout.
* **Max Engine RPM:** Configurable from `2,500 RPM` (heavy diesel) up to `14,000 RPM` (superbike screamers).
* **RPM Limiter Softness:** Controls bounce/cut behavior at the redline.
* **Flywheel Inertia:** Dial in featherweight racing flywheels for lightning-fast revs or heavy mass for crawler torque retention.
* **Drivetrain Layout Swapper:**
  * **RWD:** Pure rear-wheel drive burnout and drift dynamics.
  * **FWD:** Front-wheel drive economy configuration.
  * **AWD:** Full All-Wheel Drive with custom Front/Rear torque split (`0%` to `100%`).
* **Differential Lock Percentage:** Open, 1.5-way, 2-way, or 100% locked spool.

### Tab 2: Transmission & Gearing
* **Final Drive Ratio:** `2.00` (high top speed cruise) to `6.50` (short-ratio drag launch acceleration).
* **Gear Ratios (Gears 1 through 8):** Full individual gear step tuning.
* **Shift Delay:** Instant dual-clutch transmission (DCT) shifts down to `0.02s` or authentic manual throws (`0.45s`).

### Tab 3: Suspension & Alignment
* **Ride Height Offset:** `-0.15m` (slammed track stance) to `+0.45m` (extreme overland lift kit).
* **Spring Rate (Stiffness):** Independent front and rear spring stiffness.
* **Damper Rebound & Bump Rate:** Eliminates body bounce over construction potholes or curbs.
* **Anti-Roll Bars (ARB):** Front/Rear torsional stiffness sliders to eradicate body roll through high-G turns.

### Tab 4: Tires & Compound
* **Compound Selection:** Street Radial, Semi-Slick Track, Drag Radial, Mud-Terrain (M/T), All-Terrain (A/T), Drift Hard Compound.
* **Forward Grip Factor:** `0.5x` to `3.0x` longitudinal traction.
* **Sidewall Lateral Stiffness:** Sharp cornering turn-in or progressive break-away angles.
* **Tire Width & Rolling Radius Scale:** Visually and physically scales contact patches.

### Tab 5: Brakes & Downforce
* **Braking Torque:** `1,000 Nm` to `25,000 Nm` stopping power with zero brake fade.
* **Brake Bias:** `30% Front / 70% Rear` (drift handbrake feel) to `80% Front / 20% Rear` (straight-line stability).
* **Downforce Coefficient:** Aerodynamic suction scaling with square of velocity ($F_{down} \propto v^2$).

### Tab 6: Live Telemetry & Drag Benchmark
* Real-time G-Force meters (Lateral cornering G, Longitudinal acceleration/braking G).
* Suspension travel bar graphs displaying millimeters of compression per corner.
* Wheel slip percentage and wheel lockup detectors.
* Automated Drag Telemetry Stopwatch (0–60, 0–100, 1/4 mile, 60–0 braking).

### Tab 7: Whips & Customization Studio
* 1-Click GLB vehicle body swap selector.
* Stance air ride selector.
* Livery selector.
* Tow mode toggle.
* Showroom inspection toggles (open/close hood, doors, trunk).
* Pressure wash vehicle service.

---

## 2. Runtime 3D GLB Vehicle Reskinning Engine

Unlike traditional model replacements that only change visual meshes:
* **Native VPWheelCollider Parenting:** Custom wheel meshes hook directly onto `VPWheelCollider.wheelTransform`. The custom wheels physically compress up into fenders over rocks, gravel mounds, and potholes.
* **Separated Calipers vs. Spinning Rims:** Brake calipers remain bolted to the steering knuckle, turning left and right with steering inputs while the rims, spokes, and rubber spin through them.
* **Automatic Dimension & Contact Normalization:** Custom vehicles are automatically analyzed at load time to calculate track width, wheelbase, and hub vertical offsets, ensuring tires sit flush on the ground with zero clipping.

---

## 3. Procedural Engine Audio Synthesizer

Engine audio dynamically synthesizes real-time acoustic layers directly tied to the VehiclePhysics Pro engine data channel:
* **Supercharger Whine (1971 Cuda & 2023 Raptor R):** Real-time sine-wave frequency modulator scaling with engine RPM ($f = 350\text{ Hz} \to 3,200\text{ Hz}$) and throttle opening.
* **Twin-Turbo Spool & Blow-Off Valve (R34 GT-R & M4 GT3):** Turbo hiss builds under sustained boost; abruptly lifting off throttle triggers an authentic atmospheric blow-off valve sneeze/flutter.
* **High-RPM Screamer (Kawasaki Ninja H2 & BMW S1000RR):** Smoothly pitch-shifts the engine sound up to 14,000 RPM for authentic literbike acoustics.
* **Diesel Engine Compression Brake / Jake Brake (Sierra 2500HD):** Low-frequency rhythmic compression chuffing during decelerations in gear.

---

## 4. Nitrous Oxide (NOS) Injection & CO2 Purge

* **CO2 Fender Purge (`[N]`):** Fires dual high-pressure steam plumes out of the front hood cowl / fender louvers with authentic hiss sound.
* **Nitro Boost (`[LeftShift]`):** Injects +50% instant torque, generates blue exhaust flame plumes, punches camera FOV forward by 10%, and depletes bottle PSI.
* **HUD Pressure Gauge:** Top-left telemetry display shows current bottle pressure (`0–1,000 PSI`) with live status badge.

---

## 5. Automated Drag Telemetry & Benchmark System

Located in **Tab 6 [TELEMETRY]**:
* Arms automatically when vehicle speed drops below 0.5 km/h.
* Triggers timer the exact millisecond wheel velocity is detected.
* Tracks:
  * **0–60 mph (0–100 km/h) Elapsed Time**
  * **0–100 mph (0–160 km/h) Elapsed Time**
  * **1/4 Mile Elapsed Time & Trap Speed (mph and km/h)**
  * **60–0 mph Emergency Braking Distance (meters)**
  * **Peak Acceleration G & Peak Lateral Cornering G**

---

## 6. Dynamic Tire Smoke & Terrain Roostertails

* **Burnout Smoke:** Detects high wheel angular velocity combined with vehicle velocity under 20 km/h, generating thick tire smoke billowing from the wheel arches.
* **Off-Road Roostertails:** Dynamically sprays dirt, mud, and gravel pellets high behind drive wheels when ripping across construction dirt, sand, and gravel pits.

---

## 7. Heavy Towing & Equipment Hauling Mode

* Injects **+120% low-end torque** below 2,000 RPM.
* Locks differential 100% to conquer loose gravel launch ramps.
* Applies realistic rear tongue-weight squat to the pickup chassis when hauling 30-ton lowboy equipment trailers loaded with excavators or cranes.

---

## 8. Cockpit First-Person Sync & Sweeping Gauges

* **Dynamic Eye-Point:** Pressing **[C]** automatically positions the camera inside the custom vehicle:
  * Muscle & Sports Cars: Low, aggressive racing seating looking over the shaker hood / hood scoop.
  * Trucks: Elevated commanding viewpoint over wide hoods.
  * Motorcycles: Tucked handlebar / windscreen visor view.
* **Sweeping Gauge Needles:** Speedometer needle sweeps 0–240 km/h and tachometer needle sweeps 0–8,000 RPM in real time.
* **Interior Steering Wheel Sync:** Rotates in the driver's hands matching real-time steering angle.

---

## 9. Active Aerodynamics & High-Speed Airbrake

* Detected rear wing structures (e.g. BMW M4 GT3, Nissan Skyline R34 GT-R) automatically tilt up by **+38°** when hard braking (>50% brake input) at speeds above **75 km/h**, acting as an airbrake.

---

## 10. In-Game Paint Studio & Iconic Liveries

### Custom Paint Finishes
* **Shades:** OEM palettes including Nissan Bayside Blue, Plymouth Tor-Red & Plum Crazy, BMW Alpine White & Isle of Man Green, Ford Code Orange.
* **Surface Finishes:** Factory Gloss, Satin / Matte, Metallic Flake, and Raw Carbon Fiber.
* **Window Tint:** Clear Glass to 5% Limo Privacy Tint.

### Iconic Liveries
* **Nissan R34 GT-R:** *2 Fast 2 Furious* Silver base with dual blue racing stripes.
* **1971 Plymouth Cuda:** *426 HEMI Billboard* rear quarter-panel graphics with satin black hood.
* **BMW M4 GT3:** Official *M Motorsport* Tricolor racing livery.
* **Ford F-150 Raptor R:** *Fox Factory Baja Trophy* digital camo wrap.

---

## 11. On-The-Fly Air Ride Hydraulics (Stance)

Instant 3-way height selector in Tab 7:
1. **[Slammed / Stance]:** Drops the chassis to within an inch of the asphalt for car meets and construction site photo mode.
2. **[Street / Track]:** Balanced center-of-gravity for high-speed grip and daily driving.
3. **[Overland / Lifted]:** Raises clearance by +0.12m for deep mud, trenches, and rock crawling.

---

## 12. Dynamic Construction Mud & Pressure Wash Service

* Dynamic mud and dirt layers accumulate on body paint as you drive across muddy construction sites.
* Click **[Pressure Wash Vehicle]** in Tab 7 to instantly restore pristine showroom clear-coat gloss.

---

## 13. Interactive Showroom & Inspection Mode

* Separate toggles in Tab 7 allow opening and closing individual vehicle parts:
  * **Pop Hood:** Inspect detailed 3D engine bays (426 HEMI V8, twin-turbo RB26DETT, BMW M TwinPower Turbo, Raptor R 5.2L Supercharged V8).
  * **Open Doors:** For photo mode, walkarounds, or job site staging.
  * **Open Trunk / Tailgate:** Lower truck tailgates or open trunks.

---

## 14. Motorcycle Counter-Steering Lean Physics

* Dedicated physics calculation for the **Kawasaki Ninja H2** and **BMW S1000RR**:
  * Body tilts into corners up to **42°** based on lateral cornering acceleration ($G_{lat} = v^2 / R$).
  * Automatic kickstand / vertical stabilization when coming to a complete stop (0 mph).

---

## 15. Supported Custom Vehicle Roster

| Vehicle Name | Category | Engine Spec | Signature Features |
| :--- | :--- | :--- | :--- |
| **1971 Plymouth Cuda** | Muscle Car | 426 HEMI V8 | Supercharger whine, shaker hood, HEMI billboard livery |
| **1978 Chevrolet K5 Blazer** | Classic 4x4 | 350 Small Block V8 | Overland lift stance, mud roostertails, heavy tow hitch |
| **1999 Nissan Skyline GT-R R34** | Sports / JDM | Twin-Turbo RB26DETT | BOV flutter, F&F blue stripes, active airbrake, AWD split |
| **2020 Chevrolet Silverado 1500** | Modern Pickup | 6.2L EcoTec3 V8 | Lift kit, trailer coupling, street & overland presets |
| **2022 BMW M4 GT3** | Track Car | S58 Twin-Turbo Inline-6 | Active aero airbrake, M Motorsport livery, slick tires |
| **2022 GMC Sierra 1500 AT4X** | Off-Road Truck | 6.2L V8 | Multimatic DSSV damper tuning, 35" tires, tow mode |
| **2023 Ford F-150 Raptor R** | Super Truck | 5.2L Supercharged V8 | Fox Baja livery, supercharger scream, high-speed desert run |
| **2024 GMC Sierra 2500HD** | Heavy Duty Truck | 6.6L Duramax Turbo-Diesel | Jake brake, heavy equipment towing, dual rear wheels |
| **2024 Chevrolet Silverado 2500HD**| Heavy Duty Truck | 6.6L Duramax Turbo-Diesel | Heavy equipment towing, locked diff, work truck spec |
| **2022 Kawasaki Ninja H2** | Hypersport Bike | Supercharged 998cc I4 | 14,000 RPM scream, 42° lean physics, visor cockpit camera |
| **2022 BMW S1000RR** | Superbike | 999cc ShiftCam I4 | MotoGP quickshifter, dynamic counter-steering, race cockpit |
