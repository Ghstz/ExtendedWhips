using System;
using System.IO;
using LibCpp2IL;
using AssetRipper.Primitives;

class Program
{
    static void Main()
    {
        string gameDir = @"D:\SteamLibrary\steamapps\common\Construction Simulator";
        string assemblyPath = Path.Combine(gameDir, "GameAssembly.dll");
        string metadataPath = Path.Combine(gameDir, "ConSim_Data", "il2cpp_data", "Metadata", "global-metadata.dat");

        var metadataBytes = File.ReadAllBytes(metadataPath);
        var binaryBytes = File.ReadAllBytes(assemblyPath);

        LibCpp2IlMain.Initialize(binaryBytes, metadataBytes, new UnityVersion(2020, 3, 49));

        Console.WriteLine("Searching for string literal 'super size factor'...");
        for (int i = 0; i < LibCpp2IlMain.TheMetadata.stringLiterals.Length; i++)
        {
            var str = LibCpp2IlMain.TheMetadata.GetStringLiteralFromIndex((uint)i);
            if (str != null && str.Contains("super size factor"))
            {
                Console.WriteLine($"Found string literal index {i}: '{str}'");
                // Check string literal usages
                var usages = LibCpp2IlMain.TheMetadata.GetMetadataUsageList((uint)i);
                Console.WriteLine($"Usages: {usages?.Count ?? 0}");
            }
        }
    }
}
