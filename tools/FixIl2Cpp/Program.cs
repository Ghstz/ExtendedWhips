using System;
using System.IO;
using System.Linq;
using Mono.Cecil;

class Program
{
    static void Main()
    {
        string interopDir = @"D:\SteamLibrary\steamapps\common\Construction Simulator\BepInEx\interop";
        string coreDir = @"D:\SteamLibrary\steamapps\common\Construction Simulator\BepInEx\core";
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(interopDir);
        resolver.AddSearchDirectory(coreDir);
        var readerParams = new ReaderParameters { AssemblyResolver = resolver };

        var asm = AssemblyDefinition.ReadAssembly(Path.Combine(interopDir, "Wb.GameScripts.dll"), readerParams);
        var vp = asm.MainModule.Types.First(t => t.Name == "VPVehicleController");

        foreach (var t in asm.MainModule.Types.Where(t => t.IsEnum && (t.Name.Contains("Driven") || t.Name.Contains("Differential") || t.Name.Contains("Clutch") || t.Name.Contains("Gearbox"))))
        {
            Console.WriteLine($"\nEnum: {t.FullName}");
            foreach (var f in t.Fields.Where(f => f.IsStatic))
            {
                Console.WriteLine($"  {f.Name} = {f.Constant}");
            }
        }

        var wheelType = asm.MainModule.Types.FirstOrDefault(t => t.Name == "VPWheelCollider");
        if (wheelType != null)
        {
            Console.WriteLine($"\n==========================================");
            Console.WriteLine($" VPWheelCollider: {wheelType.FullName}");
            Console.WriteLine($"==========================================");
            foreach (var p in wheelType.Properties)
            {
                Console.WriteLine($"  prop:  {p.PropertyType.Name} {p.Name}");
            }
        }

        var tireType = asm.MainModule.Types.FirstOrDefault(t => t.FullName.Contains("TireFriction/Settings") || t.Name.Contains("TireFriction"));
        if (tireType != null)
        {
            Console.WriteLine($"\n==========================================");
            Console.WriteLine($" TireFriction: {tireType.FullName}");
            Console.WriteLine($"==========================================");
            foreach (var p in tireType.Properties)
            {
                Console.WriteLine($"  prop:  {p.PropertyType.Name} {p.Name}");
            }
        }
    }
}
