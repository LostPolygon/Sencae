using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace NGChip8Emulator.Benchmark {
    class Program {
        public static byte[] TestRom;

        static void Main(string[] args) {
            TestRom = File.ReadAllBytes("Resources/BRIX_test");

            Summary summary = BenchmarkRunner.Run<Benchmarks>();
            Console.WriteLine(summary);
            Console.Read();
        }
    }

    public class Benchmarks {
        [Benchmark(Baseline = true)]
        public void Interpreter() {
            /*BenchmarkEmulator emulator = new BenchmarkEmulator();
            MachineState machineState = new MachineState();
            emulator.Reset(ref machineState);
            emulator.LoadProgram(ref machineState, Program.TestRom);

            for (int i = 0; i < 1000; i++) {
                emulator.UpdateTimers(ref machineState);
                emulator.Update(ref machineState, 10);
            }
*/
            //Console.WriteLine($"---- Executed instructions: {machineState.ExecutedInstructionCount}");
        }
    }
}
