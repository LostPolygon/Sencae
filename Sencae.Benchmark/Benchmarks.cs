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
    private Cartridge _cartridge;
    private Machine _machine;

    public ulong Emulate(int maxCycles = 10000) {
        DebugEmulationRunner.EmulateUntilBreak(
            _machine,
            maxCycles,
            formatState: _ => "",
            outputState: _ => {
            }
        );

        return _machine.State.Cycle;
    }

    [GlobalSetup]
    public void Setup() {
        _cartridge = DebugEmulationRunner.CreateCodeCartridge(
            """

            .0000    a9 01        LDA #$01
            .0002    8d 00 02     STA $0200
            .0005    a9 05        LDA #$05
            .0007    8d 01 02     STA $0201
            .000a    a9 08        LDA #$08
            .000c    8d 02 02     STA $0202

            """
        );
        _machine = new Machine(_cartridge);
    }

    [Benchmark(Baseline = true)]
    [IterationCount(1000)]
    public void Test1() {
        Emulate();
    }
}
