using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using Sencae.SharedUtility;

namespace Sencae.Benchmark;

[Config(typeof(Config))]
public class CpuBenchmark {
    private class Config : ManualConfig {
        public Config() {
            AddJob(
                Job.Default
                    .WithPlatform(Platform.X64)
                    .WithRuntime(CoreRuntime.Core80)
                    .WithJit(Jit.RyuJit)
                    .WithEnvironmentVariables(
                        new EnvironmentVariable("DOTNET_TieredPGO", "1"),
                        new EnvironmentVariable("DOTNET_TC_QuickJitForLoops", "0")
                    )
                    .AsBaseline()
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
            );

            AddJob(
                Job.Default
                    .WithPlatform(Platform.X64)
                    .WithRuntime(NativeAotRuntime.Net80)
            );
        }
    }

    public const ulong CycleCount = 26548; // nestest.log ends here
    public const int InternalIterations = 1000;

    public MapperDevice MapperDevice { get; private set; } = null!;
    public Machine Machine { get; private set; } = null!;

    public ulong Emulate(int iterations = InternalIterations, ulong maxCycles = CycleCount) {
        DebugEmulationRunner.RunnerOptions runnerOptions = new() {
            StopOnBRKOrHalt = false,
            OverrideResetVector = 0xC000,
            MaxMachineCycles = maxCycles,
            SkipOutputState = true,
            ResetBeforeRun = false
        };

        ulong totalCycles = 0;
        for (int i = 0; i < iterations; i++) {
            Machine.CpuDevice.ApplyResetVector(0xC000);
            DebugEmulationRunner.Emulate(Machine, runnerOptions);

            totalCycles += Machine.State.Cpu.Internal.Cycles;
        }

        return totalCycles;
    }

    [GlobalSetup]
    public void Setup() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        MapperDevice = MapperDeviceFactory.Create(cartridgeData);

        Machine = new Machine(MapperDevice);
        Machine.Reset(
            new CpuResetOptions {
                OverrideResetVector = 0xC000,
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
    }

    [Benchmark]
    public void NesTest() {
        Emulate();
    }
}
