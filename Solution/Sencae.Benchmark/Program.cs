using System;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Running;

namespace Sencae.Benchmark;

internal static class Program {
    static void Main(string[] args) {
        if (args.Contains("-runner")) {
            Benchmark.Config.AddNativeAotJob = args.Contains("-aot");
            BenchmarkRunner.Run<Benchmark>();
        } else {
            using Process p = Process.GetCurrentProcess();
            p.PriorityClass = ProcessPriorityClass.High;

            Benchmark benchmark = new();
            benchmark.Setup();

            Console.WriteLine("Warming up...");
            for (int i = 0; i < 2; i++) {
                benchmark.Emulate(Benchmark.TotalCycleCount);
            }

            Console.WriteLine("Starting benchmark...");

            const ulong iterations = Benchmark.TotalCycleCount * 20;
            Stopwatch sw = Stopwatch.StartNew();
            ulong totalCycles = 0;
            totalCycles += benchmark.Emulate(iterations);

            sw.Stop();

            Console.WriteLine(
                $"{iterations} iterations took {sw.Elapsed}, " +
                $"cycles: {totalCycles}, " +
                $"speed: {totalCycles / sw.Elapsed.TotalSeconds / 1000 / 1000:0.##} megacycles/sec");
        }
    }
}
