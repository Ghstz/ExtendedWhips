using System;
using System.IO;
using System.Linq;
using Mono.Cecil;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1 || !Directory.Exists(args[0]))
        {
            Console.Error.WriteLine("usage: FixIl2Cpp <path-to-BepInEx/interop>");
            return;
        }
        string interopDir = args[0];
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(interopDir);
        var readerParams = new ReaderParameters { AssemblyResolver = resolver };
        var wbAsm = AssemblyDefinition.ReadAssembly(Path.Combine(interopDir, "Wb.GameScripts.dll"), readerParams);

        Console.WriteLine("=== VehiclePhysics Input enums ===");
        foreach (var t in wbAsm.MainModule.Types.Where(t => t.Namespace == "VehiclePhysics" && t.Name.Contains("Input")))
        {
            Console.WriteLine($"Type: {t.Name}");
            foreach (var f in t.Fields) Console.WriteLine($"  Field: {f.Name}");
        }
    }
}
