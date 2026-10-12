using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;

namespace ExtendedWhips.Models
{
    public class LoadedVehicleAsset
    {
        public GlbVehicleModel Spec { get; set; }
        public GameObject RootObject { get; set; }
        public List<GameObject> WheelObjects { get; set; } = new();
        public List<GameObject> BodyObjects { get; set; } = new();

        public LoadedVehicleAsset(GlbVehicleModel spec, GameObject root)
        {
            Spec = spec;
            RootObject = root;
        }
    }

    public static class GlbVehicleLoader
    {
        public static readonly List<GlbVehicleModel> Catalog = new()
        {
            // Classic & Sports Cars
            new GlbVehicleModel("cuda71", "1971 Plymouth Hemi Cuda", "plymouth_hemi_cuda_1971__www.vecarz.com.glb", VehicleTypeCategory.MuscleCar, "426 Hemi V8 Classic American Muscle", 1.0f, 0.05f, 1.0f, "StreetPerformance"),
            new GlbVehicleModel("r34", "Nissan Skyline GT-R R34", "nissan_r34_brians_fast_furious__www.vecarz.com.glb", VehicleTypeCategory.SportsCar, "Brian's Fast & Furious R34 GT-R Spec", 1.0f, 0.03f, 0.95f, "DriftSpec"),
            new GlbVehicleModel("bmw_m4", "BMW M4 GT3 Evo (2025)", "2025_bmw_m4_gt3_evo_g82.glb", VehicleTypeCategory.SportsCar, "G82 Factory GT3 Aerodynamic Track Weapon", 1.0f, -0.02f, 0.92f, "HyperInsane"),
            new GlbVehicleModel("hypersport", "Lykan Hypersport", "spy-hypersport.glb", VehicleTypeCategory.Supercar, "Twin-Turbo 3.7L Flat-6 Exotic Missile", 1.0f, -0.05f, 0.90f, "HyperInsane"),
            
            // Trucks & 4x4s
            new GlbVehicleModel("blazer78", "1978 Chevrolet K5 Blazer", "1978_chevrolet_blazer_k5_cheyenne.glb", VehicleTypeCategory.Offroad4x4, "Cheyenne K5 Full-Size 4x4 Classic", 1.05f, 0.15f, 1.15f, "OffroadLifted"),
            new GlbVehicleModel("raptor_r", "2024 Ford F-150 Raptor R", "2024_ford_f-150_raptor_r.glb", VehicleTypeCategory.Truck, "700+ HP Supercharged V8 Trophy Truck", 1.0f, 0.12f, 1.10f, "OffroadLifted"),
            new GlbVehicleModel("sierra_at4x", "2024 GMC Sierra 1500 AT4X", "2024_gmc_sierra_1500_at4x.glb", VehicleTypeCategory.Truck, "Premium Heavy Duty Overland Rig", 1.0f, 0.10f, 1.05f, "OffroadLifted"),
            new GlbVehicleModel("raptor", "Ford F-150 Raptor", "ford_f-150_raptor.glb", VehicleTypeCategory.Truck, "Twin-Turbo EcoBoost Desert Runner", 1.0f, 0.10f, 1.08f, "OffroadLifted"),
            new GlbVehicleModel("sierra_hd", "GMC Sierra 2500HD", "gmc_sierra_hd2500.glb", VehicleTypeCategory.Truck, "Heavy Duty Duramax Commercial Hauler", 1.05f, 0.14f, 1.12f, "OffroadLifted"),

            // Motorcycles
            new GlbVehicleModel("ninja_h2", "Kawasaki Ninja H2", "kawasaki_ninja_h2.glb", VehicleTypeCategory.Motorcycle, "Supercharged 998cc 200+ MPH Superbike", 1.0f, 0.0f, 0.85f, "HyperInsane", trackScale: 0.2f),
            new GlbVehicleModel("bmw_s1000", "BMW S1000RR", "bmw_s1000_rr.glb", VehicleTypeCategory.Motorcycle, "999cc Inline-4 Precision Track Superbike", 1.0f, 0.0f, 0.85f, "HyperInsane", trackScale: 0.2f)
        };

