using System.IO;
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

    public ulong Emulate(int maxCycles = 10000) {
        DebugEmulationRunner.Emulate(
            Machine,
            new () {
                MaxCycles = maxCycles,
                FormatState = _ => "",
                OutputState = _ => {}
            }
        );

        return Machine.State.Cycles;
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
