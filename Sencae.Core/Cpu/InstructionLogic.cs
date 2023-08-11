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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);
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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.Y);
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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);
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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.Y);
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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);
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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }

    #endregion

    #region Stack Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool PHA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                //cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.A;
                cpu.SP--;

                //cpu.PC.Value++;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PLA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                //cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 0:
                cpu.SP++;
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
                break;
            case 1:
                cpuInternal.AddressBus = cpu.PC;
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                return true;
            case afterNextOpcodeFetched:
                cpu.A = cpuInternal.Temporary.Low;

                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PHP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                //cpu.PC.Value++;
                //cpuInternal.AddressBus = cpu.PC;
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);

                CpuStatusRegisterFlags statusFlags = cpu.P | CpuStatusRegisterFlags.BreakFlag;
                cpuInternal.DataBus = (byte) statusFlags;
                cpu.SP--;
                return true;

            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PLP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                //cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 0:
                cpu.SP++;
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
                break;
            case 1:
                cpuInternal.AddressBus = cpu.PC;
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                return true;
            case afterNextOpcodeFetched:
                cpu.P = (CpuStatusRegisterFlags) cpuInternal.Temporary.Low &
                    ~CpuStatusRegisterFlags.BreakFlag |
                    CpuStatusRegisterFlags.Unused;

                break;
        }

        return false;
    }

    #endregion

    #region Decrements & Increments

    [MethodImpl(MethodOptions.Options)]
    public static bool INC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.Temporary.Low = (byte) (cpuInternal.DataBus + 1);
                break;
            case 1:
                cpuInternal.DataBus = cpuInternal.Temporary.Low;
                return true;
            case afterNextOpcodeFetched:
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpuInternal.Temporary.Low);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool DEC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.Temporary.Low = (byte) (cpuInternal.DataBus - 1);
                break;
            case 1:
                cpuInternal.DataBus = (byte) cpuInternal.Temporary;
                return true;
            case afterNextOpcodeFetched:
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpuInternal.Temporary.Low);
                break;
        }

        return false;
    }

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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);
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

                SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);
                break;
        }

        return false;
    }

    #endregion

    #region Arithmetic Operations

    [MethodImpl(MethodOptions.Options)]
    public static bool ADC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return ADC_SBC_Internal(operationCycle, ref cpu, ref cpuInternal, cpuInternal.DataBus);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SBC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return ADC_SBC_Internal(operationCycle, ref cpu, ref cpuInternal, (byte) ~cpuInternal.DataBus);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ADC_SBC_Internal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, byte data) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                ushort result16 = (ushort) (cpu.A + data + (cpu.P.HasFlag(CpuStatusRegisterFlags.Carry) ? 1 : 0));
                byte result = (byte) result16;

                cpu.P.SetFlag(CpuStatusRegisterFlags.Carry, result16 > byte.MaxValue);
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, result);

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
                cpu.A &= cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
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
                cpu.A ^= cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
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
                cpu.A |= cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }

    #endregion

    #region Shift & Rotate Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool LSR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        static void Handler(ref byte value, ref CpuStatusRegisterFlags status) {
            byte input = value;
            value = (byte) (value >> 1);

            status.RemoveFlag(CpuStatusRegisterFlags.NegativeOrSign);
            status.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);
            status.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 0) != 0);
        }

        return RmwWithImpliedInternal(
            operationCycle,
            ref cpu,
            ref cpuInternal,
            ref cpu.A,
            Handler
        );
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ASL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        static void Handler(ref byte value, ref CpuStatusRegisterFlags status) {
            byte input = value;
            value = (byte) (value << 1);

            SetGenericZeroAndNegativeSignFlags(ref status, value);
            status.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 7) != 0);
        }

        return RmwWithImpliedInternal(
            operationCycle,
            ref cpu,
            ref cpuInternal,
            ref cpu.A,
            Handler
        );
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ROL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        static void Handler(ref byte value, ref CpuStatusRegisterFlags status) {
            byte input = value;

            value = (byte) ((value << 1) | (status.HasFlag(CpuStatusRegisterFlags.Carry) ? 1 : 0));

            status.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 7) != 0);
            status.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (input & 1 << 6) != 0);
            status.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);
        }

        return RmwWithImpliedInternal(
            operationCycle,
            ref cpu,
            ref cpuInternal,
            ref cpu.A,
            Handler
        );
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ROR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        static void Handler(ref byte value, ref CpuStatusRegisterFlags status) {
            byte input = value;

            status.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, status.HasFlag(CpuStatusRegisterFlags.Carry));
            value = (byte) (value >> 1);
            value |= (byte) (status.HasFlag(CpuStatusRegisterFlags.Carry) ? 1 << 7 : 0);
            status.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 0) != 0);

            status.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);
        }

        return RmwWithImpliedInternal(
            operationCycle,
            ref cpu,
            ref cpuInternal,
            ref cpu.A,
            Handler
        );
    }

    private delegate void RmwOperationHandler(ref byte value, ref CpuStatusRegisterFlags status);

    [MethodImpl(MethodOptions.Options)]
    private static bool RmwWithImpliedInternal(
        int operationCycle,
        ref CpuState cpu,
        ref CpuInternalState cpuInternal,
        ref byte impliedValue,
        RmwOperationHandler handler
    ) {
        if (cpuInternal.Instruction.AddressingMode == AddressingMode.Implied) {
            switch (operationCycle) {
                case afterAddressingFinished:
                    return true;
                case afterNextOpcodeFetched:
                    handler(ref impliedValue, ref cpu.P);
                    break;
            }
        } else {
            switch (operationCycle) {
                case afterAddressingFinished:
                    break;
                case 0:
                    cpuInternal.Read = false;
                    cpuInternal.Temporary.Low = cpuInternal.DataBus;
                    handler(ref cpuInternal.Temporary.Low, ref cpu.P);
                    break;
                case 1:
                    cpuInternal.DataBus = cpuInternal.Temporary.Low;
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
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, result);
                break;
        }

        return false;
    }

    #endregion

    #region Conditional Branch Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool BCS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) => 
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlag(CpuStatusRegisterFlags.Carry));

    [MethodImpl(MethodOptions.Options)]
    public static bool BCC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) => 
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.HasFlag(CpuStatusRegisterFlags.Carry));

    [MethodImpl(MethodOptions.Options)]
    public static bool BEQ(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) => 
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlag(CpuStatusRegisterFlags.Zero));

    [MethodImpl(MethodOptions.Options)]
    public static bool BNE(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) => 
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.HasFlag(CpuStatusRegisterFlags.Zero));

    [MethodImpl(MethodOptions.Options)]
    public static bool BMI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) => 
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlag(CpuStatusRegisterFlags.NegativeOrSign));

    [MethodImpl(MethodOptions.Options)]
    public static bool BPL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) => 
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.HasFlag(CpuStatusRegisterFlags.NegativeOrSign));

    [MethodImpl(MethodOptions.Options)]
    public static bool BVS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) => 
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlag(CpuStatusRegisterFlags.Overflow));

    [MethodImpl(MethodOptions.Options)]
    public static bool BVC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) => 
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.HasFlag(CpuStatusRegisterFlags.Overflow));

    [MethodImpl(MethodOptions.Options)]
    private static bool BranchInternal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, bool condition) {
        switch (operationCycle) {
            case afterAddressingFinished:
                break;
            case 0:
                cpu.PC.Value++;
                if (condition) {
                    cpu.PC = (ushort) (cpu.PC + (sbyte) cpuInternal.DataBus);
                }

                cpuInternal.AddressBus = cpu.PC;
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
                cpu.PC = cpuInternal.AddressBus;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool JSR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        ushort stackAddress = CalculateStackAddress(cpu.SP);
        switch (operationCycle) {
            case afterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
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
                cpuInternal.Read = true;
                
                // subroutine address
                cpuInternal.AddressBus.High = cpuInternal.DataBus;
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpu.PC = cpuInternal.AddressBus;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool RTS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
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
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    #endregion

    #region Interrupts

    [MethodImpl(MethodOptions.Options)]
    public static bool BRK(int operationCycle, ref MachineState machineState, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.Read = false;

                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.PC.High;
                cpu.P.SetFlag(CpuStatusRegisterFlags.BreakFlag);

                cpu.SP--;
                break;
            case 1:
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.PC.Low;
                cpu.SP--;
                break;
            case 2:
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = (byte) cpu.P;
                cpu.SP--;
                break;
            case 3:
                cpuInternal.Read = true;
                cpuInternal.AddressBus = 0xFFFE;
                break;
            case 4:
                cpu.PC.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus = 0xFFFF;
                break;
            case 5:
                cpu.PC.High = cpuInternal.DataBus;
                cpuInternal.AddressBus = cpu.PC;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool RTI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpu.SP++;
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
                break;
            case 1:
                cpu.P = (CpuStatusRegisterFlags) cpuInternal.DataBus &
                    ~CpuStatusRegisterFlags.BreakFlag |
                    CpuStatusRegisterFlags.Unused;

                cpu.SP++;
                cpuInternal.AddressBus = CalculateStackAddress(cpu.SP);
                break;
            case 2:
                cpu.PC.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus = cpu.PC;
                cpu.SP++;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
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
        throw new NotImplementedException($"!!! Unhandled opcode ({cpuInternal.Instruction})");
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool LAX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.A = cpuInternal.DataBus;
                cpu.X = cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool SAX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = (byte) (cpu.A & cpu.X);
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    #endregion

    [MethodImpl(MethodOptions.Options)]
    private static void SetGenericZeroAndNegativeSignFlags(ref CpuStatusRegisterFlags status, byte value) {
        status.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);
        status.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (value & 1 << 7) != 0);
    }

    [MethodImpl(MethodOptions.Options)]
    private static ushort CalculateStackAddress(byte stackPointer)
        => (ushort) (0x100 + stackPointer);
}
