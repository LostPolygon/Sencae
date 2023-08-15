using NUnit.Framework;
using static Sencae.SharedUtility.MachineStateFormatter;
using static Sencae.SharedUtility.DebugEmulationRunner;

namespace Sencae.Tests;

public class NesTestRomTest {
    [Test]
    public void NesTest() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        CartridgeDevice cartridgeDevice = new CartridgeDevice(cartridgeData.PrgRom, cartridgeData.ChrRom, new byte[8 * 1024]);

        List<string> logOutput = new();

        bool firstStateSet = false;
        MachineState oldMachineState = default;

        Exception? emulationException = null;
        try {
            Machine machine = Emulate(
                cartridgeDevice, new() {
                    StopOnBRK = false,
                    MaxCycles = 50000,
                    OverrideResetVector = 0xC000,
                    FormatState = FormatState,
                    OutputState = s => {
                    },
                    AfterResetAction = machine => {
                        // This is not 100% correct, but nestest.log uses it
                        machine.State.Cpu.P = (CpuStatusRegisterFlags) 0x24;
                        
                        // nestest expects a certain (incorrect) state on powerup
                        machine.CpuMemoryBus.GetU8(0x4004) = 0xFF;
                        machine.CpuMemoryBus.GetU8(0x4005) = 0xFF;
                        machine.CpuMemoryBus.GetU8(0x4006) = 0xFF;
                        machine.CpuMemoryBus.GetU8(0x4007) = 0xFF;
                        machine.CpuMemoryBus.GetU8(0x4015) = 0xFF;
                    }
                });
        } catch (Exception e) {
            if (e is AssertionException)
                throw;

            emulationException = e;
            Console.WriteLine("Exception: " + e);
        }

        List<string> referenceLogOutput =
            File.ReadAllLines("Resources/nestest_cpuonly_nocycles.log")
                .Select(x => x.Trim())
                .ToList();

        int logSize = Math.Min(referenceLogOutput.Count, logOutput.Count);
        for (int i = 0; i < logSize; i++) {
            if (referenceLogOutput[i] != logOutput[i]) {
                int mismatchIndex = referenceLogOutput[i].Zip(logOutput[i], (c1, c2) => c1 == c2).TakeWhile(b => b).Count() + 1;
                mismatchIndex += "[Expected] ".Length;

                Console.WriteLine($"=== Mismatch at {i + 1}:{mismatchIndex} :");
                Console.WriteLine($"[Expected] {referenceLogOutput[i]}");
                Console.WriteLine("^".PadLeft(mismatchIndex));
                Console.WriteLine($"[Actual]   {logOutput[i]}");
                Console.WriteLine("^".PadLeft(mismatchIndex));
                break;
            }
        }

        Console.WriteLine();

        for (int i = 0; i < logSize; i++) {
            Console.WriteLine(logOutput[i]);
        }
        
        Assert.Null(emulationException);

        return;

