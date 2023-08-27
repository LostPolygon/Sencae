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

        CartridgeData cartridgeData = new(0, programRom, characterRom);
        CartridgeDevice cartridgeDevice = CartridgeDeviceFactory.Create(cartridgeData, programRam);
        return cartridgeDevice;
    }

    public static Machine Emulate(CartridgeDevice cartridgeDevice, RunnerOptions? options = null) {
        Machine machine = new(cartridgeDevice);
        Emulate(machine, options);

        return machine;
    }

    public static void Emulate(Machine machine, RunnerOptions? options = null) {
        options ??= new RunnerOptions();

        machine.Reset(options.OverrideResetVector);
        options.AfterResetAction?.Invoke(machine);

        Func<Machine, string> formatState = options.FormatState ?? (m => MachineStateFormatter.FormatCpuState(m.State));
        Action<string> outputState = options.OutputState ?? Console.WriteLine;

        outputState(formatState(machine));
        for (ulong i = 0; i < options.MaxCycles; i++) {
            machine.CpuClock();
            outputState(formatState(machine));

            ref CpuInternalState cpuInternal = ref machine.State.Cpu.Internal;
            bool shouldBreak =
                (cpuInternal.Instruction.Type == InstructionType.BRK &&
                    cpuInternal.State == CpuInternalState.ExecutionState.ExecutingInstruction) ||
                cpuInternal.Halted;
            if (options.StopOnBRKOrHalt && shouldBreak) {
                break;
            }
        }
    }

    public record RunnerOptions {
        public bool StopOnBRKOrHalt { get; init; } = true;
        public ulong MaxCycles { get; set; } = 10000;
        public ushort? OverrideResetVector { get; init; }
        public Action<Machine>? AfterResetAction { get; init; }
        public Func<Machine, string>? FormatState { get; init; }
        public Action<string>? OutputState { get; init; }
    };
}
