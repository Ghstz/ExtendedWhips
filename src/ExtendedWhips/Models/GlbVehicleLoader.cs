using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;
using ExtendedWhips.Effects;
using ExtendedWhips.Tuning;
using GltfNode = SharpGLTF.Schema2.Node;

namespace ExtendedWhips.Models
{
    public static class GlbVehicleLoader
    {
        public static readonly List<GlbVehicleModel> Catalog = new()
        {
            // Classic & Sports Cars (flush with asphalt at Y = 0.00, forward facing)
            new GlbVehicleModel("cuda71", "1971 Plymouth Hemi Cuda", "plymouth_hemi_cuda_1971__www.vecarz.com.glb", 
                VehicleTypeCategory.MuscleCar, "426 Hemi V8 Classic American Muscle", 
                scale: 2.083f, heightOffset: 0.01f, rotY: 0.0f, wheelScale: 1.0f, preset: "StreetPerformance", 
                driverY: -0.38f, 
                cockpitCam: new Vector3(-0.38f, 1.05f, -0.15f),
                exhausts: new List<Vector3> { new Vector3(-0.55f, 0.28f, -2.55f), new Vector3(0.55f, 0.28f, -2.55f) }),

            new GlbVehicleModel("r34", "Nissan Skyline GT-R R34", "nissan_r34_brians_fast_furious__www.vecarz.com.glb", 
                VehicleTypeCategory.SportsCar, "Brian's Fast & Furious R34 GT-R Spec", 
                scale: 12.105f, heightOffset: 0.02f, rotY: 0.0f, wheelScale: 0.95f, preset: "DriftSpec", 
                driverY: -0.40f, 
                cockpitCam: new Vector3(0.38f, 0.98f, -0.10f),
                exhausts: new List<Vector3> { new Vector3(-0.62f, 0.26f, -2.25f) }),

            new GlbVehicleModel("bmw_m4", "BMW M4 GT3 Evo (2025)", "2025_bmw_m4_gt3_evo_g82.glb", 
                VehicleTypeCategory.SportsCar, "G82 Factory GT3 Aerodynamic Track Weapon", 
                scale: 100.40f, heightOffset: 0.00f, rotY: 0.0f, wheelScale: 0.92f, preset: "HyperInsane", 
                driverY: -0.40f, 
                cockpitCam: new Vector3(-0.38f, 0.95f, -0.12f),
                exhausts: new List<Vector3> { new Vector3(-0.35f, 0.25f, -2.35f), new Vector3(-0.20f, 0.25f, -2.35f), new Vector3(0.20f, 0.25f, -2.35f), new Vector3(0.35f, 0.25f, -2.35f) }),

            new GlbVehicleModel("hypersport", "Lykan Hypersport Bike", "spy-hypersport.glb", 
                VehicleTypeCategory.Motorcycle, "Twin-Turbo 3.7L Exotic Concept Superbike", 
                scale: 1.00f, heightOffset: 0.01f, rotY: 0.0f, wheelScale: 0.85f, preset: "HyperInsane", 
                trackScale: 0.2f, offsetX: -0.09f, driverY: -0.45f, 
                cockpitCam: new Vector3(0.00f, 1.10f, -0.10f),
                exhausts: new List<Vector3> { new Vector3(-0.25f, 0.55f, -2.05f), new Vector3(0.25f, 0.55f, -2.05f) }),
            
            // Trucks & 4x4s
            new GlbVehicleModel("blazer78", "1978 Chevrolet K5 Blazer", "1978_chevrolet_blazer_k5_cheyenne.glb", 
                VehicleTypeCategory.Offroad4x4, "Cheyenne K5 Full-Size 4x4 Classic", 
                scale: 93.60f, heightOffset: 0.10f, rotY: 0.0f, wheelScale: 1.15f, preset: "OffroadLifted", 
                driverY: 0.0f, 
                cockpitCam: new Vector3(-0.45f, 1.60f, 0.10f),
                exhausts: new List<Vector3> { new Vector3(-0.85f, 0.35f, -2.10f), new Vector3(0.85f, 0.35f, -2.10f) }),

            new GlbVehicleModel("raptor_r", "2024 Ford F-150 Raptor R", "2024_ford_f-150_raptor_r.glb", 
                VehicleTypeCategory.Truck, "700+ HP Supercharged V8 Trophy Truck", 
                scale: 98.33f, heightOffset: 0.04f, rotY: 0.0f, wheelScale: 1.10f, preset: "OffroadLifted", 
                driverY: 0.0f, 
                cockpitCam: new Vector3(-0.45f, 1.68f, 0.20f),
                exhausts: new List<Vector3> { new Vector3(-0.55f, 0.45f, -2.60f), new Vector3(0.55f, 0.45f, -2.60f) }),

            new GlbVehicleModel("sierra_at4x", "2024 GMC Sierra 1500 AT4X", "2024_gmc_sierra_1500_at4x.glb", 
                VehicleTypeCategory.Truck, "Premium Heavy Duty Overland Rig", 
                scale: 1.00f, heightOffset: 0.02f, rotY: 0.0f, wheelScale: 1.05f, preset: "OffroadLifted", 
                driverY: 0.0f, 
                cockpitCam: new Vector3(-0.45f, 1.68f, 0.20f),
                exhausts: new List<Vector3> { new Vector3(-0.58f, 0.45f, -2.60f), new Vector3(0.58f, 0.45f, -2.60f) }),

            new GlbVehicleModel("raptor", "Ford F-150 Raptor", "ford_f-150_raptor.glb", 
                VehicleTypeCategory.Truck, "Twin-Turbo EcoBoost Desert Runner", 
                scale: 1.00f, heightOffset: 0.00f, rotY: 180.0f, wheelScale: 1.08f, preset: "OffroadLifted", 
                offsetZ: -0.24f, driverY: 0.0f, 
                cockpitCam: new Vector3(-0.45f, 1.65f, 0.15f),
                exhausts: new List<Vector3> { new Vector3(-0.55f, 0.45f, -2.60f), new Vector3(0.55f, 0.45f, -2.60f) }),

            new GlbVehicleModel("sierra_hd", "GMC Sierra 2500HD", "gmc_sierra_hd2500.glb", 
                VehicleTypeCategory.Truck, "Heavy Duty Duramax Commercial Hauler", 
                scale: 0.403f, heightOffset: -0.11f, rotY: 28.68f, wheelScale: 1.12f, preset: "OffroadLifted", 
                offsetX: 2.37f, offsetZ: -0.37f, driverY: 0.0f, 
                cockpitCam: new Vector3(-0.45f, 1.75f, 0.20f),
                exhausts: new List<Vector3> { new Vector3(0.82f, 0.42f, -2.85f) }),

            // Motorcycles
            new GlbVehicleModel("ninja_h2", "Kawasaki Ninja H2", "kawasaki_ninja_h2.glb", 
                VehicleTypeCategory.Motorcycle, "Supercharged 998cc 200+ MPH Superbike", 
                scale: 0.90f, heightOffset: 0.56f, rotY: 180.0f, wheelScale: 0.85f, preset: "HyperInsane", 
                trackScale: 0.2f, driverY: -0.50f, 
                cockpitCam: new Vector3(0.00f, 1.12f, -0.05f),
                exhausts: new List<Vector3> { new Vector3(0.38f, 0.35f, -0.85f) }),

            new GlbVehicleModel("bmw_s1000", "BMW S1000RR", "bmw_s1000_rr.glb", 
                VehicleTypeCategory.Motorcycle, "999cc Inline-4 Precision Track Superbike", 
                scale: 0.90f, heightOffset: 0.46f, rotY: 180.0f, wheelScale: 0.85f, preset: "HyperInsane", 
                trackScale: 0.2f, driverY: -0.50f, 
                cockpitCam: new Vector3(0.00f, 1.10f, -0.05f),
                exhausts: new List<Vector3> { new Vector3(0.36f, 0.32f, -0.80f) })
        };

