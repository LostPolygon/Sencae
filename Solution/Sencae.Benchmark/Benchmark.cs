using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

namespace Sencae.Benchmark;

[Config(typeof(Config))]
public class Benchmark {
    public class Config : ManualConfig {
        public static bool AddNativeAotJob;

        public Config() {
            /*AddDiagnoser(new InliningDiagnoser(
                allowedNamespaces: ["Sencae"]
            ));*/
            
            AddJob(
                Job.Default
                    .WithPlatform(Platform.X64)
                    .WithRuntime(CoreRuntime.Core80)
                    .WithJit(Jit.RyuJit)
                    .AsBaseline()
            );

            /*AddJob(
                Job.Default
                    .WithPlatform(Platform.X64)
                    .WithRuntime(CoreRuntime.Core80)
                    .WithJit(Jit.RyuJit)
                    .WithEnvironmentVariables(
                        new EnvironmentVariable("DOTNET_TieredPGO", "1"),
                        new EnvironmentVariable("DOTNET_TC_QuickJitForLoops", "0")
                    )
                    //.AsBaseline()
            );

            AddJob(
                Job.Default
                    .WithPlatform(Platform.X64)
                    .WithRuntime(CoreRuntime.Core80)
                    .WithJit(Jit.RyuJit)
                    .WithEnvironmentVariables(
                        new EnvironmentVariable("DOTNET_TieredPGO", "0"),
                        new EnvironmentVariable("DOTNET_TC_QuickJitForLoops", "1")
                    )
            );*/

            if (AddNativeAotJob) {
                AddJob(
                    Job.Default
                        .WithPlatform(Platform.X64)
                        .WithRuntime(NativeAotRuntime.Net80)
                );
            }

            AddColumn(new AverageFpsColumn());
            AddColumn(new RealtimeMultiplierColumn());
        }
    }

    public MapperDevice MapperDevice { get; private set; } = null!;
    public static Machine Machine { get; private set; } = null!;

    public ulong Emulate(ulong maxCycles) {
        for (ulong i = 0; i < maxCycles; i++) {
            Machine.Clock();
        }

        return Machine.State.MachineCycles;
    }

    [GlobalSetup]
    public void Setup() {
        byte[] testRom = File.ReadAllBytes("Resources/Contra (U)_autodemo.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        MapperDevice = MapperDeviceFactory.Create(cartridgeData);

        Machine = new Machine(MapperDevice);
        Machine.Reset(
            new CpuResetOptions {
                CpuMemoryResetState = CpuMemoryResetState.Uninitialized
            }
        );
    }

    [GlobalCleanup]
    public void Cleanup() {
        Machine.Dispose();
    }

    [IterationSetup]
    public void IterationSetup() {
        Machine.Reset(
            new CpuResetOptions {
                CpuMemoryResetState = CpuMemoryResetState.Uninitialized
            },
            false
        );
    }

    public const ulong CyclesPerFrame = 29780 * 3;
    public const ulong FramesPerSecond = 60;
    public const ulong TotalFrames = FramesPerSecond * 30;
    public const ulong TotalCycleCount = TotalFrames * CyclesPerFrame; // ~60s of gameplay

    [Benchmark]
    public void ContraAutodemo() {
        Emulate(TotalCycleCount);
    }

    private class AverageFpsColumn() : IColumn {
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) {
            if (summary == null || benchmarkCase == null)
                return "N/A";

            BenchmarkReport report = summary[benchmarkCase];
            double meanSeconds = TimeInterval.FromNanoseconds(report.ResultStatistics!.Mean).ToSeconds();
            double averageFps = TotalFrames / meanSeconds;
            return $"{averageFps:###.#}";
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public bool IsAvailable(Summary summary) => true;

        public string Id => $"{nameof(AverageFpsColumn)}.{ColumnName}";
        public string ColumnName => "Average FPS";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Metric;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => $"Custom '{ColumnName}' column";

        public override string ToString() => ColumnName;
    }

    private class RealtimeMultiplierColumn() : IColumn {
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) {
            if (summary == null || benchmarkCase == null)
                return "N/A";

            BenchmarkReport report = summary[benchmarkCase];
            double meanSeconds = TimeInterval.FromNanoseconds(report.ResultStatistics!.Mean).ToSeconds();
            double realtimeX = TotalFrames / meanSeconds / FramesPerSecond;
            return $"{realtimeX:###.#}";
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public bool IsAvailable(Summary summary) => true;

        public string Id => $"{nameof(AverageFpsColumn)}.{ColumnName}";
        public string ColumnName => "Realtime mult.";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Metric;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => $"Custom '{ColumnName}' column";

        public override string ToString() => ColumnName;
    }
}
