using System;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Running;

namespace Sencae.Benchmark;

internal static class Program {
    public static byte[] TestRom;

    public static ulong TotalCycles;

    static void Main(string[] args) {
        if (args.Contains("-runner")) {
            BenchmarkRunner.Run<Benchmarks>();
        } else {
            using Process p = Process.GetCurrentProcess();
            p.PriorityClass = ProcessPriorityClass.High;

            Benchmarks benchmarks = new();
            benchmarks.Setup();

            Console.WriteLine("Warming up...");
            for (int i = 0; i < 500; i++) {
                benchmarks.Emulate();
            }

            Console.WriteLine("Starting benchmark...");

            const int iterations = 25000;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) {
                TotalCycles += benchmarks.Emulate();
            }
            
            benchmarks.EmulateWithStats();

            sw.Stop();

            Console.WriteLine(
                $"{iterations} iterations took {sw.Elapsed}, " +
                $"cycles: {TotalCycles}, " +
                $"speed: {TotalCycles / sw.Elapsed.TotalSeconds / 1000 / 1000:0.##} megacycles/sec");
        }
    }
}