        private static Color GetDefaultPaintColor(GlbVehicleModel spec)
        {
            return spec.Id switch
            {
                "cuda71" => new Color(0.95f, 0.28f, 0.05f, 1.0f),       // High-Impact Tor-Red / Vitamin C
                "r34" => new Color(0.08f, 0.25f, 0.85f, 1.0f),          // Nissan Bayside Blue TV2
                "bmw_m4" => new Color(0.95f, 0.95f, 0.97f, 1.0f),       // Factory Alpine White
                "hypersport" => new Color(0.72f, 0.05f, 0.10f, 1.0f),   // Royal Ruby Metallic
                "blazer78" => new Color(0.85f, 0.60f, 0.18f, 1.0f),     // Ochre Yellow / Desert Sand Cheyenne
                "raptor_r" => new Color(0.95f, 0.35f, 0.04f, 1.0f),     // Ford Code Orange
                "raptor" => new Color(0.12f, 0.38f, 0.82f, 1.0f),       // Velocity Blue Metallic
                "sierra_at4x" => new Color(0.68f, 0.08f, 0.08f, 1.0f),   // Volcanic Red Tintcoat
                "sierra_hd" => new Color(0.92f, 0.92f, 0.94f, 1.0f),     // Summit White HD
                "ninja_h2" => new Color(0.15f, 0.82f, 0.12f, 1.0f),     // Kawasaki Racing Green / Mirror Black
                "bmw_s1000" => new Color(0.10f, 0.40f, 0.92f, 1.0f),    // BMW M Motorsport Racing Blue
                _ => new Color(0.85f, 0.85f, 0.85f, 1.0f)
            };
        }

        public static string? ResolveModelPath(string fileName)
        {
            string p1 = Path.Combine(Paths.PluginPath, "ExtendedWhips", "Vehicles", fileName);
            if (File.Exists(p1)) return p1;

            string p2 = Path.Combine(@"D:\VSC Projects\ExtendedWhips\Vehicles", fileName);
            if (File.Exists(p2)) return p2;

            string p3 = Path.Combine(Paths.GameRootPath, "Vehicles", fileName);
            if (File.Exists(p3)) return p3;

            return null;
        }

        // ------------------------------------------------------------------------------------
        // Coordinate conventions
        // ------------------------------------------------------------------------------------
        // glTF is right-handed (+Y up); Unity is left-handed (+Y up). Everything is converted by
        // mirroring the X axis: positions (-x, y, z), normals (-x, y, z), triangle winding reversed,
        // and node matrices M * T * M (where M = diag(-1, 1, 1)).
        // The root GameObject carries ModelScale exactly ONCE. Every child position (wheel pivots
        // included) is kept in unscaled glTF units so the root scale is never applied twice.
        // ------------------------------------------------------------------------------------
        private static readonly System.Numerics.Matrix4x4 MirrorX = System.Numerics.Matrix4x4.CreateScale(-1f, 1f, 1f);

        private static System.Numerics.Matrix4x4 ToUnityBasis(System.Numerics.Matrix4x4 gltfMatrix)
        {
            return MirrorX * gltfMatrix * MirrorX;
        }