        public static string? ResolveModelPath(string fileName)
        {
            // 1. Plugin directory Vehicles folder
            string p1 = Path.Combine(Paths.PluginPath, "ExtendedWhips", "Vehicles", fileName);
            if (File.Exists(p1)) return p1;

            // 2. Development directory
            string p2 = Path.Combine(@"D:\VSC Projects\ExtendedWhips\Vehicles", fileName);
            if (File.Exists(p2)) return p2;

            // 3. Fallback game root
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

                // Determine base shader
                Shader? targetShader = null;
                if (fallbackMat != null && fallbackMat.shader != null)
                {
                    targetShader = fallbackMat.shader;
                }
                if (targetShader == null)
                {
                    targetShader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
                }

                // Materials cache
                var materials = new Dictionary<int, Material>();
                for (int i = 0; i < gltf.LogicalMaterials.Count; i++)
                {
                    var gMat = gltf.LogicalMaterials[i];
                    var uMat = targetShader != null ? new Material(targetShader) : new Material(Shader.Find("Standard"));
                    uMat.name = string.IsNullOrEmpty(gMat.Name) ? $"Mat_{i}" : gMat.Name;

                    // Set diffuse / base color
                    var pbr = gMat.FindChannel("BaseColor");
                    if (pbr != null)
                    {
                        var factor = pbr.Value.Color;
                        Color col = new Color(factor.X, factor.Y, factor.Z, factor.W);
                        if (uMat.HasProperty("_BaseColor")) uMat.SetColor("_BaseColor", col);
                        if (uMat.HasProperty("_Color")) uMat.SetColor("_Color", col);

                        if (pbr.Value.Texture != null && pbr.Value.Texture.PrimaryImage != null)
                        {
                            int imgIdx = pbr.Value.Texture.PrimaryImage.LogicalIndex;
                            if (textures.TryGetValue(imgIdx, out var tex))
                            {
                                if (uMat.HasProperty("_BaseColorMap")) uMat.SetTexture("_BaseColorMap", tex);
                                uMat.mainTexture = tex;
                            }
                        }
                    }

                    materials[i] = uMat;
                }

                // Create GameObjects for nodes & meshes
                foreach (var node in gltf.LogicalNodes)
                {
                    if (node.Mesh == null) continue;

                    string nodeName = string.IsNullOrEmpty(node.Name) ? $"Node_{node.LogicalIndex}" : node.Name;
                    var nodeObj = new GameObject(nodeName);
                    nodeObj.transform.SetParent(rootObj.transform, false);

                    // Transform
                    var localPos = node.LocalTransform.Translation;
                    var localRot = node.LocalTransform.Rotation;
                    var localScale = node.LocalTransform.Scale;

                    nodeObj.transform.localPosition = new Vector3(-localPos.X, localPos.Y, localPos.Z);
                    nodeObj.transform.localRotation = new Quaternion(-localRot.X, localRot.Y, localRot.Z, -localRot.W);
                    nodeObj.transform.localScale = new Vector3(localScale.X, localScale.Y, localScale.Z);

                    bool isWheel = nodeName.Contains("wheel", StringComparison.OrdinalIgnoreCase) ||
                                  nodeName.Contains("tire", StringComparison.OrdinalIgnoreCase) ||
                                  nodeName.Contains("rim", StringComparison.OrdinalIgnoreCase);

                    if (isWheel) asset.WheelObjects.Add(nodeObj);
                    else asset.BodyObjects.Add(nodeObj);

                    // Mesh Primitives
                    foreach (var prim in node.Mesh.Primitives)
                    {
                        var posAcc = prim.GetVertexAccessor("POSITION");
                        if (posAcc == null) continue;

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

                        var rawIndices = prim.GetIndices();
                        var triangles = new int[rawIndices.Count];
                        for (int t = 0; t < rawIndices.Count; t += 3)
                        {
                            if (t + 2 < rawIndices.Count)
                            {
                                triangles[t] = (int)rawIndices[t];
                                triangles[t + 1] = (int)rawIndices[t + 2]; // Flip winding for inverted X
                                triangles[t + 2] = (int)rawIndices[t + 1];
                            }
                        }

                        var mesh = new Mesh();
                        mesh.name = $"{nodeName}_prim";
                        mesh.vertices = vertices;
                        if (normals != null) mesh.normals = normals;
                        if (uvs != null) mesh.uv = uvs;
                        mesh.triangles = triangles;

                        if (normals == null) mesh.RecalculateNormals();
                        mesh.RecalculateBounds();

                        var mf = nodeObj.AddComponent<MeshFilter>();
                        mf.sharedMesh = mesh;

                        var mr = nodeObj.AddComponent<MeshRenderer>();
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

                // Global model scale & height offset
                rootObj.transform.localScale = Vector3.one * spec.ModelScale;
                rootObj.transform.localPosition = new Vector3(0f, spec.HeightOffset, 0f);

                Plugin.LogMsg($"[GlbVehicleLoader] Successfully loaded '{spec.DisplayName}' ({asset.BodyObjects.Count} body nodes, {asset.WheelObjects.Count} wheel nodes).");
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
