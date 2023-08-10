using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public static class InstructionLogic {
    const int afterAddressingFinished = -1;
    const int afterNextOpcodeFetched = 100;

    #region Transfer Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool LDA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.A = cpuInternal.DataBus;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.X = cpuInternal.DataBus;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.Y = cpuInternal.DataBus;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.A;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.X;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.Y;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TAX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.X = cpu.A;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TAY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.Y = cpu.A;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TSX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.X = cpu.SP;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TXA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.A = cpu.X;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TXS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.SP = cpu.X;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TYA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.A = cpu.Y;
                break;
        }

        return false;
    }

    #endregion

    #region Stack Instructions

    #endregion

    #region Decrements & Increments

    [MethodImpl(MethodOptions.Options)]
    public static bool DEX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return DEX_DEY_Internal(operationCycle, ref cpu, ref cpuInternal, ref cpu.X);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool DEY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return DEX_DEY_Internal(operationCycle, ref cpu, ref cpuInternal, ref cpu.Y);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool INX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return INX_INY_Internal(operationCycle, ref cpu, ref cpuInternal, ref cpu.X);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool INY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return INX_INY_Internal(operationCycle, ref cpu, ref cpuInternal, ref cpu.Y);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool INX_INY_Internal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, ref byte value) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                value++;

                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (value & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);

                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool DEX_DEY_Internal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, ref byte value) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                value--;

                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (value & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);

                break;
        }

        return false;
    }

    #endregion

    #region Arithmetic Operations

    [MethodImpl(MethodOptions.Options)]
    public static bool ADC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                byte data = cpuInternal.DataBus;
                ushort result16 = (ushort) (cpu.A + data + (cpu.P.HasFlag(CpuStatusRegisterFlags.Carry) ? 1 : 0));
                byte result = (byte) result16;

                cpu.P.SetFlag(CpuStatusRegisterFlags.Carry, result16 > byte.MaxValue);
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpu.X & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, cpu.X == 0);

                bool overflow = ((cpu.A ^ result) & (data ^ result) & 1 << 7) != 0;

                cpu.P.SetFlag(CpuStatusRegisterFlags.Overflow, overflow);
                cpu.A = result;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SBC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                byte data = (byte) ~cpuInternal.DataBus;
                ushort result16 = (ushort) (cpu.A + data + (cpu.P.HasFlag(CpuStatusRegisterFlags.Carry) ? 1 : 0));
                byte result = (byte) result16;

                cpu.P.SetFlag(CpuStatusRegisterFlags.Carry, result16 > byte.MaxValue);
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpu.X & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, cpu.X == 0);

                bool overflow = ((cpu.A ^ result) & (data ^ result) & 1 << 7) != 0;

                cpu.P.SetFlag(CpuStatusRegisterFlags.Overflow, overflow);
                cpu.A = result;
                break;
        }

        return false;
    }

    #endregion

    #region Logical Operations

    [MethodImpl(MethodOptions.Options)]
    public static bool AND(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.A &= cpu.Internal.DataBus;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool EOR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.A ^= cpu.Internal.DataBus;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ORA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.A |= cpu.Internal.DataBus;
                break;
        }

        return false;
    }

    #endregion

    #region Shift & Rotate Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool LSR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return RmwWithImpliedInternal(operationCycle, ref cpu, ref cpuInternal, b => (byte) (b >> 1));
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ASL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return RmwWithImpliedInternal(operationCycle, ref cpu, ref cpuInternal, b => (byte) (b << 1));
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool DEC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.Temporary.Value = (ushort) (cpu.Internal.DataBus - 1);
                break;
            case 1:
                cpu.Internal.DataBus = (byte) cpuInternal.Temporary;
                return true;
            case afterNextOpcodeFetched:
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpuInternal.Temporary & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, cpuInternal.Temporary == 0);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool INC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.Temporary.Value = (ushort) (cpu.Internal.DataBus - 1);
                break;
            case 1:
                cpu.Internal.DataBus = (byte) cpuInternal.Temporary;
                return true;
            case afterNextOpcodeFetched:
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpuInternal.Temporary & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, cpuInternal.Temporary == 0);
                break;
        }

        return false;
    }

    private static bool RmwWithImpliedInternal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, Func<byte, byte> operation) {
        if (cpuInternal.Instruction.AddressingMode == AddressingMode.Implied) {
            switch (operationCycle) {
                case afterAddressingFinished:
                    return true;
                case afterNextOpcodeFetched:
                    byte input = cpu.A;
                    cpu.A = operation(input);
                    cpu.P.RemoveFlag(CpuStatusRegisterFlags.NegativeOrSign);
                    cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, cpu.A == 0);
                    cpu.P.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 0) != 0);
                    /*cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpu.A & 1 << 7) != 0);
                    if (cpu.A == 0) {
                        cpu.P.SetFlag(CpuStatusRegisterFlags.Zero);
                    } else {
                        cpu.P.RemoveFlag(CpuStatusRegisterFlags.Zero);
                        cpu.P.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 7) != 0);
                    }*/

                    break;
            }
        } else {
            switch (operationCycle) {
                case afterAddressingFinished:
                    break;
                case 0:
                    cpuInternal.Read = false;
                    cpuInternal.Temporary = operation(cpu.Internal.DataBus);
                    break;
                case 1:
                    cpu.Internal.DataBus = (byte) cpuInternal.Temporary;
                    return true;
                case afterNextOpcodeFetched:
                    break;
            }
        }

        return false;
    }

    #endregion

    #region Flag Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool CLC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.P.RemoveFlag(CpuStatusRegisterFlags.Carry);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLD(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.P.RemoveFlag(CpuStatusRegisterFlags.DecimalMode);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.P.RemoveFlag(CpuStatusRegisterFlags.InterruptDisable);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLV(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.P.RemoveFlag(CpuStatusRegisterFlags.Overflow);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SEC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.P.SetFlag(CpuStatusRegisterFlags.Carry);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SED(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.P.SetFlag(CpuStatusRegisterFlags.DecimalMode);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SEI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.P.SetFlag(CpuStatusRegisterFlags.InterruptDisable);
                break;
        }

        return false;
    }

    #endregion

    #region Comparisons

    [MethodImpl(MethodOptions.Options)]
    public static bool CMP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, ref cpu, ref cpuInternal, ref cpu.A);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CPX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, ref cpu, ref cpuInternal, ref cpu.X);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CPY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, ref cpu, ref cpuInternal, ref cpu.Y);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool CMP_Internal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, ref byte comparedValue) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                byte result = (byte) (comparedValue - cpuInternal.DataBus);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Carry, cpuInternal.DataBus <= comparedValue);
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (result & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, result == 0);
                break;
        }

        return false;
    }

    #endregion

    #region Conditional Branch Instructions

    public static bool BCS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlag(CpuStatusRegisterFlags.Carry));
    }

    public static bool BCC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.HasFlag(CpuStatusRegisterFlags.Carry));
    }

    public static bool BEQ(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlag(CpuStatusRegisterFlags.Zero));
    }

    public static bool BNE(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.HasFlag(CpuStatusRegisterFlags.Zero));
    }

    public static bool BMI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlag(CpuStatusRegisterFlags.NegativeOrSign));
    }

    public static bool BPL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.HasFlag(CpuStatusRegisterFlags.NegativeOrSign));
    }

    public static bool BVS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlag(CpuStatusRegisterFlags.Overflow));
    }

    public static bool BVC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.HasFlag(CpuStatusRegisterFlags.Overflow));
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool BranchInternal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, bool condition) {
        switch (operationCycle) {
            case afterAddressingFinished:
                break;
            case 0:
                if (condition) {
                    cpu.PC.Low = (byte) (cpu.PC.Low + cpuInternal.DataBus);
                }

                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                return true;
            case 1:
                //cpu.PC.Value++;
                //cpuInternal.AddressBus = cpu.PC;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    #endregion

    #region Jumps & Subroutines

    [MethodImpl(MethodOptions.Options)]
    public static bool JMP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpu.PC = cpu.Internal.AddressBus;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool JSR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        ushort stackAddress = (ushort) (0x100 + cpu.SP);
        switch (operationCycle) {
            case afterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.Temporary = cpuInternal.DataBus;
                cpuInternal.AddressBus = stackAddress;
                cpu.PC.Value++;
                break;
            case 1:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = stackAddress;
                cpuInternal.DataBus = cpu.PC.High;
                cpu.SP--;
                break;
            case 2:
                cpuInternal.AddressBus.Value--;
                cpuInternal.DataBus = cpu.PC.Low;
                cpu.SP--;
                break;
            case 3:
                cpuInternal.Read = true;
                cpuInternal.AddressBus.Value = cpu.PC;
                break;
            case 4:
                UInt16Bytes subroutineAddress = new() {
                    High = cpuInternal.DataBus,
                    Low = (byte) cpuInternal.Temporary
                };

                cpuInternal.Read = true;
                cpuInternal.AddressBus = subroutineAddress;
                cpu.PC = cpu.Internal.AddressBus;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool RTS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        ushort stackAddress = (ushort) (0x100 + cpu.SP);
        switch (operationCycle) {
            case afterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.AddressBus = stackAddress;
                break;
            case 1:
                cpuInternal.AddressBus.Value++;
                cpu.SP++;
                break;
            case 2:
                cpu.PC.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value++;
                cpu.SP++;
                break;
            case 3:
                cpu.PC.High = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = cpu.PC;

                //cpu.PC++;
                return true;
                break;
            case 4:
            //cpuInternal.AddressBus.Value = cpu.PC;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    #endregion

    #region Interrupts

    [MethodImpl(MethodOptions.Options)]
    public static bool BRK(int operationCycle, ref MachineState machineState, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        if (machineState.Cycle > 0) {
            cpu.Halted = true;
        }

        return true;
    }

    #endregion

    #region Other

    [MethodImpl(MethodOptions.Options)]
    public static bool BIT(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                byte result = (byte) (cpu.A & cpuInternal.DataBus);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Carry, cpuInternal.DataBus <= cpu.A);
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpuInternal.DataBus & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Overflow, (cpuInternal.DataBus & 1 << 6) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, result == 0);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool NOP(int operationCycle, ref MachineState machineState, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    #endregion

    #region Unofficial

    [MethodImpl(MethodOptions.Options)]
    public static bool XXX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        throw new Exception($"!!! Unhandled opcode ({cpu.Internal.Instruction})");
        return true;
    }

    #endregion
}
