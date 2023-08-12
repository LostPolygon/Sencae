using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Sencae.SharedUtility;

namespace Sencae.Benchmark;

[DisassemblyDiagnoser(
    printSource: true,
    exportHtml: true,
    printInstructionAddresses: true,
    maxDepth: 100
)]
[RyuJitX64Job]
[TailCallDiagnoser(filterByNamespace: false)]
[InliningDiagnoser(allowedNamespaces: new[] { "Sencae" })]
public class Benchmarks {
    public CartridgeDevice CartridgeDevice { get; private set; }
    public Machine Machine { get; private set; }

    public ulong Emulate(ulong maxCycles = 16000, Func<Machine, string>? formatState = null) {
        formatState ??= _ => "";

        DebugEmulationRunner.Emulate(
            Machine,
            new() {
                StopOnBRK = false,
                OverrideResetVector = 0xC000,
                MaxCycles = maxCycles,
                FormatState = formatState,
                OutputState = _ => {
                }
            }
        );

        return Machine.State.Cycles;
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
        CartridgeDevice = new CartridgeDevice(cartridgeData.PrgRom, cartridgeData.ChrRom, new byte[0]);

        /*CartridgeDevice = DebugEmulationRunner.CreateCodeCartridge(
            """

            .0000    a9 01        LDA #$01
            .0002    8d 00 02     STA $0200
            .0005    a9 05        LDA #$05
            .0007    8d 01 02     STA $0201
            .000a    a9 08        LDA #$08
            .000c    8d 02 02     STA $0202

            """
        );*/
        Machine = new Machine(CartridgeDevice);
    }

    [Benchmark(Baseline = true)]
    [IterationCount(1000)]
    public void Test1() {
        Emulate();
    }
}
