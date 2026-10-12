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
        var imgAsm = AssemblyDefinition.ReadAssembly(Path.Combine(interopDir, "UnityEngine.ImageConversionModule.dll"), readerParams);
        var imgType = imgAsm.MainModule.Types.First(t => t.Name == "ImageConversion");
        Console.WriteLine("=== ImageConversion methods ===");
        foreach (var m in imgType.Methods)
        {
            Console.WriteLine($"  {m.Name}({string.Join(", ", m.Parameters.Select(p => p.ParameterType.Name))})");
        }
    }
}
