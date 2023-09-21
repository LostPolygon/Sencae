using System;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Running;

namespace Sencae.Benchmark;

internal static class Program {
    public static ulong TotalCycles;

    static void Main(string[] args) {
        if (args.Contains("-runner")) {
            BenchmarkRunner.Run<CpuBenchmark>();
        } else {
            using Process p = Process.GetCurrentProcess();
            p.PriorityClass = ProcessPriorityClass.High;

            CpuBenchmark cpuBenchmark = new();
            cpuBenchmark.Setup();

            Console.WriteLine("Warming up...");
            for (int i = 0; i < 600; i++) {
                cpuBenchmark.Emulate();
            }

            Console.WriteLine("Starting benchmark...");

            const int iterations = 25000;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) {
                ulong cycles = cpuBenchmark.Emulate();
                TotalCycles += cycles;
            }

            sw.Stop();

            Console.WriteLine(
                $"{iterations} iterations took {sw.Elapsed}, " +
                $"cycles: {TotalCycles}, " +
                $"speed: {TotalCycles / sw.Elapsed.TotalSeconds / 1000 / 1000:0.##} megacycles/sec");
        }
    }
}
