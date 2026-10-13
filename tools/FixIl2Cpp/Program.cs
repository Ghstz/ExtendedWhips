using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using SharpGLTF.Schema2;

class Program
{
    static void Main()
    {
        string dir = @"D:\VSC Projects\ExtendedWhips\Vehicles";
        var map = new Dictionary<string, string> {
            { "plymouth_hemi_cuda_1971__www.vecarz.com.glb", "cuda71" },
            { "nissan_r34_brians_fast_furious__www.vecarz.com.glb", "r34" },
            { "2025_bmw_m4_gt3_evo_g82.glb", "bmw_m4" },
            { "spy-hypersport.glb", "hypersport" },
            { "1978_chevrolet_blazer_k5_cheyenne.glb", "blazer78" },
            { "2024_ford_f-150_raptor_r.glb", "raptor_r" },
            { "2024_gmc_sierra_1500_at4x.glb", "sierra_at4x" },
            { "ford_f-150_raptor.glb", "raptor" },
            { "gmc_sierra_hd2500.glb", "sierra_hd" },
            { "kawasaki_ninja_h2.glb", "ninja_h2" },
            { "bmw_s1000_rr.glb", "bmw_s1000" }
        };

        foreach (var kvp in map)
        {
            var model = ModelRoot.Load(Path.Combine(dir, kvp.Key));
            Console.WriteLine($"\n=== {kvp.Value}: {kvp.Key} ===");

            var wheelMeshes = new List<(Node node, Vector3 center, float radius, bool isCaliper)>();

            foreach (var node in model.LogicalNodes)
            {
                if (node.Mesh == null) continue;
                if (IsSteering(node)) continue;

                if (IsWheelOrCaliper(node, kvp.Value, out bool isCal))
                {
                    Vector3 min = new Vector3(float.MaxValue);
                    Vector3 max = new Vector3(float.MinValue);
                    foreach (var prim in node.Mesh.Primitives)
                    {
                        var acc = prim.GetVertexAccessor("POSITION");
                        if (acc == null) continue;
                        foreach (var v in acc.AsVector3Array())
                        {
                            var pt = Vector3.Transform(v, node.WorldMatrix);
                            min = Vector3.Min(min, pt);
                            max = Vector3.Max(max, pt);
                        }
                    }
                    if (min.X <= max.X)
                    {
                        var center = (min + max) * 0.5f;
                        float radius = (max - min).Length() * 0.5f;
                        wheelMeshes.Add((node, center, radius, isCal));
                    }
                }
            }

            // Cluster road wheels
            var roadWheels = wheelMeshes.Where(w => !w.isCaliper).ToList();
            var clusters = ClusterMeshes(roadWheels);
            Console.WriteLine($"  Road Wheels: {roadWheels.Count} meshes -> {clusters.Count} clusters");
            for (int i = 0; i < clusters.Count; i++)
            {
                var c = clusters[i];
                Vector3 min = new Vector3(float.MaxValue), max = new Vector3(float.MinValue);
                foreach (var m in c)
                {
                    min = Vector3.Min(min, m.center - new Vector3(m.radius * 0.5f));
                    max = Vector3.Max(max, m.center + new Vector3(m.radius * 0.5f));
                }
                var center = (min + max) * 0.5f;
                Console.WriteLine($"    Cluster {i} ({c.Count} meshes): Center=({center.X:F3}, {center.Y:F3}, {center.Z:F3})");
            }

            var calMeshes = wheelMeshes.Where(w => w.isCaliper).ToList();
            var calClusters = ClusterMeshes(calMeshes);
            Console.WriteLine($"  Calipers: {calMeshes.Count} meshes -> {calClusters.Count} clusters");
        }
    }

    static bool IsSteering(Node n)
    {
        var cur = n;
        while (cur != null)
        {
            string s = ((cur.Name ?? "") + " " + (cur.Mesh?.Name ?? "")).ToLowerInvariant();
            if (s.Contains("steer") || s.Contains("stealing") || s.Contains("volant") || s.Contains("handlebar")) return true;
            cur = cur.VisualParent;
        }
        return false;
    }

    static bool IsWheelOrCaliper(Node n, string vid, out bool isCal)
    {
        isCal = false;
        if (vid == "hypersport" && (n.LogicalIndex == 2 || n.LogicalIndex == 25)) return true;

        string s = ((n.Name ?? "") + " " + (n.Mesh?.Name ?? "")).ToLowerInvariant();
        if (s.Contains("light") || s.Contains("lamp")) return false;

        if (s.Contains("caliper") || (s.Contains("brake") && !s.Contains("brakelight") && !s.Contains("airbrake")) || s.Contains("pad"))
        {
            isCal = true; return true;
        }
        if (s.Contains("wheel") || s.Contains("tire") || s.Contains("tyre") || s.Contains("rim") || s.Contains("spoke") || s.Contains("hub"))
        {
            return true;
        }

        if (n.Mesh != null)
        {
            foreach (var p in n.Mesh.Primitives)
            {
                string pm = (p.Material?.Name ?? "").ToLowerInvariant();
                if (pm.Contains("light") || pm.Contains("lamp")) continue;
                if (pm.Contains("caliper") || (pm.Contains("brake") && !pm.Contains("brakelight") && !pm.Contains("airbrake"))) { isCal = true; return true; }
                if (pm.Contains("wheel") || pm.Contains("tire") || pm.Contains("tyre") || pm.Contains("rim")) return true;
            }
        }

        // Check ancestors
        var cur = n.VisualParent;
        while (cur != null)
        {
            string ps = (cur.Name ?? "").ToLowerInvariant();
            if (ps == "rootnode" || ps.StartsWith("rootnode_") || ps == "root" || ps == "sketchfab_model" || ps.StartsWith("sketchfab_model_")) break;
            if (ps.Contains("caliper") || (ps.Contains("brake") && !ps.Contains("brakelight") && !ps.Contains("airbrake"))) { isCal = true; return true; }
            if (ps.Contains("wheel") || ps.Contains("tire") || ps.Contains("tyre") || ps.Contains("rim") || ps.Contains("spoke") || ps.Contains("hub") || ps.Contains("disc_scale")) return true;
            cur = cur.VisualParent;
        }

        return false;
    }

    static List<List<(Node node, Vector3 center, float radius, bool isCaliper)>> ClusterMeshes(List<(Node node, Vector3 center, float radius, bool isCaliper)> meshes)
    {
        var clusters = new List<List<(Node, Vector3, float, bool)>>();
        foreach (var m in meshes)
        {
            bool added = false;
            foreach (var c in clusters)
            {
                // If centers are close relative to bounding radius
                float threshold = Math.Max(0.25f * (c[0].radius + m.radius), 0.05f);
                if (Vector3.Distance(m.center, c[0].center) < threshold)
                {
                    c.Add(m);
                    added = true;
                    break;
                }
            }
            if (!added) clusters.Add(new List<(Node, Vector3, float, bool)> { m });
        }
        return clusters;
    }
}