        string FormatState(Machine machine) {
            MachineState machineState = machine.State;
            if (!firstStateSet) {
                firstStateSet = true;
                oldMachineState = machineState;
            }

            string text = "";
            if (machineState.Cpu.Internal.State != CpuInternalState.ExecutionState.InterpretOpcode)
                return text;

            machineState.Cpu.Internal.Instruction = 
                Sencae.Instructions.DescriptorByOpcode[machine.CpuMemoryBus.GetU8(machineState.Cpu.PC.Value)];

            text = FormatCpuStateNesTest(machine, machineState).Trim();
            logOutput.Add(text);

            // Console.WriteLine(text);

            return text;
        }
    }

    private static string FormatCpuStateNesTest(Machine machine, MachineState machineState) {
        machine = new Machine(machine.CartridgeDevice) {
            State = machineState,
            CpuMemoryBus = machine.CpuMemoryBus
        };
        const int cycleOffset = 7;

        CpuState cpu = machineState.Cpu;
        InstructionDescriptor instruction = cpu.Internal.Instruction;

        string hexDump = "";
        byte instructionOpcodeSize = instruction.OpcodeSize;
        for (int i = 0; i < instructionOpcodeSize; i++) {
            byte data = machine.CpuMemoryBus.GetU8((ushort) (machineState.Cpu.PC.Value + i));
            hexDump += $"{data:X2} ";
        }

        hexDump = hexDump.PadRight(9, ' ');
        hexDump += (instruction.Flags & InstructionFlags.Unofficial) != 0 ? "*" : " ";

        string asmDump = instruction.Type switch {
            InstructionType.ISC => "ISB",
            _ => $"{instruction.Type}"
        };
        asmDump += " ";
        byte dataP1 = machine.CpuMemoryBus.GetU8((ushort) (machineState.Cpu.PC.Value + 1));
        UInt16Bytes dataU16 = default;
        dataU16.Low = dataP1;

        string oldAsmDump = asmDump;
        switch (instruction.AddressingMode) {
            case AddressingMode.Implied:
                if (instruction.Type is InstructionType.LSR or InstructionType.ASL or InstructionType.ROR or InstructionType.ROL) {
                    asmDump += "A";
                }

                break;
            case AddressingMode.Immediate:
                asmDump += $"#${dataP1:X2}";
                break;
            case AddressingMode.Absolute: {
                dataU16.High = machine.CpuMemoryBus.GetU8((ushort) (machineState.Cpu.PC.Value + 2));
                byte targetData = machine.CpuMemoryBus.GetU8(dataU16.Value);
                asmDump += $"${dataU16.Value:X4}";
                if (!instruction.Flags.HasFlag(InstructionFlags.FetchesNextOpcode)) {
                    asmDump += $" = {targetData:X2}";
                }

                break;
            }
            case AddressingMode.AbsoluteXIndexed:
            case AddressingMode.AbsoluteYIndexed: {
                string indexName;
                byte index;
                switch (instruction.AddressingMode) {
                    case AddressingMode.AbsoluteXIndexed:
                        index = cpu.X;
                        indexName = "X";
                        break;
                    case AddressingMode.AbsoluteYIndexed:
                        index = cpu.Y;
                        indexName = "Y";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                dataU16.High = machine.CpuMemoryBus.GetU8((ushort) (machineState.Cpu.PC.Value + 2));
                ushort effectiveAddress = (ushort) (dataU16.Value + index);
                byte targetData = machine.CpuMemoryBus.GetU8(effectiveAddress);
                asmDump += $"${dataU16.Value:X4},{indexName} @ {effectiveAddress:X4}";
                if (true || instruction.Flags.HasFlag(InstructionFlags.FetchesNextOpcode)) {
                    asmDump += $" = {targetData:X2}";
                }

                break;
            }
            case AddressingMode.ZeroPage: {
                byte targetData = machine.CpuMemoryBus.GetU8(dataP1);
                asmDump += $"${dataP1:X2} = {targetData:X2}";
                break;
            }
            case AddressingMode.ZeroPageXIndexed:
            case AddressingMode.ZeroPageYIndexed: {
                string indexName;
                byte index;
                switch (instruction.AddressingMode) {
                    case AddressingMode.ZeroPageXIndexed:
                        index = cpu.X;
                        indexName = "X";
                        break;
                    case AddressingMode.ZeroPageYIndexed:
                        index = cpu.Y;
                        indexName = "Y";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ushort effectiveAddress = (byte) (dataP1 + index);
                byte targetData = machine.CpuMemoryBus.GetU8(effectiveAddress);
                asmDump += $"${dataP1:X2},{indexName} @ {effectiveAddress:X2} = {targetData:X2}";
                break;
            }
            case AddressingMode.AbsoluteIndirect: {
                dataU16.High = machine.CpuMemoryBus.GetU8((ushort) (machineState.Cpu.PC.Value + 2));
                UInt16Bytes effectiveAddressAddress = dataU16;
                bool pageBoundaryBug = effectiveAddressAddress.Low == 0xFF;

                UInt16Bytes effectiveAddress = default;
                effectiveAddress.Low = machine.CpuMemoryBus.GetU8(effectiveAddressAddress.Value);
                effectiveAddress.High = machine.CpuMemoryBus.GetU8(
                    (ushort) (pageBoundaryBug ?
                        effectiveAddressAddress.Value & 0xFF00 :
                        effectiveAddressAddress.Value + 1)
                );

                asmDump += $"(${effectiveAddressAddress.Value:X4}) = {effectiveAddress.Value:X4}";
                break;
            }
            case AddressingMode.ZeroPageIndirectXPreIndexed: {
                ushort effectiveAddressAddress = (byte) (dataP1 + cpu.X);
                UInt16Bytes effectiveAddress = new() {
                    Low = machine.CpuMemoryBus.GetU8((byte) effectiveAddressAddress),
                    High = machine.CpuMemoryBus.GetU8((byte) (effectiveAddressAddress + 1)),
                };

                byte targetData = machine.CpuMemoryBus.GetU8(effectiveAddress.Value);

                asmDump += $"(${dataP1:X2},X) @ {effectiveAddressAddress:X2} = {effectiveAddress.Value:X4} = {targetData:X2}";
                break;
            }
            case AddressingMode.ZeroPageIndirectYPostIndexed: {
                ushort effectiveAddressAddress = dataP1;
                UInt16Bytes effectiveAddress = new() {
                    Low = machine.CpuMemoryBus.GetU8((byte) effectiveAddressAddress),
                    High = machine.CpuMemoryBus.GetU8((byte) (effectiveAddressAddress + 1)),
                };
                ushort effectiveAddressIndexed = (ushort) (effectiveAddress.Value + cpu.Y);
                byte targetData = machine.CpuMemoryBus.GetU8(effectiveAddressIndexed);

                asmDump += $"(${dataP1:X2}),Y = {effectiveAddress.Value:X4} @ {effectiveAddressIndexed:X4} = {targetData:X2}";
                break;
            }
            case AddressingMode.Relative: {
                // +2 for instruction size itself
                ushort effectiveAddress = (ushort) (machineState.Cpu.PC.Value + (sbyte) dataP1 + 2);
                asmDump += $"${effectiveAddress:X4}";
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (asmDump == oldAsmDump && instruction.AddressingMode != AddressingMode.Implied) {
            asmDump += "? " + instruction.AddressingMode;
        }

        asmDump = asmDump.PadRight(32, ' ');

        return
            $"{cpu.PC.Value:X4}  " +
            hexDump +
            asmDump +
            $"A:{cpu.A:X2} " +
            $"X:{cpu.X:X2} " +
            $"Y:{cpu.Y:X2} " +
            $"P:{(int) cpu.P:X2} " +
            $"SP:{cpu.SP:X2} " +
            //$"CYC:{machineState.Cycle + cycleOffset} " +
            ""
            ;
    }
}
