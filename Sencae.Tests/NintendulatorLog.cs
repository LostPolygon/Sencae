namespace Sencae.Tests;

public static class NintendulatorLog {
    public static void ResetMachineToNintendulatorInitialState(Machine machine) {
        CpuMemoryBus cpuMemoryBus = machine.CpuMemoryBus;
            
        for (ushort i = 0x2000; i <= 0x2007; i++) {
            cpuMemoryBus.Write(i, 0xFF);
        }
            
        cpuMemoryBus.Write(0x4017, 0xFF); // frame irq enabled
        cpuMemoryBus.Write(0x4015, 0xFF); // all APU channels disabled

        for (ushort i = 0x4000; i <= 0x400F; i++) {
            cpuMemoryBus.Write(i, 0xFF);
        }

        for (ushort i = 0x4010; i <= 0x4013; i++) {
            cpuMemoryBus.Write(i, 0xFF);
        }
    }
    
    public static List<string> ReadLogFromFile(string path) {
        return File.ReadAllLines(path)
            .Select(x => x.Trim())
            .ToList();
    }
    
    public static (int? mismatchLine, int? mismatchColumn, int logSize) CompareLogs(List<string> reference, List<string> actual) {
        (int? mismatchLine, int? mismatchColumn, int logSize) result = default;
        
        result.logSize = Math.Min(reference.Count, actual.Count);
        for (int i = 0; i < result.logSize; i++) {
            if (reference[i] != actual[i]) {
                int mismatchIndex = reference[i].Zip(actual[i], (c1, c2) => c1 == c2).TakeWhile(b => b).Count() + 1;
                result.mismatchLine = i;
                result.mismatchColumn = mismatchIndex;
                mismatchIndex += "[Expected] ".Length;

                Console.WriteLine($"=== Mismatch at {i + 1}:{mismatchIndex} :");
                Console.WriteLine($"[Expected] {reference[i]}");
                Console.WriteLine("^".PadLeft(mismatchIndex));
                Console.WriteLine($"[Actual]   {actual[i]}");
                Console.WriteLine("^".PadLeft(mismatchIndex));
                break;
            }
        }
        
        Console.WriteLine();

        for (int i = 0; i < result.logSize; i++) {
            Console.WriteLine(actual[i]);
        }

        return result;
    }
    
    public static void FormatState(Machine machine, List<string> logOutput) {
        string? text = FormatState(machine);
        if (!String.IsNullOrWhiteSpace(text)) {
            logOutput.Add(text);
        }
    }

    public static string? FormatState(Machine machine) {
        MachineState machineState = machine.State;
        if (machineState.Cpu.Internal.State != CpuInternalState.ExecutionState.InterpretOpcode)
            return null;

        machineState.Cpu.Internal.Instruction =
            Sencae.Instructions.DescriptorByOpcode[machine.CpuMemoryBus.Read(machineState.Cpu.PC.Value)];

        string text = FormatCpuStateNesTest(machine, machineState).Trim();
        return text;
    }

    private static string FormatCpuStateNesTest(Machine machine, MachineState machineState) {
        /*machine = new Machine(machine.CartridgeDevice) {
            State = machineState,
            CpuMemoryBus = machine.CpuMemoryBus
        };*/

        CpuState cpu = machineState.Cpu;
        InstructionDescriptor instruction = cpu.Internal.Instruction;

        string hexDump = "";
        byte instructionOpcodeSize = instruction.OpcodeSize;
        for (int i = 0; i < instructionOpcodeSize; i++) {
            byte data = machine.CpuMemoryBus.Read((ushort) (machineState.Cpu.PC.Value + i));
            hexDump += $"{data:X2} ";
        }

        hexDump = hexDump.PadRight(9, ' ');
        hexDump += (instruction.Flags & InstructionFlags.Unofficial) != 0 ? "*" : " ";

        string asmDump = instruction.Type switch {
            InstructionType.ISC => "ISB",
            _ => $"{instruction.Type}"
        };
        asmDump += " ";
        byte dataP1 = machine.CpuMemoryBus.Read((ushort) (machineState.Cpu.PC.Value + 1));
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
                dataU16.High = machine.CpuMemoryBus.Read((ushort) (machineState.Cpu.PC.Value + 2));
                byte targetData = machine.CpuMemoryBus.Read(dataU16.Value);
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

                dataU16.High = machine.CpuMemoryBus.Read((ushort) (machineState.Cpu.PC.Value + 2));
                ushort effectiveAddress = (ushort) (dataU16.Value + index);
                byte targetData = machine.CpuMemoryBus.Read(effectiveAddress);
                asmDump += $"${dataU16.Value:X4},{indexName} @ {effectiveAddress:X4}";
                if (true || instruction.Flags.HasFlag(InstructionFlags.FetchesNextOpcode)) {
                    asmDump += $" = {targetData:X2}";
                }

                break;
            }
            case AddressingMode.ZeroPage: {
                byte targetData = machine.CpuMemoryBus.Read(dataP1);
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
                byte targetData = machine.CpuMemoryBus.Read(effectiveAddress);
                asmDump += $"${dataP1:X2},{indexName} @ {effectiveAddress:X2} = {targetData:X2}";
                break;
            }
            case AddressingMode.AbsoluteIndirect: {
                dataU16.High = machine.CpuMemoryBus.Read((ushort) (machineState.Cpu.PC.Value + 2));
                UInt16Bytes effectiveAddressAddress = dataU16;
                bool pageBoundaryBug = effectiveAddressAddress.Low == 0xFF;

                UInt16Bytes effectiveAddress = default;
                effectiveAddress.Low = machine.CpuMemoryBus.Read(effectiveAddressAddress.Value);
                effectiveAddress.High = machine.CpuMemoryBus.Read(
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
                    Low = machine.CpuMemoryBus.Read((byte) effectiveAddressAddress),
                    High = machine.CpuMemoryBus.Read((byte) (effectiveAddressAddress + 1)),
                };

                byte targetData = machine.CpuMemoryBus.Read(effectiveAddress.Value);

                asmDump += $"(${dataP1:X2},X) @ {effectiveAddressAddress:X2} = {effectiveAddress.Value:X4} = {targetData:X2}";
                break;
            }
            case AddressingMode.ZeroPageIndirectYPostIndexed: {
                ushort effectiveAddressAddress = dataP1;
                UInt16Bytes effectiveAddress = new() {
                    Low = machine.CpuMemoryBus.Read((byte) effectiveAddressAddress),
                    High = machine.CpuMemoryBus.Read((byte) (effectiveAddressAddress + 1)),
                };
                ushort effectiveAddressIndexed = (ushort) (effectiveAddress.Value + cpu.Y);
                byte targetData = machine.CpuMemoryBus.Read(effectiveAddressIndexed);

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
            $"{cpu.Internal.AddressBus.Value:X4}  " +
            hexDump +
            asmDump +
            $"A:{cpu.A:X2} " +
            $"X:{cpu.X:X2} " +
            $"Y:{cpu.Y:X2} " +
            $"P:{(int) cpu.P.ToFlags():X2} " +
            $"SP:{cpu.SP:X2} " +
            $"CYC:{machineState.CpuCycles} " +
            ""
            ;
    }
}
