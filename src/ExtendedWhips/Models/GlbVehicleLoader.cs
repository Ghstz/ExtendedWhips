using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;
using ExtendedWhips.Effects;
using ExtendedWhips.Tuning;

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
                scale: 93.60f, heightOffset: 0.49f, rotY: 0.0f, wheelScale: 1.15f, preset: "OffroadLifted", 
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
                scale: 1.00f, heightOffset: 0.00f, rotY: 0.0f, wheelScale: 1.08f, preset: "OffroadLifted", 
                offsetZ: -0.24f, driverY: 0.0f, 
                cockpitCam: new Vector3(-0.45f, 1.65f, 0.15f),
                exhausts: new List<Vector3> { new Vector3(-0.55f, 0.45f, -2.60f), new Vector3(0.55f, 0.45f, -2.60f) }),

            new GlbVehicleModel("sierra_hd", "GMC Sierra 2500HD", "gmc_sierra_hd2500.glb", 
                VehicleTypeCategory.Truck, "Heavy Duty Duramax Commercial Hauler", 
                scale: 0.403f, heightOffset: -0.11f, rotY: 180.0f, wheelScale: 1.12f, preset: "OffroadLifted", 
                offsetX: 1.82f, offsetZ: -1.63f, driverY: 0.0f, 
                cockpitCam: new Vector3(-0.45f, 1.75f, 0.20f),
                exhausts: new List<Vector3> { new Vector3(0.82f, 0.42f, -2.85f) }),

            // Motorcycles
            new GlbVehicleModel("ninja_h2", "Kawasaki Ninja H2", "kawasaki_ninja_h2.glb", 
                VehicleTypeCategory.Motorcycle, "Supercharged 998cc 200+ MPH Superbike", 
                scale: 0.90f, heightOffset: 0.56f, rotY: 0.0f, wheelScale: 0.85f, preset: "HyperInsane", 
                trackScale: 0.2f, driverY: -0.50f, 
                cockpitCam: new Vector3(0.00f, 1.12f, -0.05f),
                exhausts: new List<Vector3> { new Vector3(0.38f, 0.35f, -0.85f) }),

            new GlbVehicleModel("bmw_s1000", "BMW S1000RR", "bmw_s1000_rr.glb", 
                VehicleTypeCategory.Motorcycle, "999cc Inline-4 Precision Track Superbike", 
                scale: 0.90f, heightOffset: 0.46f, rotY: 0.0f, wheelScale: 0.85f, preset: "HyperInsane", 
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

                // Base shader
                Shader? targetShader = null;
                if (fallbackMat != null && fallbackMat.shader != null)
                {
                    targetShader = fallbackMat.shader;
                }
                if (targetShader == null)
                {
                    targetShader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
                }

                // Materials cache & categorization
                var materials = new Dictionary<int, Material>();
                for (int i = 0; i < gltf.LogicalMaterials.Count; i++)
                {
                    var gMat = gltf.LogicalMaterials[i];
                    var uMat = targetShader != null ? new Material(targetShader) : new Material(Shader.Find("Standard"));
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
                    bool isBodyPaint = lowerMat.Contains("body") || lowerMat.Contains("paint") || lowerMat.Contains("carpaint") || lowerMat.Contains("exterior") || lowerMat.Contains("chassis") || lowerMat.Contains("hood") || lowerMat.Contains("door") || lowerMat.Contains("trunk") || lowerMat.Contains("bumper") || lowerMat.Contains("fender") || lowerMat.Contains("roof") || lowerMat.Contains("color");

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
                    }
                    else if (isChrome)
                    {
                        col = new Color(0.92f, 0.92f, 0.94f, 1.0f);
                        metallic = 0.95f;
                        smoothness = 0.95f;
                        asset.WheelMaterials.Add(uMat);
                    }
                    else if (isTire)
                    {
                        col = new Color(0.12f, 0.12f, 0.12f, 1.0f);
                        metallic = 0.02f;
                        smoothness = 0.25f;
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
                    }
                    else if (isInterior)
                    {
                        if (foundColor && (colorVec.X > 0.15f || colorVec.Y > 0.15f || colorVec.Z > 0.15f))
                            col = new Color(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W);
                        else
                            col = new Color(0.16f, 0.16f, 0.17f, 1.0f);
                        metallic = 0.05f;
                        smoothness = 0.30f;
                    }
                    else if (isLight)
                    {
                        col = new Color(0.95f, 0.95f, 0.95f, 0.80f);
                        metallic = 0.10f;
                        smoothness = 0.90f;

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

                    materials[i] = uMat;
                }

                // Process nodes & meshes
                foreach (var node in gltf.LogicalNodes)
                {
                    if (node.Mesh == null || node.Mesh.Primitives.Count == 0) continue;

                    string nodeName = string.IsNullOrEmpty(node.Name) ? $"Node_{node.LogicalIndex}" : node.Name;
                    string lowerNode = nodeName.ToLowerInvariant();
                    string lowerMesh = (node.Mesh.Name ?? "").ToLowerInvariant();

                    bool matIsWheel = false;
                    bool matIsCaliper = false;
                    foreach (var p in node.Mesh.Primitives)
                    {
                        if (p.Material != null)
                        {
                            string matName = (p.Material.Name ?? "").ToLowerInvariant();
                            if (matName.Contains("wheel") || matName.Contains("tire") || matName.Contains("tyre") || matName.Contains("rim"))
                            {
                                matIsWheel = true;
                            }
                            if (matName.Contains("caliper") || matName.Contains("brake"))
                            {
                                matIsCaliper = true;
                            }
                        }
                    }

                    bool isCaliper = lowerNode.Contains("caliper") || lowerNode.Contains("brake") || lowerNode.Contains("pad") ||
                                    lowerMesh.Contains("caliper") || lowerMesh.Contains("brake") || matIsCaliper;

                    bool isWheel = (lowerNode.Contains("wheel") || lowerNode.Contains("tire") || lowerNode.Contains("tyre") ||
                                   lowerNode.Contains("rim") || lowerMesh.Contains("wheel") || lowerMesh.Contains("tire") ||
                                   lowerMesh.Contains("tyre") || lowerMesh.Contains("rim") || matIsWheel) || isCaliper;

                    bool isSteeringWheel = lowerNode.Contains("steer") || lowerMesh.Contains("steer") || lowerNode.Contains("volant") || lowerNode.Contains("handlebar");
                    bool isSpoiler = lowerNode.Contains("spoiler") || lowerMesh.Contains("spoiler") || lowerNode.Contains("wing") || lowerMesh.Contains("wing") || lowerNode.Contains("airbrake");
                    bool isSpeedNeedle = lowerNode.Contains("speed") && (lowerNode.Contains("needle") || lowerNode.Contains("arrow") || lowerNode.Contains("gauge"));
                    bool isRpmNeedle = (lowerNode.Contains("rpm") || lowerNode.Contains("tacho") || lowerNode.Contains("rev")) && (lowerNode.Contains("needle") || lowerNode.Contains("arrow") || lowerNode.Contains("gauge"));
                    bool isHood = lowerNode.Contains("hood") || lowerMesh.Contains("hood") || lowerNode.Contains("bonnet");
                    bool isDoorL = lowerNode.Contains("door") && (lowerNode.Contains("_l") || lowerNode.Contains("left") || lowerNode.Contains("fl"));
                    bool isDoorR = lowerNode.Contains("door") && (lowerNode.Contains("_r") || lowerNode.Contains("right") || lowerNode.Contains("fr"));
                    bool isTrunk = lowerNode.Contains("trunk") || lowerMesh.Contains("trunk") || lowerNode.Contains("boot") || lowerNode.Contains("tailgate");

                    var worldMatrix = node.WorldMatrix;
                    System.Numerics.Matrix4x4 normMatrix = worldMatrix;
                    if (System.Numerics.Matrix4x4.Invert(worldMatrix, out var invMat))
                    {
                        normMatrix = System.Numerics.Matrix4x4.Transpose(invMat);
                    }

                    // Axle Center computation
                    System.Numerics.Vector3 centerPivot = System.Numerics.Vector3.Zero;
                    if (isWheel || isSteeringWheel)
                    {
                        float minX = float.MaxValue, maxX = float.MinValue;
                        float minY = float.MaxValue, maxY = float.MinValue;
                        float minZ = float.MaxValue, maxZ = float.MinValue;

                        foreach (var prim in node.Mesh.Primitives)
                        {
                            var posAcc = prim.GetVertexAccessor("POSITION");
                            if (posAcc == null) continue;
                            var positions = posAcc.AsVector3Array();
                            for (int i = 0; i < positions.Count; i++)
                            {
                                var p = System.Numerics.Vector3.Transform(positions[i], worldMatrix);
                                if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
                                if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
                                if (p.Z < minZ) minZ = p.Z; if (p.Z > maxZ) maxZ = p.Z;
                            }
                        }

                        if (minX <= maxX)
                        {
                            centerPivot = new System.Numerics.Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f);
                        }
                    }

                    var nodeObj = new GameObject(nodeName);
                    nodeObj.transform.SetParent(rootObj.transform, false);

                    if (isWheel)
                    {
                        nodeObj.transform.localPosition = new Vector3(-centerPivot.X, centerPivot.Y, centerPivot.Z);
                        nodeObj.transform.localRotation = Quaternion.identity;
                        nodeObj.transform.localScale = Vector3.one;

                        // Store rest position for real suspension spring travel
                        asset.WheelRestLocalPositions[nodeObj.transform] = nodeObj.transform.localPosition;

                        float unityX = -centerPivot.X;
                        float unityZ = centerPivot.Z;

                        if (isCaliper)
                        {
                            // Separate Brake Calipers: steer without spinning
                            if (unityZ > 0.05f)
                            {
                                if (unityX < 0f) asset.FrontLeftCalipers.Add(nodeObj.transform);
                                else asset.FrontRightCalipers.Add(nodeObj.transform);
                            }
                            else
                            {
                                if (unityX < 0f) asset.RearLeftCalipers.Add(nodeObj.transform);
                                else asset.RearRightCalipers.Add(nodeObj.transform);
                            }
                        }
                        else
                        {
                            // Spinning Rims & Tires
                            if (unityZ > 0.05f)
                            {
                                if (unityX < 0f) asset.FrontLeftWheels.Add(nodeObj.transform);
                                else asset.FrontRightWheels.Add(nodeObj.transform);
                            }
                            else
                            {
                                if (unityX < 0f) asset.RearLeftWheels.Add(nodeObj.transform);
                                else asset.RearRightWheels.Add(nodeObj.transform);
                            }
                        }
                        asset.WheelObjects.Add(nodeObj);
                    }
                    else
                    {
                        nodeObj.transform.localPosition = Vector3.zero;
                        nodeObj.transform.localRotation = Quaternion.identity;
                        nodeObj.transform.localScale = Vector3.one;
                        asset.BodyObjects.Add(nodeObj);

                        if (isSteeringWheel)
                        {
                            asset.SteeringWheelTransforms.Add(nodeObj.transform);
                            asset.SteeringWheelRestRotations[nodeObj.transform] = nodeObj.transform.localRotation;
                        }
                        if (isHood)
                        {
                            asset.HoodTransforms.Add(nodeObj.transform);
                            asset.HoodRestRotations[nodeObj.transform] = nodeObj.transform.localRotation;
                        }
                        if (isDoorL)
                        {
                            asset.LeftDoorTransforms.Add(nodeObj.transform);
                            asset.LeftDoorRestRotations[nodeObj.transform] = nodeObj.transform.localRotation;
                        }
                        if (isDoorR)
                        {
                            asset.RightDoorTransforms.Add(nodeObj.transform);
                            asset.RightDoorRestRotations[nodeObj.transform] = nodeObj.transform.localRotation;
                        }
                        if (isTrunk)
                        {
                            asset.TrunkTransforms.Add(nodeObj.transform);
                            asset.TrunkRestRotations[nodeObj.transform] = nodeObj.transform.localRotation;
                        }
                        if (isSpoiler)
                        {
                            asset.SpoilerTransforms.Add(nodeObj.transform);
                            asset.SpoilerRestRotations[nodeObj.transform] = nodeObj.transform.localRotation;
                        }
                        if (isSpeedNeedle)
                        {
                            asset.SpeedometerNeedles.Add(nodeObj.transform);
                            asset.GaugeRestRotations[nodeObj.transform] = nodeObj.transform.localRotation;
                        }
                        if (isRpmNeedle)
                        {
                            asset.TachometerNeedles.Add(nodeObj.transform);
                            asset.GaugeRestRotations[nodeObj.transform] = nodeObj.transform.localRotation;
                        }
                    }

                    // Mesh Primitives
                    for (int pIdx = 0; pIdx < node.Mesh.Primitives.Count; pIdx++)
                    {
                        var prim = node.Mesh.Primitives[pIdx];
                        var posAcc = prim.GetVertexAccessor("POSITION");
                        if (posAcc == null) continue;

                        var rawPos = posAcc.AsVector3Array();
                        var vertices = new Vector3[rawPos.Count];
                        for (int v = 0; v < rawPos.Count; v++)
                        {
                            var p = System.Numerics.Vector3.Transform(rawPos[v], worldMatrix);
                            if (isWheel)
                            {
                                p -= centerPivot;
                            }
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
                                var nm = System.Numerics.Vector3.Normalize(System.Numerics.Vector3.TransformNormal(rawNorm[n], normMatrix));
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

                        var rawIndices = prim.GetIndices();
                        var triangles = new int[rawIndices.Count];
                        for (int t = 0; t < rawIndices.Count; t += 3)
                        {
                            if (t + 2 < rawIndices.Count)
                            {
                                triangles[t] = (int)rawIndices[t];
                                triangles[t + 1] = (int)rawIndices[t + 2];
                                triangles[t + 2] = (int)rawIndices[t + 1];
                            }
                        }

                        var mesh = new Mesh();
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
                        if (prim.Material != null && materials.TryGetValue(prim.Material.LogicalIndex, out var mat))
                        {
                            mr.sharedMaterial = mat;
                        }
                        else if (fallbackMat != null)
                        {
                            mr.sharedMaterial = fallbackMat;
                        }
                    }
                }

                // Global model scale, center offset, ground height offset, and rotation angle
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

                Plugin.LogMsg($"[GlbVehicleLoader] Successfully loaded '{spec.DisplayName}' (Body: {asset.BodyObjects.Count}, FL: {asset.FrontLeftWheels.Count}, Calipers: {asset.FrontLeftCalipers.Count + asset.FrontRightCalipers.Count}, Doors: {asset.LeftDoorTransforms.Count + asset.RightDoorTransforms.Count}, Hood: {asset.HoodTransforms.Count}).");
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
