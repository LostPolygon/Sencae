using System.Runtime.CompilerServices;
using Sencae.Mappers;
using Sencae.Ppu;
using Sixty502DotNet.Shared;
using static Sencae.SharedUtility.AssemblerUtility;

namespace Sencae.SharedUtility;

public static class DebugEmulationRunner {
    public static MapperDevice CreateCodeCartridge(
        string assembly,
        ushort programRomSize = 32 * 1024,
        ushort programRamSize = 4 * 1024
    ) {
        CodeOutput codeOutput = AssembleFromListing(assembly);
        byte[] assembledCode = codeOutput.GetCompilation().ToArray();
        byte[] programRom = new byte[programRomSize];
        byte[] programRam = new byte[programRamSize];
        byte[] characterRam = new byte[8 * 1024];

        assembledCode.AsSpan().CopyTo(programRom);

        ushort resetVector = (ushort) (0xFFFF + 1 - programRomSize);
        programRom[^4] = (byte) resetVector;
        programRom[^3] = (byte) (resetVector >> 8);

        CartridgeData cartridgeData = new(0, programRom, null, characterRam, PpuNametableMirroringType.TwoScreenVerticalMirroring);
        MapperDevice mapperDevice = new(
            new Mapper000CartridgeToCpuBusDeviceWriteable(cartridgeData.PrgRom, programRam),
            new Mapper000.CartridgeToPpuBusDevice(cartridgeData.ChrRom, cartridgeData.ChrRam, cartridgeData.PpuNametableMirroringType)
        );

        return mapperDevice;
    }

    public static Machine Emulate(MapperDevice mapperDevice, RunnerOptions? options = null) {
        Machine machine = new(mapperDevice);
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
    }

    private readonly struct Mapper000CartridgeToCpuBusDeviceWriteable(byte[] prgRom, byte[]? prgRam = null) : IMapperDevice {
        private readonly DirectArray<byte> _prgRomMemory = new(prgRom, 0x8000);
        private readonly DirectArray<byte>? _prgRamMemory = DirectArray.FromNullable(prgRam, 0x6000);

        public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            return address switch {
                >= 0x6000 and < 0x8000 => BusDeviceHandlers.ReadWrite(_prgRamMemory, address, accessMode, ref bus),
                >= 0x8000 and <= 0xFFFF => BusDeviceHandlers.ReadWrite(_prgRomMemory, address, accessMode, ref bus),
                _ => false
            };
        }

        public void Reset() {
            _prgRamMemory?.AsSpan().Clear();
        }

        public void Dispose() {
            _prgRomMemory.Dispose();
            _prgRamMemory?.Dispose();
        }
    }
}
