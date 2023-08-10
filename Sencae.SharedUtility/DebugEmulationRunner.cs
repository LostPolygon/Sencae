using static Sencae.SharedUtility.AssemblerUtility;

namespace Sencae.SharedUtility;

public class DebugEmulationRunner {
    public static Cartridge CreateCodeCartridge(string assembly, ushort programRomSize = 16 * 1024) {
        byte[] assembledCode = AssembleFromListing(assembly);
        byte[] programRom = new byte[programRomSize];

        assembledCode.AsSpan().CopyTo(programRom);

        ushort resetVector = (ushort) (0xFFFF - programRomSize);
        programRom[^3] = (byte) resetVector;
        programRom[^2] = (byte) (resetVector >> 8);

        byte[] characterRom = new byte[8 * 1024];

        return new Cartridge(programRom, characterRom);
    }

    public static Machine EmulateUntilBreak(
        Cartridge cartridge,
        int maxCycles = 10000,
        ushort? overrideResetVector = null,
        Action<Machine>? afterResetAction = null,
        Func<MachineState, string>? formatState = null,
        Action<string>? outputState = null
    ) {
        Machine machine = new(cartridge);
        EmulateUntilBreak(machine, maxCycles, overrideResetVector, afterResetAction, formatState, outputState);
        
        return machine;
    }
    
    public static void EmulateUntilBreak(
        Machine machine,
        int maxCycles = 10000,
        ushort? overrideResetVector = null,
        Action<Machine>? afterResetAction = null,
        Func<MachineState, string>? formatState = null,
        Action<string>? outputState = null
    ) {
        ref MachineState state = ref machine.GetState();
        machine.Reset(overrideResetVector);
        afterResetAction?.Invoke(machine);
        
        formatState ??= MachineStateFormatter.FormatCpuState;
        outputState ??= Console.WriteLine;

        outputState(formatState(state));
        for (int i = 0; i < maxCycles; i++) {
            machine.Clock();
            outputState(formatState(state));
            if (machine.State.Cpu.Halted)
                break;
        }
    }
}