        private static void ApplyMatrix(System.Numerics.Matrix4x4 unityMatrix, Transform t)
        {
            if (System.Numerics.Matrix4x4.Decompose(unityMatrix, out var s, out var r, out var p))
            {
                t.localPosition = new Vector3(p.X, p.Y, p.Z);
                t.localRotation = new Quaternion(r.X, r.Y, r.Z, r.W);
                t.localScale = new Vector3(s.X, s.Y, s.Z);
            }
            else
            {
                t.localPosition = new Vector3(unityMatrix.M41, unityMatrix.M42, unityMatrix.M43);
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
        }

        private sealed class Aabb
        {
            public Vector3 Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            public Vector3 Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            public bool IsValid => Min.x <= Max.x;
            public Vector3 Center => (Min + Max) * 0.5f;
            public Vector3 Size => IsValid ? Max - Min : Vector3.zero;

            public void Encapsulate(Vector3 p)
            {
                if (p.x < Min.x) Min.x = p.x; if (p.x > Max.x) Max.x = p.x;
                if (p.y < Min.y) Min.y = p.y; if (p.y > Max.y) Max.y = p.y;
                if (p.z < Min.z) Min.z = p.z; if (p.z > Max.z) Max.z = p.z;
            }

            public void Encapsulate(Aabb other)
            {
                if (!other.IsValid) return;
                Encapsulate(other.Min);
                Encapsulate(other.Max);
            }
        }

        private enum HubKind { None, Wheel, Caliper }
        private enum HubDecision { Plain, Container, Hub }

        private sealed class BuildContext
        {
            public GlbVehicleModel Spec = null!;
            public LoadedVehicleAsset Asset = null!;
            public Transform Root = null!;
            public Transform Chassis = null!;
            public Material? FallbackMat;

            public Dictionary<int, Material> Materials = new();
            public HashSet<int> NonBodyMaterials = new();
            public HashSet<int> NormalMappedMaterials = new();
            public Dictionary<int, float> MaterialArea = new();

            public Dictionary<int, Aabb> OwnBounds = new();
            public Aabb VehicleBounds = new();
            public Dictionary<long, Mesh> MeshCache = new();
            public Dictionary<long, float> AreaCache = new();

            public List<(Transform transform, Vector3 localPos)> PendingWheels = new();
            public List<(Transform transform, Vector3 localPos)> PendingCalipers = new();
        }

        // ------------------------------------------------------------------------------------
        // Name classification helpers
        // ------------------------------------------------------------------------------------
        private static bool HasWord(string lower, string word)
        {
            if (string.IsNullOrEmpty(lower)) return false;
            int idx = 0;
            while ((idx = lower.IndexOf(word, idx, StringComparison.Ordinal)) >= 0)
            {
                bool startOk = idx == 0 || !char.IsLetter(lower[idx - 1]);
                int end = idx + word.Length;
                bool endOk = end >= lower.Length || !char.IsLetter(lower[end]);
                if (startOk && endOk) return true;
                idx = end;
            }
            return false;
        }

        private static bool IsWheelName(string lower)
        {
            if (string.IsNullOrEmpty(lower)) return false;
            // Wheel wells / arch liners / steering wheels are body or cockpit parts, not spinning wheels.
            if (lower.Contains("well") || lower.Contains("arch") || lower.Contains("liner") || lower.Contains("steer")) return false;
            return lower.Contains("wheel") || lower.Contains("tire") || lower.Contains("tyre") || HasWord(lower, "rim") || HasWord(lower, "rims");
        }

        private static bool IsCaliperName(string lower)
        {
            if (string.IsNullOrEmpty(lower)) return false;
            // "brake light" / "brake lamp" are lights, not hardware.
            if (lower.Contains("light") || lower.Contains("lamp")) return false;
            return lower.Contains("caliper") || lower.Contains("brake") || HasWord(lower, "pad") || HasWord(lower, "pads");
        }

        private static bool IsBodyPaintName(string lower)
        {
            if (lower.Contains("body") || lower.Contains("paint") || lower.Contains("exterior") || lower.Contains("chassis") ||
                lower.Contains("hood") || lower.Contains("door") || lower.Contains("trunk") || lower.Contains("bumper") ||
                lower.Contains("fender") || lower.Contains("roof") || lower.Contains("color") || lower.Contains("coat") ||
                lower.Contains("shell") || lower.Contains("panel") || lower.Contains("primary"))
                return true;

            // "car" must not swallow carbon / carpet / cargo / card.
            if (lower.Contains("car") && !lower.Contains("carbon") && !lower.Contains("carpet") && !lower.Contains("cargo") && !lower.Contains("card"))
                return true;

            return false;
        }

        // ------------------------------------------------------------------------------------
        // Material creation (shader fallback + classification)
        // ------------------------------------------------------------------------------------
        private static Material? _sceneLitTemplate;

        private static bool IsUsableLitShader(Shader? sh)
        {
            if (sh == null) return false;
            string n = sh.name ?? "";
            if (n.Length == 0) return false;
            string l = n.ToLowerInvariant();
            if (l.StartsWith("hidden") || l.Contains("decal") || l.Contains("particle") || l.Contains("sprite") || l.Contains("unlit") ||
                l.Contains("skybox") || l.Contains("terrain") || l.Contains("water") || l.Contains("glass") || l.Contains("error"))
                return false;
            return l.Contains("lit") || l.Contains("standard");
        }

        public static bool IsUsableTemplate(Material? m)
        {
            if (m == null || m.shader == null) return false;
            if (!IsUsableLitShader(m.shader)) return false;
            return m.HasProperty("_BaseColor") || m.HasProperty("_Color");
        }

        /// <summary>
        /// Finds an already-loaded opaque HDRP/Standard material in the scene (Weltenbauer vehicle paints first)
        /// whose shader, diffusion profile and render state are known to render correctly in this game.
        /// </summary>
        private static Material? FindSceneLitTemplate()
        {
            if (_sceneLitTemplate != null) return _sceneLitTemplate;

            try
            {
                var all = Resources.FindObjectsOfTypeAll<Material>();
                Material? best = null;
                foreach (var m in all)
                {
                    if (m == null || !IsUsableTemplate(m)) continue;
                    string shaderName = m.shader.name ?? "";
                    if (m.name != null && m.name.IndexOf("Weltenbauer", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        best = m;
                        break;
                    }
                    if (best == null && (shaderName == "HDRP/Lit" || shaderName.EndsWith("/Lit", StringComparison.Ordinal)))
                    {
                        best = m;
                    }
                }
                _sceneLitTemplate = best;
            }
            catch (Exception ex)
            {
                Plugin.LogOnce(ex);
            }
            return _sceneLitTemplate;
        }

        private static readonly string[] TemplateTextureProps =
        {
            "_BaseColorMap", "_BaseMap", "_MainTex", "_NormalMap", "_BumpMap", "_MaskMap", "_MetallicGlossMap", "_OcclusionMap",
            "_EmissiveColorMap", "_EmissionMap", "_DetailMap", "_HeightMap", "_BentNormalMap", "_CoatMaskMap", "_SpecularColorMap",
            "_AnisotropyMap", "_IridescenceThicknessMap", "_SubsurfaceMaskMap", "_TransmissionMaskMap", "_ThicknessMap"
        };

        private static Material CreateBaseMaterial(Material? template, bool doubleSided)
        {
            Material m;
            if (template != null)
            {
                // Clone shader + diffusion profile + render state of a known-good host material, then strip content.
                m = new Material(template);
                foreach (var prop in TemplateTextureProps)
                {
                    if (m.HasProperty(prop)) m.SetTexture(prop, null);
                }
                if (m.HasProperty("_EmissiveColor")) m.SetColor("_EmissiveColor", Color.black);
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", Color.black);
                if (m.HasProperty("_SurfaceType")) m.SetFloat("_SurfaceType", 0f);
                if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 1f);
                if (m.HasProperty("_AlphaCutoffEnable")) m.SetFloat("_AlphaCutoffEnable", 0f);
                m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.DisableKeyword("_NORMALMAP");
                m.DisableKeyword("_MASKMAP");
                m.DisableKeyword("_EMISSIVE_COLOR_MAP");
                m.renderQueue = -1;
            }
            else
            {
                Shader? shader = null;
                foreach (var shaderName in new[] { "HDRP/Lit", "Universal Render Pipeline/Lit", "Standard", "Unlit/Color", "Sprites/Default" })
                {
                    shader = Shader.Find(shaderName);
                    if (shader != null) break;
                }
                m = shader != null ? new Material(shader) : new Material(Shader.Find("Hidden/InternalErrorShader"));

                // HDRP/Lit created from scratch has no diffusion profile: force the plain "Standard" material ID.
                if (m.HasProperty("_MaterialID")) m.SetFloat("_MaterialID", 1f);
                if (m.HasProperty("_EmissiveColor")) m.SetColor("_EmissiveColor", Color.black);
            }

            if (doubleSided)
            {
                if (m.HasProperty("_DoubleSidedEnable")) m.SetFloat("_DoubleSidedEnable", 1f);
                if (m.HasProperty("_CullMode")) m.SetFloat("_CullMode", 0f);
                if (m.HasProperty("_CullModeForward")) m.SetFloat("_CullModeForward", 0f);
                if (m.HasProperty("_Cull")) m.SetFloat("_Cull", 0f);
                m.EnableKeyword("_DOUBLESIDED_ON");
            }

            return m;
        }

        private static void CreateMaterials(SharpGLTF.Schema2.ModelRoot gltf, GlbVehicleModel spec, LoadedVehicleAsset asset,
            Dictionary<int, Texture2D> textures, Material? fallbackMat, BuildContext ctx)
        {
            // Template priority: host vehicle's own valid material -> any known-good lit material in the scene -> shader lookup.
            Material? template = IsUsableTemplate(fallbackMat) ? fallbackMat : FindSceneLitTemplate();

            for (int i = 0; i < gltf.LogicalMaterials.Count; i++)
            {
                var gMat = gltf.LogicalMaterials[i];
                var uMat = CreateBaseMaterial(template, gMat.DoubleSided);
                string matName = string.IsNullOrEmpty(gMat.Name) ? $"Mat_{i}" : gMat.Name;
                uMat.name = matName;

                string lowerMat = matName.ToLowerInvariant();
                bool isGlass = lowerMat.Contains("glass") || lowerMat.Contains("window") || lowerMat.Contains("windshield") || lowerMat.Contains("transp") || lowerMat.Contains("lens") || lowerMat.Contains("visor");
                bool isChrome = lowerMat.Contains("chrome") || lowerMat.Contains("silver") || lowerMat.Contains("mirror") || lowerMat.Contains("exhaust") || lowerMat.Contains("metal");
                bool isTire = lowerMat.Contains("tire") || lowerMat.Contains("tyre") || lowerMat.Contains("rubber");
                bool isWheel = lowerMat.Contains("rim") || lowerMat.Contains("wheel") || lowerMat.Contains("alloy");
                bool isCaliper = lowerMat.Contains("caliper") || lowerMat.Contains("brake");
                bool isInterior = lowerMat.Contains("interior") || lowerMat.Contains("seat") || lowerMat.Contains("dash") || lowerMat.Contains("leather") || lowerMat.Contains("steering");
                bool isLight = lowerMat.Contains("light") || lowerMat.Contains("lamp") || lowerMat.Contains("headlight") || lowerMat.Contains("taillight") || lowerMat.Contains("glow") || lowerMat.Contains("led");
                bool isCarbon = lowerMat.Contains("carbon");
                bool isBodyPaint = IsBodyPaintName(lowerMat);

                System.Numerics.Vector4 colorVec = System.Numerics.Vector4.One;
                bool foundColor = false;
                Texture2D? baseTex = null;

                try
                {
                    var baseColorChannel = gMat.FindChannel("BaseColor") ?? gMat.FindChannel("Diffuse");
                    if (baseColorChannel.HasValue)
                    {
                        try
                        {
                            colorVec = baseColorChannel.Value.Color;
                            foundColor = true;
                        }
                        catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

                        if (baseColorChannel.Value.Texture != null && baseColorChannel.Value.Texture.PrimaryImage != null)
                        {
                            int imgIdx = baseColorChannel.Value.Texture.PrimaryImage.LogicalIndex;
                            textures.TryGetValue(imgIdx, out baseTex);
                        }
                    }
                }
                catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

                Color col;
                float metallic = 0.5f;
                float smoothness = 0.5f;

                if (isGlass)
                {
                    col = new Color(0.1f, 0.12f, 0.15f, 0.35f);
                    metallic = 0.10f;
                    smoothness = 0.95f;
                    asset.GlassMaterials.Add(uMat);
                    ctx.NonBodyMaterials.Add(i);
                }
                else if (isChrome)
                {
                    col = new Color(0.92f, 0.92f, 0.94f, 1.0f);
                    metallic = 0.95f;
                    smoothness = 0.95f;
                    asset.WheelMaterials.Add(uMat);
                    ctx.NonBodyMaterials.Add(i);
                }
                else if (isTire)
                {
                    col = new Color(0.12f, 0.12f, 0.12f, 1.0f);
                    metallic = 0.02f;
                    smoothness = 0.25f;
                    ctx.NonBodyMaterials.Add(i);
                }
                else if (isWheel || isCaliper)
                {
                    if (foundColor && (colorVec.X > 0.15f || colorVec.Y > 0.15f || colorVec.Z > 0.15f))
                        col = new Color(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W);
                    else
                        col = new Color(0.75f, 0.75f, 0.78f, 1.0f);
                    metallic = 0.85f;
                    smoothness = 0.85f;
                    asset.WheelMaterials.Add(uMat);
                    ctx.NonBodyMaterials.Add(i);
                }
                else if (isInterior)
                {
                    if (foundColor && (colorVec.X > 0.15f || colorVec.Y > 0.15f || colorVec.Z > 0.15f))
                        col = new Color(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W);
                    else
                        col = new Color(0.16f, 0.16f, 0.17f, 1.0f);
                    metallic = 0.05f;
                    smoothness = 0.30f;
                    ctx.NonBodyMaterials.Add(i);
                }
                else if (isLight)
                {
                    col = new Color(0.95f, 0.95f, 0.95f, 0.80f);
                    metallic = 0.10f;
                    smoothness = 0.90f;
                    ctx.NonBodyMaterials.Add(i);

                    if (lowerMat.Contains("head") || lowerMat.Contains("front")) asset.HeadlightMaterials.Add(uMat);
                    else if (lowerMat.Contains("brake") || lowerMat.Contains("tail") || lowerMat.Contains("rear") || lowerMat.Contains("stop")) asset.TaillightMaterials.Add(uMat);
                    else if (lowerMat.Contains("rev") || lowerMat.Contains("backup")) asset.ReverseMaterials.Add(uMat);
                    else if (lowerMat.Contains("ind") || lowerMat.Contains("turn") || lowerMat.Contains("blink")) asset.IndicatorMaterials.Add(uMat);
                    else asset.HeadlightMaterials.Add(uMat);
                }
                else if (isCarbon)
                {
                    col = new Color(0.15f, 0.15f, 0.15f, 1.0f);
                    metallic = 0.40f;
                    smoothness = 0.75f;
                    asset.BodyMaterials.Add(uMat);
                }
                else if (baseTex != null)
                {
                    if (foundColor && Mathf.Max(colorVec.X, colorVec.Y, colorVec.Z) > 0.35f)
                        col = new Color(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W);
                    else
                        col = Color.white;
                    metallic = isBodyPaint ? 0.70f : 0.30f;
                    smoothness = isBodyPaint ? 0.85f : 0.60f;
                    if (isBodyPaint) asset.BodyMaterials.Add(uMat);
                }
                else
                {
                    if (foundColor && Mathf.Max(colorVec.X, colorVec.Y, colorVec.Z) > 0.18f)
                        col = new Color(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W);
                    else
                        col = GetDefaultPaintColor(spec);
                    metallic = 0.85f;
                    smoothness = 0.92f;
                    asset.BodyMaterials.Add(uMat);
                }

                if (uMat.HasProperty("_BaseColor")) uMat.SetColor("_BaseColor", col);
                if (uMat.HasProperty("_Color")) uMat.SetColor("_Color", col);
                if (uMat.HasProperty("_Metallic")) uMat.SetFloat("_Metallic", metallic);
                if (uMat.HasProperty("_Smoothness")) uMat.SetFloat("_Smoothness", smoothness);
                if (uMat.HasProperty("_Glossiness")) uMat.SetFloat("_Glossiness", smoothness);

                if (baseTex != null)
                {
                    if (uMat.HasProperty("_BaseColorMap")) uMat.SetTexture("_BaseColorMap", baseTex);
                    if (uMat.HasProperty("_BaseMap")) uMat.SetTexture("_BaseMap", baseTex);
                    if (uMat.HasProperty("_MainTex")) uMat.SetTexture("_MainTex", baseTex);
                    uMat.mainTexture = baseTex;
                }

                try
                {
                    var normChannel = gMat.FindChannel("Normal");
                    if (normChannel.HasValue && normChannel.Value.Texture != null && normChannel.Value.Texture.PrimaryImage != null)
                    {
                        int normIdx = normChannel.Value.Texture.PrimaryImage.LogicalIndex;
                        if (textures.TryGetValue(normIdx, out var nTex))
                        {
                            if (uMat.HasProperty("_NormalMap")) uMat.SetTexture("_NormalMap", nTex);
                            if (uMat.HasProperty("_BumpMap")) uMat.SetTexture("_BumpMap", nTex);
                            uMat.EnableKeyword("_NORMALMAP");
                            uMat.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
                            ctx.NormalMappedMaterials.Add(i);
                        }
                    }
                }
                catch (System.Exception ex_) { Plugin.LogOnce(ex_); }

                if (isGlass || col.a < 0.95f)
                {
                    if (uMat.HasProperty("_SurfaceType")) uMat.SetFloat("_SurfaceType", 1f);
                    if (uMat.HasProperty("_BlendMode")) uMat.SetFloat("_BlendMode", 0f);
                    if (uMat.HasProperty("_ZWrite")) uMat.SetFloat("_ZWrite", 0f);
                    uMat.renderQueue = 3000;
                }

                ctx.Materials[i] = uMat;
            }
        }

        // ------------------------------------------------------------------------------------
        // Node bounds (root space, already converted to Unity handedness)
        // ------------------------------------------------------------------------------------
        private static Aabb GetOwnBounds(BuildContext ctx, GltfNode node)
        {
            if (ctx.OwnBounds.TryGetValue(node.LogicalIndex, out var cached)) return cached;

            var box = new Aabb();
            if (node.Mesh != null)
            {
                var world = node.WorldMatrix;
                foreach (var prim in node.Mesh.Primitives)
                {
                    var posAcc = prim.GetVertexAccessor("POSITION");
                    if (posAcc == null) continue;
                    var positions = posAcc.AsVector3Array();
                    for (int i = 0; i < positions.Count; i++)
                    {
                        var p = System.Numerics.Vector3.Transform(positions[i], world);
                        box.Encapsulate(new Vector3(-p.X, p.Y, p.Z));
                    }
                }
            }

            ctx.OwnBounds[node.LogicalIndex] = box;
            return box;
        }

        private static Aabb GetSubtreeBounds(BuildContext ctx, GltfNode node)
        {
            var box = new Aabb();
            box.Encapsulate(GetOwnBounds(ctx, node));
            foreach (var child in node.VisualChildren)
            {
                box.Encapsulate(GetSubtreeBounds(ctx, child));
            }
            return box;
        }

        private static HubKind ClassifyHub(GltfNode node, string lowerNode, bool hasMesh, HubKind inherited)
        {
            string lowerMesh = (node.Mesh?.Name ?? "").ToLowerInvariant();
            bool matWheel = false;
            bool matCaliper = false;

            if (hasMesh)
            {
                foreach (var p in node.Mesh!.Primitives)
                {
                    if (p.Material == null) continue;
                    string matName = (p.Material.Name ?? "").ToLowerInvariant();
                    if (IsWheelName(matName)) matWheel = true;
                    if (IsCaliperName(matName)) matCaliper = true;
                }
            }

            if (IsCaliperName(lowerNode) || (hasMesh && (IsCaliperName(lowerMesh) || matCaliper))) return HubKind.Caliper;
            if (IsWheelName(lowerNode) || (hasMesh && (IsWheelName(lowerMesh) || matWheel))) return HubKind.Wheel;
            return inherited;
        }

        private static HubDecision DecideHub(BuildContext ctx, Aabb sub, HubKind kind, bool forced)
        {
            if (!sub.IsValid) return HubDecision.Plain;
            if (forced) return HubDecision.Hub;

            Vector3 s = sub.Size;
            Vector3 v = ctx.VehicleBounds.Size;

            // Wheels and calipers live in the lower part of the vehicle; a "wheel" up in the cabin is a steering wheel.
            float heightLimit = ctx.VehicleBounds.Min.y + 0.45f * v.y;
            if (sub.Center.y > heightLimit) return HubDecision.Plain;

            // A group containing several wheels (axle pair, all four) is a container, not a single wheel hub.
            if (s.x > 0.5f * v.x || s.z > 0.55f * v.z) return HubDecision.Container;
            if (kind == HubKind.Wheel)
            {
                float h = Mathf.Max(s.y, 0.0001f);
                if (s.x > 2f * h || s.z > 2f * h) return HubDecision.Container;
            }

            return HubDecision.Hub;
        }

        private static bool HasAncestorIn(Transform t, List<Transform> list)
        {
            for (var p = t.parent; p != null; p = p.parent)
            {
                if (list.Contains(p)) return true;
            }
            return false;
        }

        private static void AddPart(List<Transform> list, Dictionary<Transform, Quaternion> rest, Transform t)
        {
            // A parent group that already animates would double-rotate its children.
            if (HasAncestorIn(t, list)) return;
            list.Add(t);
            rest[t] = t.localRotation;
        }

        private static void RegisterBodyParts(LoadedVehicleAsset asset, Transform t, string lowerNode, string lowerMesh)
        {
            bool isSteeringWheel = lowerNode.Contains("steer") || lowerMesh.Contains("steer") || lowerNode.Contains("volant") || lowerNode.Contains("handlebar");
            bool isSpoiler = lowerNode.Contains("spoiler") || lowerMesh.Contains("spoiler") || lowerNode.Contains("wing") || lowerMesh.Contains("wing") || lowerNode.Contains("airbrake");
            bool isSpeedNeedle = lowerNode.Contains("speed") && (lowerNode.Contains("needle") || lowerNode.Contains("arrow") || lowerNode.Contains("gauge"));
            bool isRpmNeedle = (lowerNode.Contains("rpm") || lowerNode.Contains("tacho") || lowerNode.Contains("rev")) && (lowerNode.Contains("needle") || lowerNode.Contains("arrow") || lowerNode.Contains("gauge"));
            bool isHood = lowerNode.Contains("hood") || lowerMesh.Contains("hood") || lowerNode.Contains("bonnet");
            bool isDoorL = lowerNode.Contains("door") && (lowerNode.Contains("_l") || lowerNode.Contains("left") || lowerNode.Contains("fl"));
            bool isDoorR = lowerNode.Contains("door") && (lowerNode.Contains("_r") || lowerNode.Contains("right") || lowerNode.Contains("fr"));
            bool isTrunk = lowerNode.Contains("trunk") || lowerMesh.Contains("trunk") || lowerNode.Contains("boot") || lowerNode.Contains("tailgate");

            if (isSteeringWheel) AddPart(asset.SteeringWheelTransforms, asset.SteeringWheelRestRotations, t);
            if (isHood) AddPart(asset.HoodTransforms, asset.HoodRestRotations, t);
            if (isDoorL) AddPart(asset.LeftDoorTransforms, asset.LeftDoorRestRotations, t);
            if (isDoorR) AddPart(asset.RightDoorTransforms, asset.RightDoorRestRotations, t);
            if (isTrunk) AddPart(asset.TrunkTransforms, asset.TrunkRestRotations, t);
            if (isSpoiler) AddPart(asset.SpoilerTransforms, asset.SpoilerRestRotations, t);
            if (isSpeedNeedle) AddPart(asset.SpeedometerNeedles, asset.GaugeRestRotations, t);
            if (isRpmNeedle) AddPart(asset.TachometerNeedles, asset.GaugeRestRotations, t);
        }

        // ------------------------------------------------------------------------------------
        // Hierarchy reconstruction
        // ------------------------------------------------------------------------------------
        private static void BuildNode(BuildContext ctx, GltfNode node, Transform parent, bool insideHub, HubKind inheritedKind)
        {
            string nodeName = string.IsNullOrEmpty(node.Name) ? $"Node_{node.LogicalIndex}" : node.Name;
            string lowerNode = nodeName.ToLowerInvariant();
            string lowerMesh = (node.Mesh?.Name ?? "").ToLowerInvariant();
            bool hasMesh = node.Mesh != null && node.Mesh.Primitives.Count > 0;

            HubKind kind = HubKind.None;
            bool forced = false;
            if (!insideHub)
            {
                kind = ClassifyHub(node, lowerNode, hasMesh, inheritedKind);

                // Special case for spy-hypersport superbike wheels
                if (ctx.Spec.Id == "hypersport" && (node.LogicalIndex == 2 || node.LogicalIndex == 25))
                {
                    kind = HubKind.Wheel;
                    forced = true;
                }
            }

            HubDecision decision = HubDecision.Plain;
            Aabb? sub = null;
            if (kind != HubKind.None)
            {
                sub = GetSubtreeBounds(ctx, node);
                decision = DecideHub(ctx, sub, kind, forced);
            }

            GameObject nodeObj;
            if (decision == HubDecision.Hub && sub != null)
            {
                // The hub is the WheelTransform: it sits exactly at the axle centre, directly under the root, with
                // identity rotation so spin (local X) and steer (local Y) are applied in vehicle space.
                // The glTF node (and its whole child subtree) hangs below it with its world pose re-expressed
                // relative to the axle, so the node's own offset is never scaled or applied a second time.
                Vector3 pivot = sub.Center;

                var hubObj = new GameObject($"{(kind == HubKind.Caliper ? "CaliperHub" : "WheelHub")}_{nodeName}");
                hubObj.transform.SetParent(ctx.Root, false);
                hubObj.transform.localPosition = pivot;
                hubObj.transform.localRotation = Quaternion.identity;
                hubObj.transform.localScale = Vector3.one;

                nodeObj = new GameObject(nodeName);
                nodeObj.transform.SetParent(hubObj.transform, false);
                ApplyMatrix(ToUnityBasis(node.WorldMatrix), nodeObj.transform);
                nodeObj.transform.localPosition = nodeObj.transform.localPosition - pivot;

                ctx.Asset.WheelRestLocalPositions[hubObj.transform] = hubObj.transform.localPosition;
                ctx.Asset.WheelRestLocalRotations[hubObj.transform] = hubObj.transform.localRotation;
                ctx.Asset.WheelObjects.Add(hubObj);

                if (kind == HubKind.Caliper) ctx.PendingCalipers.Add((hubObj.transform, hubObj.transform.localPosition));
                else ctx.PendingWheels.Add((hubObj.transform, hubObj.transform.localPosition));
            }
            else
            {
                nodeObj = new GameObject(nodeName);
                nodeObj.transform.SetParent(parent, false);
                ApplyMatrix(ToUnityBasis(node.LocalMatrix), nodeObj.transform);

                if (!insideHub && hasMesh)
                {
                    ctx.Asset.BodyObjects.Add(nodeObj);
                    RegisterBodyParts(ctx.Asset, nodeObj.transform, lowerNode, lowerMesh);
                }
            }

            AttachMeshes(ctx, node, nodeObj, nodeName);

            bool childInside = insideHub || decision == HubDecision.Hub;
            HubKind childKind = decision == HubDecision.Container ? kind : HubKind.None;
            foreach (var child in node.VisualChildren)
            {
                BuildNode(ctx, child, nodeObj.transform, childInside, childKind);
            }
        }

        private static void AttachMeshes(BuildContext ctx, GltfNode node, GameObject nodeObj, string nodeName)
        {
            if (node.Mesh == null || node.Mesh.Primitives.Count == 0) return;

            // Approximate uniform world scale of this node, used to weight material surface area.
            float worldScale = MathF.Cbrt(MathF.Abs(node.WorldMatrix.GetDeterminant()));
            if (worldScale < 1e-6f) worldScale = 1f;

            for (int pIdx = 0; pIdx < node.Mesh.Primitives.Count; pIdx++)
            {
                var prim = node.Mesh.Primitives[pIdx];
                var posAcc = prim.GetVertexAccessor("POSITION");
                if (posAcc == null) continue;

                int matIdx = prim.Material != null ? prim.Material.LogicalIndex : -1;
                long key = ((long)node.Mesh.LogicalIndex << 16) | (long)(uint)pIdx;

                if (!ctx.MeshCache.TryGetValue(key, out var mesh) || mesh == null)
                {
                    // Vertices stay in node-local space; the GameObject hierarchy carries every transform.
                    var rawPos = posAcc.AsVector3Array();
                    var vertices = new Vector3[rawPos.Count];
                    for (int v = 0; v < rawPos.Count; v++)
                    {
                        var p = rawPos[v];
                        vertices[v] = new Vector3(-p.X, p.Y, p.Z);
                    }

                    Vector3[]? normals = null;
                    var normAcc = prim.GetVertexAccessor("NORMAL");
                    if (normAcc != null)
                    {
                        var rawNorm = normAcc.AsVector3Array();
                        normals = new Vector3[rawNorm.Count];
                        for (int n = 0; n < rawNorm.Count; n++)
                        {
                            var nm = rawNorm[n];
                            normals[n] = new Vector3(-nm.X, nm.Y, nm.Z);
                        }
                    }

                    Vector2[]? uvs = null;
                    var uvAcc = prim.GetVertexAccessor("TEXCOORD_0");
                    if (uvAcc != null)
                    {
                        var rawUv = uvAcc.AsVector2Array();
                        uvs = new Vector2[rawUv.Count];
                        for (int u = 0; u < rawUv.Count; u++)
                        {
                            var uv = rawUv[u];
                            uvs[u] = new Vector2(uv.X, 1.0f - uv.Y);
                        }
                    }

                    // X mirroring flips handedness, so triangle winding is reversed to keep faces outward.
                    var rawIndices = prim.GetIndices();
                    var triangles = new int[rawIndices.Count];
                    float area = 0f;
                    for (int t = 0; t + 2 < rawIndices.Count; t += 3)
                    {
                        int i0 = (int)rawIndices[t];
                        int i1 = (int)rawIndices[t + 1];
                        int i2 = (int)rawIndices[t + 2];
                        triangles[t] = i0;
                        triangles[t + 1] = i2;
                        triangles[t + 2] = i1;

                        if (i0 < vertices.Length && i1 < vertices.Length && i2 < vertices.Length)
                        {
                            area += 0.5f * Vector3.Cross(vertices[i1] - vertices[i0], vertices[i2] - vertices[i0]).magnitude;
                        }
                    }

                    mesh = new Mesh();
                    mesh.name = $"{nodeName}_prim{pIdx}";
                    if (vertices.Length > 65530)
                    {
                        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    }
                    mesh.vertices = vertices;
                    if (normals != null) mesh.normals = normals;
                    if (uvs != null) mesh.uv = uvs;
                    mesh.triangles = triangles;

                    if (normals == null) mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    if (uvs != null && matIdx >= 0 && ctx.NormalMappedMaterials.Contains(matIdx))
                    {
                        mesh.RecalculateTangents();
                    }

                    ctx.MeshCache[key] = mesh;
                    ctx.AreaCache[key] = area;
                }

                if (matIdx >= 0 && ctx.AreaCache.TryGetValue(key, out var primArea))
                {
                    ctx.MaterialArea.TryGetValue(matIdx, out var existing);
                    ctx.MaterialArea[matIdx] = existing + primArea * worldScale * worldScale;
                }

                GameObject targetObj = (pIdx == 0) ? nodeObj : new GameObject($"{nodeName}_p{pIdx}");
                if (targetObj != nodeObj)
                {
                    targetObj.transform.SetParent(nodeObj.transform, false);
                    targetObj.transform.localPosition = Vector3.zero;
                    targetObj.transform.localRotation = Quaternion.identity;
                    targetObj.transform.localScale = Vector3.one;
                }

                var mf = targetObj.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;

                var mr = targetObj.AddComponent<MeshRenderer>();
                if (matIdx >= 0 && ctx.Materials.TryGetValue(matIdx, out var mat))
                {
                    mr.sharedMaterial = mat;
                }
                else if (ctx.FallbackMat != null)
                {
                    mr.sharedMaterial = ctx.FallbackMat;
                }
            }
        }

        private static void PartitionHubs(List<(Transform transform, Vector3 localPos)> pending, float rotationY,
            List<Transform> frontLeft, List<Transform> frontRight, List<Transform> rearLeft, List<Transform> rearRight)
        {
            if (pending.Count == 0) return;

            float minZ = float.MaxValue, maxZ = float.MinValue;
            float minX = float.MaxValue, maxX = float.MinValue;

            // If model has authored RotationY (like Sierra HD or the 180-degree bikes), project points into vehicle-aligned axes
            float rad = rotationY * Mathf.Deg2Rad;
            float cosY = Mathf.Cos(rad);
            float sinY = Mathf.Sin(rad);

            var alignedCoords = new List<Vector2>(); // (alignedX, alignedZ)
            for (int i = 0; i < pending.Count; i++)
            {
                var p = pending[i].localPos;
                float ax = p.x * cosY + p.z * sinY;
                float az = -p.x * sinY + p.z * cosY;
                alignedCoords.Add(new Vector2(ax, az));

                if (az < minZ) minZ = az;
                if (az > maxZ) maxZ = az;
                if (ax < minX) minX = ax;
                if (ax > maxX) maxX = ax;
            }

            float midZ = (minZ + maxZ) / 2f;
            float midX = (minX + maxX) / 2f;

            for (int i = 0; i < pending.Count; i++)
            {
                var t = pending[i].transform;
                var ac = alignedCoords[i];

                bool isFront = (ac.y > midZ);
                bool isLeft = (ac.x < midX);

                if (isFront)
                {
                    if (isLeft) frontLeft.Add(t);
                    else frontRight.Add(t);
                }
                else
                {
                    if (isLeft) rearLeft.Add(t);
                    else rearRight.Add(t);
                }
            }
        }

        public static LoadedVehicleAsset? LoadGlbModel(GlbVehicleModel spec, Material? fallbackMat)
        {
            string? filePath = ResolveModelPath(spec.FileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Plugin.LogMsg($"[GlbVehicleLoader] File not found: {spec.FileName}");
                return null;
            }

            try
            {
                Plugin.LogMsg($"[GlbVehicleLoader] Loading GLB model '{spec.DisplayName}' from: {filePath}");
                var gltf = SharpGLTF.Schema2.ModelRoot.Load(filePath);

                var rootObj = new GameObject($"CustomWhip_{spec.Id}");
                var asset = new LoadedVehicleAsset(spec, rootObj);

                // ChassisPivot owns every body mesh (lean/roll is applied here only). Wheel hubs stay directly under the root.
                var chassisObj = new GameObject("ChassisPivot");
                chassisObj.transform.SetParent(rootObj.transform, false);
                chassisObj.transform.localPosition = Vector3.zero;
                chassisObj.transform.localRotation = Quaternion.identity;
                chassisObj.transform.localScale = Vector3.one;
                asset.ChassisPivot = chassisObj;

                // Cache textures
                var textures = new Dictionary<int, Texture2D>();
                for (int i = 0; i < gltf.LogicalImages.Count; i++)
                {
                    try
                    {
                        var img = gltf.LogicalImages[i];
                        byte[] rawBytes = img.Content.Content.ToArray();
                        if (rawBytes.Length > 0)
                        {
                            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                            ImageConversion.LoadImage(tex, rawBytes);
                            tex.name = string.IsNullOrEmpty(img.Name) ? $"Tex_{i}" : img.Name;
                            textures[i] = tex;
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogMsg($"[GlbVehicleLoader] Failed to decode image {i}: {ex.Message}");
                    }
                }

                var ctx = new BuildContext
                {
                    Spec = spec,
                    Asset = asset,
                    Root = rootObj.transform,
                    Chassis = chassisObj.transform,
                    FallbackMat = fallbackMat
                };

                CreateMaterials(gltf, spec, asset, textures, fallbackMat, ctx);

                // Rebuild the glTF scene graph so children inherit their parents' transforms.
                var sceneRoots = gltf.LogicalNodes.Where(n => n.VisualParent == null).ToList();
                foreach (var sceneRoot in sceneRoots)
                {
                    ctx.VehicleBounds.Encapsulate(GetSubtreeBounds(ctx, sceneRoot));
                }
                foreach (var sceneRoot in sceneRoots)
                {
                    BuildNode(ctx, sceneRoot, chassisObj.transform, false, HubKind.None);
                }

                // Paint Studio fallback: generic material names ("Material #25") never match a keyword, so
                // adopt the material covering the largest surface area (excluding glass / wheels / lights / interior).
                if (asset.BodyMaterials.Count == 0)
                {
                    int bestIdx = -1;
                    float bestArea = -1f;
                    foreach (var kv in ctx.MaterialArea)
                    {
                        if (ctx.NonBodyMaterials.Contains(kv.Key)) continue;
                        if (!ctx.Materials.ContainsKey(kv.Key)) continue;
                        if (kv.Value > bestArea)
                        {
                            bestArea = kv.Value;
                            bestIdx = kv.Key;
                        }
                    }

                    if (bestIdx >= 0)
                    {
                        asset.BodyMaterials.Add(ctx.Materials[bestIdx]);
                        Plugin.LogMsg($"[GlbVehicleLoader] No body material keyword match for '{spec.DisplayName}'; using largest surface '{ctx.Materials[bestIdx].name}' ({gltf.LogicalMaterials.Count} materials) for Paint Studio.");
                    }
                }

                // Partition detected wheels / calipers into Front/Rear and Left/Right using relative bounding midpoints
                PartitionHubs(ctx.PendingWheels, spec.RotationY, asset.FrontLeftWheels, asset.FrontRightWheels, asset.RearLeftWheels, asset.RearRightWheels);
                PartitionHubs(ctx.PendingCalipers, spec.RotationY, asset.FrontLeftCalipers, asset.FrontRightCalipers, asset.RearLeftCalipers, asset.RearRightCalipers);

                // Global model scale (applied exactly once, here), center offset, ground height offset, and rotation angle
                rootObj.transform.localScale = Vector3.one * spec.ModelScale;
                rootObj.transform.localPosition = new Vector3(spec.CenterOffsetX, spec.HeightOffset, spec.CenterOffsetZ);
                rootObj.transform.localRotation = Quaternion.Euler(0f, spec.RotationY, 0f);

                // Initialize Lights, Exhaust FX, Audio Synthesizer, Nitrous, Surface FX, Strobes, and Stereo (isolated failure protection)
                try { CustomVehicleLights.InitializeLights(asset); } catch (Exception ex) { Plugin.LogMsg($"[GlbVehicleLoader] Failed to initialize lights: {ex}"); }
                try { CustomVehicleExhaust.InitializeExhaust(asset); } catch (Exception ex) { Plugin.LogMsg($"[GlbVehicleLoader] Failed to initialize exhaust: {ex}"); }
                try { CustomVehicleAudio.InitializeAudio(asset); } catch (Exception ex) { Plugin.LogMsg($"[GlbVehicleLoader] Failed to initialize audio: {ex}"); }
                try { CustomVehicleNitrous.InitializeNitrous(asset); } catch (Exception ex) { Plugin.LogMsg($"[GlbVehicleLoader] Failed to initialize nitrous: {ex}"); }
                try { CustomSurfaceFX.InitializeSurfaceFX(asset); } catch (Exception ex) { Plugin.LogMsg($"[GlbVehicleLoader] Failed to initialize surface FX: {ex}"); }
                try { CustomEmergencyLighting.InitializeEmergencyLighting(asset); } catch (Exception ex) { Plugin.LogMsg($"[GlbVehicleLoader] Failed to initialize emergency lighting: {ex}"); }
                try { InCabinStereo.InitializeStereo(asset); } catch (Exception ex) { Plugin.LogMsg($"[GlbVehicleLoader] Failed to initialize stereo: {ex}"); }
                try { EvPowertrainSystem.InitializeEvAudio(asset); } catch (Exception ex) { Plugin.LogMsg($"[GlbVehicleLoader] Failed to initialize EV audio: {ex}"); }

                int wheelCount = asset.FrontLeftWheels.Count + asset.FrontRightWheels.Count + asset.RearLeftWheels.Count + asset.RearRightWheels.Count;
                Plugin.LogMsg($"[GlbVehicleLoader] Successfully loaded '{spec.DisplayName}' (Body: {asset.BodyObjects.Count}, Wheels: {wheelCount}, FL: {asset.FrontLeftWheels.Count}, Calipers: {asset.FrontLeftCalipers.Count + asset.FrontRightCalipers.Count}, Doors: {asset.LeftDoorTransforms.Count + asset.RightDoorTransforms.Count}, Hood: {asset.HoodTransforms.Count}, PaintMaterials: {asset.BodyMaterials.Count}).");
                return asset;
            }
            catch (Exception ex)
            {
                Plugin.LogMsg($"[GlbVehicleLoader] Error importing GLB model '{spec.DisplayName}': {ex}");
                return null;
            }
        }
    }
}
