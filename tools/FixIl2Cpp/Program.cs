using System;
using System.IO;
using System.Collections.Generic;
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

        ulong targetVA = 0x18025B830;
        Console.WriteLine($"Searching for all CALL instructions to 0x{targetVA:X} in binary...");

        // A relative CALL in x64 is opcode E8 followed by 4-byte signed displacement:
        // displacement = targetVA - (callInstrVA + 5)
        // callInstrVA + 5 + displacement = targetVA
        int matches = 0;
        for (int i = 0; i < binaryBytes.Length - 5; i++)
        {
            if (binaryBytes[i] == 0xE8)
            {
                int disp = BitConverter.ToInt32(binaryBytes, i + 1);
                // Calculate target address assuming binary offset to VA mapping
                // Let's check if (callerVA + 5 + disp) == targetVA
                // But callerVA depends on section. Let's check sections.
            }
        }
    }
}
