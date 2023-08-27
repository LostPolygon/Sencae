using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Sencae.SharedUtility;

namespace Sencae.Benchmark;

[RyuJitX86Job]
[RyuJitX64Job]
/*[DisassemblyDiagnoser(
    printSource: true,
    exportHtml: true,
    printInstructionAddresses: true,
    maxDepth: 100
)]*/
//[TailCallDiagnoser(filterByNamespace: false)]
//[InliningDiagnoser(allowedNamespaces: new[] { "Sencae" })]
public class Benchmarks {
    public CartridgeDevice CartridgeDevice { get; private set; }
    public Machine Machine { get; private set; }
    //private GCHandle _machineGcHandle;

    public ulong Emulate(ulong maxCycles = 16000, Func<Machine, string>? formatState = null) {
        formatState ??= _ => "";

        DebugEmulationRunner.Emulate(
            Machine,
            new() {
                StopOnBRKOrHalt = false,
                OverrideResetVector = 0xC000,
                MaxCycles = maxCycles,
                FormatState = formatState,
                OutputState = _ => {
                }
            }
        );

        return Machine.State.CpuProgramCycles;
    }

    public ulong EmulateWithStats(ulong maxCycles = 16000) {
        Dictionary<InstructionType, ulong> opcodeStats = new();
        ulong cycles = Emulate(
            maxCycles,
            machine => {
                if (machine.State.Cpu.Internal.State == CpuInternalState.ExecutionState.InterpretOpcode) {
                    opcodeStats.TryAdd(machine.State.Cpu.Internal.Instruction.Type, 0);
                    opcodeStats[machine.State.Cpu.Internal.Instruction.Type]++;
                }

                return "";
            }
        );

        Console.WriteLine("=== stats begin");

        KeyValuePair<InstructionType, ulong>[] sortedOpcodeStats = opcodeStats.OrderByDescending(kv => kv.Value).ToArray();
        for (int i = 0; i < sortedOpcodeStats.Length; i++) {
            if (sortedOpcodeStats[i].Value == 0)
                continue;

            Console.WriteLine($"{sortedOpcodeStats[i].Key}: {sortedOpcodeStats[i].Value}");
        }

        Console.WriteLine("=== stats end");

        return cycles;
    }

    [GlobalSetup]
    public void Setup() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        CartridgeDevice = CartridgeDeviceFactory.Create(cartridgeData);

        Machine = new Machine(CartridgeDevice);
        //_machineGcHandle = GCHandle.Alloc(Machine, GCHandleType.Pinned);
    }

    [GlobalCleanup]
    public void Cleanup() {
        Machine.Dispose();
        //_machineGcHandle.Free();
    }

    [Benchmark(Baseline = true)]
    [IterationCount(100)]
    public void Test1() {
        Emulate(formatState: _ => "");
    }
}
