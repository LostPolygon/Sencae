using System.Diagnostics;
using System.Globalization;
using FastEnumToString;

namespace Sencae.SharedUtility;

public static class NintendulatorLog {
    public static void FormatState(Machine machine, List<string> logOutput) {
        string? text = FormatState(machine);
        if (!String.IsNullOrWhiteSpace(text)) {
            logOutput.Add(text);
        }
    }

    public static bool IsInstructionStartCycle(Machine machine) =>
        machine.State.Cpu.Internal.State == CpuInternalState.ExecutionState.InterpretOpcode;

    public static string? FormatState(Machine machine) {
        MachineState machineState = machine.State;
        if (!IsInstructionStartCycle(machine))
            return null;

        machineState.Cpu.Internal.Instruction =
            Instructions.DescriptorByOpcode[machine.CpuMemoryBus.Read(machineState.Cpu.PC.Value)];

        MachineLogEntry logEntry = CreateMachineLogEntry(machine);
        return FormatMachineLogEntry(logEntry);
    }

    public static void FormatMachineLogEntry(MachineLogEntry machineLogEntry, Span<char> destination, out int charsWritten) {
        const int maxLength = 92;
        if (destination.Length < maxLength)
            throw new ArgumentException("Length < " + maxLength, nameof(destination));

        Span<char> result = destination[..maxLength];
        result.Fill(' ');

        MachineLogEntry.CpuState cpu = machineLogEntry.Cpu;
        InstructionDescriptor instruction = cpu.Instruction;

        CultureInfo culture = CultureInfo.InvariantCulture;
        int cursor = 0;

        void AppendText(Span<char> result, ReadOnlySpan<char> text) {
            text.CopyTo(result[cursor..]);
            cursor += text.Length;
        }

        void AppendValue<T>(Span<char> result, T value, ReadOnlySpan<char> format) where T : struct, ISpanFormattable {
            bool success = value.TryFormat(result[cursor..], out int charsWritten, format, culture);
            Debug.Assert(success, "success");
            cursor += charsWritten;
        }

        void AppendHex2<T>(Span<char> result, T value) where T : struct, ISpanFormattable {
            AppendValue(result, value, "X2");
        }

        void AppendHex4<T>(Span<char> result, T value) where T : struct, ISpanFormattable {
            AppendValue(result, value, "X4");
        }

        AppendHex4(result, cpu.AddressBus);
        cursor += 2;

        int hexDumpStart = cursor;

        AppendHex2(result, cpu.Instruction.Opcode);
        cursor += 1;
        switch (instruction.OpcodeSize) {
            case 1:
                break;
            case 2:
                AppendHex2(result, cpu.OpcodeData.Low);
                break;
            case 3:
                AppendHex2(result, cpu.OpcodeData.Low);
                cursor += 1;
                AppendHex2(result, cpu.OpcodeData.High);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        cursor = hexDumpStart + 9;
        AppendText(result, (instruction.Flags & InstructionFlags.Unofficial) != 0 ? "*" : " ");

        int asmDumpStart = cursor;
        switch (instruction.Type) {
            case InstructionType.ISC:
                AppendText(result, "ISB");
                break;
            default:
                AppendText(result, instruction.Type.FastToString());
                break;
        }

        cursor += 1;

        switch (instruction.AddressingMode) {
            case AddressingMode.Implied:
                if (instruction.Type is
                    InstructionType.LSR or InstructionType.ASL or InstructionType.ROR or InstructionType.ROL) {
                    AppendText(result, "A");
                }

                break;
            case AddressingMode.Immediate:
                AppendText(result, "#$");
                AppendHex2(result, cpu.OpcodeData.Low);
                break;
            case AddressingMode.Absolute: {
                AppendText(result, "$");
                AppendHex4(result, cpu.OpcodeData.Value);
                if (!instruction.Flags.HasFlag(InstructionFlags.FetchesNextOpcode)) {
                    AppendText(result, " = ");
                    AppendHex2(result, cpu.TargetData);
                }

                break;
            }
            case AddressingMode.AbsoluteXIndexed:
            case AddressingMode.AbsoluteYIndexed: {
                string indexName = instruction.AddressingMode switch {
                    AddressingMode.AbsoluteXIndexed => "X",
                    AddressingMode.AbsoluteYIndexed => "Y",
                    _ => throw new ArgumentOutOfRangeException()
                };

                AppendText(result, "$");
                AppendHex4(result, cpu.OpcodeData.Value);
                AppendText(result, ",");
                AppendText(result, indexName);
                AppendText(result, " @ ");
                AppendHex4(result, cpu.EffectiveAddress);
                AppendText(result, " = ");
                AppendHex2(result, cpu.TargetData);

                break;
            }
            case AddressingMode.ZeroPage: {
                AppendText(result, "$");
                AppendHex2(result, cpu.OpcodeData.Low);
                AppendText(result, " = ");
                AppendHex2(result, cpu.TargetData);
                break;
            }
            case AddressingMode.ZeroPageXIndexed:
            case AddressingMode.ZeroPageYIndexed: {
                string indexName = instruction.AddressingMode switch {
                    AddressingMode.ZeroPageXIndexed => "X",
                    AddressingMode.ZeroPageYIndexed => "Y",
                    _ => throw new ArgumentOutOfRangeException()
                };

                AppendText(result, "$");
                AppendHex2(result, cpu.OpcodeData.Low);
                AppendText(result, ",");
                AppendText(result, indexName);
                AppendText(result, " @ ");
                AppendHex2(result, cpu.EffectiveAddress);
                AppendText(result, " = ");
                AppendHex2(result, cpu.TargetData);
                break;
            }
            case AddressingMode.AbsoluteIndirect: {
                AppendText(result, "($");
                AppendHex4(result, cpu.EffectiveAddressAddress);
                AppendText(result, ") = ");
                AppendHex4(result, cpu.EffectiveAddress);
                break;
            }
            case AddressingMode.ZeroPageIndirectXPreIndexed: {
                AppendText(result, "($");
                AppendHex2(result, cpu.OpcodeData.Low);
                AppendText(result, ",X) @ ");
                AppendHex2(result, cpu.EffectiveAddressAddress);
                AppendText(result, " = ");
                AppendHex4(result, cpu.EffectiveAddress);
                AppendText(result, " = ");
                AppendHex2(result, cpu.TargetData);
                break;
            }
            case AddressingMode.ZeroPageIndirectYPostIndexed: {
                AppendText(result, "($");
                AppendHex2(result, cpu.OpcodeData.Low);
                AppendText(result, "),Y = ");
                AppendHex4(result, cpu.EffectiveAddress);
                AppendText(result, " @ ");
                AppendHex4(result, cpu.EffectiveAddressIndexed);
                AppendText(result, " = ");
                AppendHex2(result, cpu.TargetData);
                break;
            }
            case AddressingMode.Relative: {
                AppendText(result, "$");
                AppendHex4(result, cpu.EffectiveAddress);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        cursor = asmDumpStart + 32;
        
        // @formatter:off
        AppendText(result, "A:"); AppendHex2(result, cpu.A); cursor += 1;
        AppendText(result, "X:"); AppendHex2(result, cpu.X); cursor += 1;
        AppendText(result, "Y:"); AppendHex2(result, cpu.Y); cursor += 1;
        AppendText(result, "P:"); AppendHex2(result, (int) cpu.P); cursor += 1;
        AppendText(result, "SP:"); AppendHex2(result, cpu.SP); cursor += 1;
        AppendText(result, "CYC:"); AppendValue(result, cpu.CpuCycles, "0");
        // @formatter:on

        charsWritten = cursor;
    }

    public static string FormatMachineLogEntry(MachineLogEntry machineLogEntry) {
        Span<char> result = stackalloc char[92];
        FormatMachineLogEntry(machineLogEntry, result, out int charsWritten);

        return new String(result[..charsWritten]);
    }

    public static MachineLogEntry CreateMachineLogEntry(Machine machine) {
        MachineState machineState = machine.State;
        CpuState cpu = machineState.Cpu;
        InstructionDescriptor instruction = cpu.Internal.Instruction;

        MachineLogEntry.CpuState cpuState = new(
            cpu.Internal.AddressBus.Value,
            cpu.A,
            cpu.X,
            cpu.Y,
            cpu.P.ToFlags(),
            cpu.SP,
            instruction,
            default,
            machineState.CpuCycles,
            0,
            0,
            0,
            0
        );

        byte Read(int address) {
            return machine.CpuMemoryBus.Read((ushort) address);
        }
        
        byte dataP1 = Read(machineState.Cpu.PC.Value + 1);
        UInt16Bytes dataU16 = default;
        dataU16.Low = dataP1;

        switch (instruction.AddressingMode) {
            case AddressingMode.Implied:
                break;
            case AddressingMode.Immediate:
                break;
            case AddressingMode.Absolute: {
                dataU16.High = Read(machineState.Cpu.PC.Value + 2);
                byte targetData = Read(dataU16.Value);
                cpuState = cpuState with {
                    TargetData = targetData
                };

                break;
            }
            case AddressingMode.AbsoluteXIndexed:
            case AddressingMode.AbsoluteYIndexed: {
                byte index = instruction.AddressingMode switch {
                    AddressingMode.AbsoluteXIndexed => cpu.X,
                    AddressingMode.AbsoluteYIndexed => cpu.Y,
                    _ => throw new ArgumentOutOfRangeException()
                };

                dataU16.High = Read(machineState.Cpu.PC.Value + 2);
                ushort effectiveAddress = (ushort) (dataU16.Value + index);
                byte targetData = Read(effectiveAddress);

                cpuState = cpuState with {
                    TargetData = targetData,
                    EffectiveAddress = effectiveAddress
                };

                break;
            }
            case AddressingMode.ZeroPage: {
                byte targetData = Read(dataP1);
                cpuState = cpuState with {
                    TargetData = targetData
                };

                break;
            }
            case AddressingMode.ZeroPageXIndexed:
            case AddressingMode.ZeroPageYIndexed: {
                byte index = instruction.AddressingMode switch {
                    AddressingMode.ZeroPageXIndexed => cpu.X,
                    AddressingMode.ZeroPageYIndexed => cpu.Y,
                    _ => throw new ArgumentOutOfRangeException()
                };

                ushort effectiveAddress = (byte) (dataP1 + index);
                byte targetData = Read(effectiveAddress);
                cpuState = cpuState with {
                    TargetData = targetData,
                    EffectiveAddress = effectiveAddress
                };

                break;
            }
            case AddressingMode.AbsoluteIndirect: {
                dataU16.High = Read(machineState.Cpu.PC.Value + 2);
                UInt16Bytes effectiveAddressAddress = dataU16;
                bool pageBoundaryBug = effectiveAddressAddress.Low == 0xFF;

                UInt16Bytes effectiveAddress = default;
                effectiveAddress.Low = Read(effectiveAddressAddress.Value);
                effectiveAddress.High = Read(
                    (ushort) (pageBoundaryBug ?
                        effectiveAddressAddress.Value & 0xFF00 :
                        effectiveAddressAddress.Value + 1)
                );

                cpuState = cpuState with {
                    EffectiveAddressAddress = effectiveAddressAddress.Value,
                    EffectiveAddress = effectiveAddress.Value
                };

                break;
            }
            case AddressingMode.ZeroPageIndirectXPreIndexed: {
                byte effectiveAddressAddress = (byte) (dataP1 + cpu.X);
                UInt16Bytes effectiveAddress = new() {
                    Low = Read(effectiveAddressAddress),
                    High = Read((byte) (effectiveAddressAddress + 1)),
                };

                byte targetData = Read(effectiveAddress.Value);
                cpuState = cpuState with {
                    EffectiveAddressAddress = effectiveAddressAddress,
                    EffectiveAddress = effectiveAddress.Value,
                    TargetData = targetData
                };

                break;
            }
            case AddressingMode.ZeroPageIndirectYPostIndexed: {
                byte effectiveAddressAddress = dataP1;
                UInt16Bytes effectiveAddress = new() {
                    Low = Read(effectiveAddressAddress),
                    High = Read((byte) (effectiveAddressAddress + 1)),
                };
                ushort effectiveAddressIndexed = (ushort) (effectiveAddress.Value + cpu.Y);
                byte targetData = Read(effectiveAddressIndexed);

                cpuState = cpuState with {
                    EffectiveAddress = effectiveAddress.Value,
                    EffectiveAddressIndexed = effectiveAddressIndexed,
                    TargetData = targetData
                };

                break;
            }
            case AddressingMode.Relative: {
                // +2 for instruction size itself
                ushort effectiveAddress = (ushort) (machineState.Cpu.PC.Value + (sbyte) dataP1 + 2);
                cpuState = cpuState with {
                    EffectiveAddress = effectiveAddress,
                };
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        cpuState = cpuState with {
            OpcodeData = dataU16
        };

        return new MachineLogEntry(cpuState);
    }
}