using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Sixty502DotNet.Shared;
using Environment = Sixty502DotNet.Shared.Environment;

namespace NGChip8Emulator.Benchmark {
    class Program {
        public static byte[] TestRom;

        static void Main(string[] args) {
            //TestRom = File.ReadAllBytes("Resources/BRIX_test");

            /*Benchmarks benchmarks = new Benchmarks();
            benchmarks.Test1();
            benchmarks.Test1();*/

            Summary summary = BenchmarkRunner.Run<Benchmarks>();
            Console.WriteLine(summary);

            //Test1();
            //Console.Read();
        }
    }

    [DisassemblyDiagnoser(
        printSource: true,
        exportHtml: true, 
        maxDepth: 100
        )
    ]
    [RyuJitX64Job]
    [TailCallDiagnoser(filterByNamespace: false)]
    [InliningDiagnoser(allowedNamespaces: new []{ "NGChip8Emulator" })]
    public class Benchmarks {
        private Cartridge _cartridge;
        private Emulator _emulator;

        /*public void Interpreter() {
            BenchmarkEmulator emulator = new BenchmarkEmulator();
            MachineState machineState = new MachineState();
            emulator.Reset(ref machineState);
            emulator.LoadProgram(ref machineState, Program.TestRom);

            for (int i = 0; i < 1000; i++) {
                emulator.UpdateTimers(ref machineState);
                emulator.Update(ref machineState, 10);
            }

            //Console.WriteLine($"---- Executed instructions: {machineState.ExecutedInstructionCount}");
        }*/

        private Machine Emulate(int cycles = 10000) {
            Emulator emulator = _emulator;
            Machine machine = emulator.Machine;
            
            emulator.Reset();

            //Console.WriteLine(FormatCpuState(state));
            for (int i = 0; i < cycles; i++) {
                emulator.Clock();

                //Console.WriteLine(FormatCpuState(state));
                //Console.ReadLine();
                if (machine.State.Cpu.Halted)
                    break;
            }

            return machine;
        }

        public static Cartridge CreateCodeCartridge(string assembly, ushort programRomSize = 16 * 1024) {
            byte[] assembledCode = AssembleFromListing(assembly);
            byte[] programRom = new byte[programRomSize];

            assembledCode.AsSpan().CopyTo(programRom);

            ushort resetVector = (ushort) (0xFFFF - programRomSize);
            programRom[^3] = (byte) resetVector;
            programRom[^2] = (byte) (resetVector >> 8);

            byte[] characterRom = new byte[8 * 1024];

            return new Cartridge(programRom, characterRom);
        }

        [GlobalSetup]
        public void Setup() {
            _cartridge = CreateCodeCartridge(
                """

                .0000    a9 01        LDA #$01
                .0002    8d 00 02     STA $0200
                .0005    a9 05        LDA #$05
                .0007    8d 01 02     STA $0201
                .000a    a9 08        LDA #$08
                .000c    8d 02 02     STA $0202

                """
            );
            _emulator = new(new Machine(_cartridge));
        }

        [Benchmark(Baseline = true)]
        [IterationCount(10)]
        public void Test1() {
            Machine machine = Emulate();
        }

        private static string FormatCpuState(in MachineState machine) {
            CpuState cpu = machine.Cpu;
            return
                $"==== Cycle: {machine.Cycle}, " +
                $"AB: {cpu.Internal.AddressBus.Value:X4}, " +
                $"DB: {cpu.Internal.DataBus:X2}, " +
                $"PC: {cpu.PC.Value:X4}, " +
                $"RW: {(cpu.Internal.Read ? 1 : 0)}, " +
                $"OP: {cpu.Internal.InstructionOpcode:X2}, " +
                $"A: {cpu.A:X2}, " +
                $"X: {cpu.X:X2}, " +
                $"Y: {cpu.Y:X2}, " +
                $"SP: {cpu.SP:X2}"
                ;
        }

        public static byte[] AssembleFromListing(string sourceListing) {
            Environment.Reset();
            Interpreter interpreter = new Interpreter(new Options {
                ArchitectureOptions = new ArchitectureOptions {
                    Cpuid = "6502"
                },
                GeneralOptions = new GeneralOptions {
                },
                DiagnosticOptions = new DiagnosticOptions {
                },
                OutputOptions = new OutputOptions {
                    NoDisassembly = true,
                }
            }, new FileSystemBinaryReader(null));

            string truncatedListing =
                String.Join(
                    "\n",
                    sourceListing
                        .Split("\n", StringSplitOptions.TrimEntries)
                        .Select(s => s.Trim())
                        .Select(s => {
                            Match match = Regex.Match(s, "(?<!^)([A-Z]){3}", RegexOptions.Singleline);
                            return s[match.Index..];
                        })
                ).Trim();

            AssemblyState assemblyState = interpreter.Exec(truncatedListing, new StringSourceFactory());
            assemblyState.Warnings.ForEach(w => Console.WriteLine($"[Asm Warning] {w}"));
            if (assemblyState.Errors.Any()) {
                assemblyState.Errors.ForEach(w => Console.WriteLine($"[Asm Error] {w}"));
                throw new Exception("Assembly failed");
            }

            Console.WriteLine("=== Asm Start");
            assemblyState.StatementListings
                .Select(s => {
                    Match match = Regex.Match(s, "(?<!^)([A-Z]){3}", RegexOptions.Singleline);
                    return s[..(match.Index - 34)] + s[match.Index..];
                })
                .ToList()
                .ForEach(Console.WriteLine);
            Console.WriteLine("=== Asm End");

            return assemblyState.Output.GetCompilation().ToArray();
        }
    }
}
