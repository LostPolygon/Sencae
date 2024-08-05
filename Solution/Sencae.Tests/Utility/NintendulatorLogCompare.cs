namespace Sencae.Tests;

public static class NintendulatorLogCompare {
    public static void ResetMachineToNintendulatorInitialState(Machine machine) {
        MachineCpuMemoryBus cpuMemoryBus = machine.CpuMemoryBus;

        machine.State.Ppu.Registers.PPUCTRL = 0x0;
        machine.State.Ppu.Registers.PPUSTATUS =  0x0;
        machine.State.Cpu.Internal.Cycles = 7;
        machine.State.MachineCycles = 7 * 3;
        //cpuMemoryBus.Write(0x2000, 0xFF);
        //cpuMemoryBus.Write(0x2002, 0xFF);
        //machine.State.Ppu.OAMADDR = 0xFF;
        //machine.State.Ppu.PPUCTRL = (PpuControlRegisterFlags) 0xFF;
        //machine.State.Ppu.PPUMASK = (PpuMaskRegisterFlags) 0xFF;
        //machine.State.Ppu.PPUSTATUS = (PpuStatusRegisterFlags) 0xFF;
        /*for (ushort i = 0x2000; i <= 0x2007; i++) {
            cpuMemoryBus.Write(i, 0xFF);
        }*/
            
        cpuMemoryBus.Write(0x4017, 0xFF); // frame irq enabled
        cpuMemoryBus.Write(0x4015, 0xFF); // all APU channels disabled

        for (ushort i = 0x4000; i <= 0x400F; i++) {
            cpuMemoryBus.Write(i, 0xFF);
        }

        for (ushort i = 0x4010; i <= 0x4013; i++) {
            cpuMemoryBus.Write(i, 0xFF);
        }
        
        machine.State.Cpu.Registers.P = CpuStatusRegisterFlags.FromFlags((CpuStatusRegisterPackedFlags) 0x24);
    }
    
    public static List<string> ReadLogFromFile(string path) {
        return File.ReadAllLines(path)
            .Select(x => x.Trim())
            .ToList();
    }
    
    public static (int? mismatchLine, int? mismatchColumn, int logSize) CompareLogs(List<string> reference, List<string> actual) {
        (int? mismatchLine, int? mismatchColumn, int logSize) result = default;
        
        result.logSize = Math.Min(reference.Count, actual.Count);
        for (int i = 0; i < result.logSize; i++) {
            if (reference[i] != actual[i]) {
                int mismatchIndex = reference[i].Zip(actual[i], (c1, c2) => c1 == c2).TakeWhile(b => b).Count() + 1;
                result.mismatchLine = i;
                result.mismatchColumn = mismatchIndex;
                mismatchIndex += "[Expected] ".Length;

                Console.WriteLine($"=== Mismatch at {i + 1}:{mismatchIndex} :");
                Console.WriteLine($"[Expected] {reference[i]}");
                Console.WriteLine("^".PadLeft(mismatchIndex));
                Console.WriteLine($"[Actual]   {actual[i]}");
                Console.WriteLine("^".PadLeft(mismatchIndex));
                break;
            }
        }
        
        Console.WriteLine();

        for (int i = 0; i < result.logSize; i++) {
            Console.WriteLine(actual[i]);
        }

        return result;
    }
}
