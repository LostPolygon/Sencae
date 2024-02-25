using Sixty502DotNet.Shared;
using static Sencae.SharedUtility.AssemblerUtility;

namespace Sencae.SharedUtility;

public static class DebugEmulationRunner {
    public static CartridgeDevice CreateCodeCartridge(
        string assembly,
        ushort programRomSize = 32 * 1024,
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

        CartridgeData cartridgeData = new(0, programRom, characterRom, true);
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

        if (options.ResetBeforeRun) {
            machine.Reset(
                new CpuResetOptions {
                    OverrideResetVector = options.OverrideResetVector,
                    CpuMemoryResetState = CpuMemoryResetState.AllZero
                }
            );
        }

        options.AfterResetAction?.Invoke(machine);

        Func<Machine, string> formatState = default!;
        Action<string> outputState = default!;
        if (!options.SkipOutputState) {
            formatState = options.FormatState ?? (m => MachineStateFormatter.FormatCpuState(m.State));
            outputState = options.OutputState ?? GetDefaultOutputMethod();
        }

        for (ulong i = 0; i < options.MaxMachineCycles; i++) {
            if (!options.SkipOutputState) {
                outputState(formatState(machine));
            }

            if (options.CpuOnly) {
                machine.CpuClock();
            } else {
                machine.Clock();
            }

            if (options.StopOnBRKOrHalt) {
                ref CpuInternalState cpuInternal = ref machine.State.Cpu.Internal;
                bool shouldBreak =
                    (cpuInternal.Instruction.Type == InstructionType.BRK &&
                        cpuInternal.State == CpuInternalState.ExecutionState.ExecutingInstruction) ||
                    cpuInternal.Halted;

                if (shouldBreak)
                    break;
            }
        }
    }

    private static Action<string> GetDefaultOutputMethod() {
        if (CIUtility.IsRunningUnderCI) {
            return _ => {
            };
        }
        
        return Console.WriteLine;
    }

    public record RunnerOptions {
        public bool CpuOnly { get; init; } = true;
        public bool ResetBeforeRun { get; init; } = true;
        public bool StopOnBRKOrHalt { get; init; } = true;
        public ulong MaxMachineCycles { get; set; } = 10000;
        public ushort? OverrideResetVector { get; init; }
        public Action<Machine>? AfterResetAction { get; init; }
        public bool SkipOutputState { get; init; }
        public Func<Machine, string>? FormatState { get; init; }
        public Action<string>? OutputState { get; init; }
    };
}
