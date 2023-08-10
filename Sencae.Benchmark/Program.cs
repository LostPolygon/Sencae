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
            Benchmarks benchmarks = new();
            benchmarks.Setup();

            const int iterations = 20000000;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++) {
                benchmarks.Test1();
            }

            sw.Stop();

            Console.WriteLine($"{iterations} tooks {sw.Elapsed}, cycles: {TotalCycles}, speed: {TotalCycles / sw.Elapsed.TotalSeconds / 1000} kilocycles/sec");
        }
    }
}