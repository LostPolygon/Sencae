using Sixty502DotNet.Shared;
using static Sencae.SharedUtility.AssemblerUtility;

namespace Sencae.SharedUtility;

public static class DebugEmulationRunner {
    public static CartridgeDevice CreateCodeCartridge(
        string assembly,
        ushort programRomSize = 16 * 1024,
        ushort programRamSize = 4 * 1024
    ) {
        CodeOutput codeOutput = AssembleFromListing(assembly);
        byte[] assembledCode = codeOutput.GetCompilation().ToArray();
        byte[] programRom = new byte[programRomSize];

        assembledCode.AsSpan().CopyTo(programRom);

        ushort resetVector = (ushort) (0xFFFF + 1 - programRomSize);
        programRom[^4] = (byte) resetVector;
        programRom[^3] = (byte) (resetVector >> 8);

        byte[] characterRom = new byte[8 * 1024];
        byte[] programRam = new byte[programRamSize];

        CartridgeDevice cartridgeDevice = new(programRom, characterRom, programRam);
        return cartridgeDevice;
    }

    public static Machine EmulateUntilBreak(CartridgeDevice cartridgeDevice, RunnerOptions? options = null) {
        Machine machine = new(cartridgeDevice);
        EmulateUntilBreak(machine, options);

        return machine;
    }

    public static void EmulateUntilBreak(Machine machine, RunnerOptions? options = null) {
        options ??= new RunnerOptions();

        machine.Reset(options.OverrideResetVector);
        options.AfterResetAction?.Invoke(machine);

        Func<Machine, string> formatState = options.FormatState ?? (m => MachineStateFormatter.FormatCpuState(m.State));
        Action<string> outputState = options.OutputState ?? Console.WriteLine;

        outputState(formatState(machine));
        for (int i = 0; i < options.MaxCycles; i++) {
            machine.Clock();
            outputState(formatState(machine));
            if (options.StopOnBRK &&
                machine.State.Cpu.Internal.Instruction.Type == InstructionType.BRK &&
                machine.State.Cpu.Internal.State == CpuInternalState.ExecutionState.ExecutingInstruction) {
                break;
            }
        }
    }

    public record RunnerOptions {
        public bool StopOnBRK { get; init; } = true;
        public int MaxCycles { get; set; } = 10000;
        public ushort? OverrideResetVector { get; init; }
        public Action<Machine>? AfterResetAction { get; init; }
        public Func<Machine, string>? FormatState { get; init; }
        public Action<string>? OutputState { get; init; }
    };
}
