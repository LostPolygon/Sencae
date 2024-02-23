using System;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Local

namespace Sencae;

using I = InstructionType;

public static class InstructionLogic {
    public const int AfterAddressingFinished = -1;
    public const int AfterNextOpcodeFetched = -2;

    #region Dispatcher

    // Enabling inlining here actually reduces perf on x64 by about 25%!
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool Dispatch(CpuState cpu, ref CpuInternalState cpuInternal, InstructionType instructionType) {
        int operationCycle = cpuInternal.OperationCycle;

        return instructionType switch {
            // Transfer Instructions

            I.LDA => LDA(operationCycle, cpu, ref cpuInternal), // load accumulator
            I.LDX => LDX(operationCycle, cpu, ref cpuInternal), // load X
            I.LDY => LDY(operationCycle, cpu, ref cpuInternal), // load Y
            I.STA => STA(operationCycle, cpu, ref cpuInternal), // store accumulator
            I.STX => STX(operationCycle, cpu, ref cpuInternal), // store X
            I.STY => STY(operationCycle, cpu, ref cpuInternal), // store Y
            I.TAX => TAX(operationCycle, cpu, ref cpuInternal), // transfer accumulator to X
            I.TAY => TAY(operationCycle, cpu, ref cpuInternal), // transfer accumulator to Y
            I.TSX => TSX(operationCycle, cpu, ref cpuInternal), // transfer stack pointer to X
            I.TXA => TXA(operationCycle, cpu, ref cpuInternal), // transfer X to accumulator
            I.TXS => TXS(operationCycle, cpu, ref cpuInternal), // transfer X to stack pointer
            I.TYA => TYA(operationCycle, cpu, ref cpuInternal), // transfer Y to accumulator

            // Stack Instructions

            I.PHA => PHA(operationCycle, cpu, ref cpuInternal), // push accumulator
            I.PHP => PHP(operationCycle, cpu, ref cpuInternal), // push processor status register (with break flag set)
            I.PLA => PLA(operationCycle, cpu, ref cpuInternal), // pull accumulator
            I.PLP => PLP(operationCycle, cpu, ref cpuInternal), // pull processor status register 

            // Decrements & Increments

            I.DEC => DEC(operationCycle, cpu, ref cpuInternal), // decrement (memory)
            I.DEX => DEX(operationCycle, cpu, ref cpuInternal), // decrement X
            I.DEY => DEY(operationCycle, cpu, ref cpuInternal), // decrement Y
            I.INC => INC(operationCycle, cpu, ref cpuInternal), // increment (memory)
            I.INX => INX(operationCycle, cpu, ref cpuInternal), // increment X
            I.INY => INY(operationCycle, cpu, ref cpuInternal), // increment Y

            // Arithmetic Operations

            I.ADC => ADC(operationCycle, cpu, ref cpuInternal), // add with carry (prepare by CLC)
            I.SBC => SBC(operationCycle, cpu, ref cpuInternal), // subtract with carry (prepare by SEC)

            // Logical Operations

            I.AND => AND(operationCycle, cpu, ref cpuInternal), // and (with accumulator)
            I.EOR => EOR(operationCycle, cpu, ref cpuInternal), // exclusive or (with accumulator)
            I.ORA => ORA(operationCycle, cpu, ref cpuInternal), // (inclusive) or with accumulator

            // Shift & Rotate Instructions

            I.ASL => ASL(operationCycle, cpu, ref cpuInternal), // arithmetic shift left (shifts in a zero bit on the right)
            I.LSR => LSR(operationCycle, cpu, ref cpuInternal), // logical shift right (shifts in a zero bit on the left)
            I.ROL => ROL(operationCycle, cpu, ref cpuInternal), // rotate left (shifts in carry bit on the right)
            I.ROR => ROR(operationCycle, cpu, ref cpuInternal), // rotate right (shifts in zero bit on the left)

            // Flag Instructions

            I.CLC => CLC(operationCycle, cpu, ref cpuInternal), // clear carry
            I.CLD => CLD(operationCycle, cpu, ref cpuInternal), // clear decimal (BCD arithmetics disabled)
            I.CLI => CLI(operationCycle, cpu, ref cpuInternal), // clear interrupt disable
            I.CLV => CLV(operationCycle, cpu, ref cpuInternal), // clear overflow
            I.SEC => SEC(operationCycle, cpu, ref cpuInternal), // set carry
            I.SED => SED(operationCycle, cpu, ref cpuInternal), // set decimal (BCD arithmetics enabled)
            I.SEI => SEI(operationCycle, cpu, ref cpuInternal), // set interrupt disable

            // Comparisons
            I.CMP => CMP(operationCycle, cpu, ref cpuInternal), // compare (with accumulator)
            I.CPX => CPX(operationCycle, cpu, ref cpuInternal), // compare with X
            I.CPY => CPY(operationCycle, cpu, ref cpuInternal), // compare with Y 

            // Jumps & Subroutines

            I.JMP => JMP(operationCycle, cpu, ref cpuInternal), // jump
            I.JSR => JSR(operationCycle, cpu, ref cpuInternal), // jump subroutine
            I.RTS => RTS(operationCycle, cpu, ref cpuInternal), // return from subroutine 

            // Conditional Branch Instructions

            I.BCS => BCS(operationCycle, cpu, ref cpuInternal), // branch on carry set
            I.BCC => BCC(operationCycle, cpu, ref cpuInternal), // branch on carry clear
            I.BEQ => BEQ(operationCycle, cpu, ref cpuInternal), // branch on equal (zero set)
            I.BNE => BNE(operationCycle, cpu, ref cpuInternal), // branch on not equal (zero clear)
            I.BMI => BMI(operationCycle, cpu, ref cpuInternal), // branch on minus (negative set)
            I.BPL => BPL(operationCycle, cpu, ref cpuInternal), // branch on plus (negative clear)
            I.BVS => BVS(operationCycle, cpu, ref cpuInternal), // branch on overflow set 
            I.BVC => BVC(operationCycle, cpu, ref cpuInternal), // branch on overflow clear

            // Interrupts

            I.BRK => BRK(operationCycle, cpu, ref cpuInternal), // break / software interrupt
            I.SpecialNmiHandler => NmiHandler(operationCycle, cpu, ref cpuInternal),
            I.RTI => RTI(operationCycle, cpu, ref cpuInternal), // return from interrupt 

            // Other

            I.BIT => BIT(operationCycle, cpu, ref cpuInternal), // bit test (accumulator & memory)
            I.NOP => NOP(operationCycle, cpu, ref cpuInternal), // no operation

            // Unofficial

            I.STP => STP(operationCycle, cpu, ref cpuInternal),
            I.SLO => SLO(operationCycle, cpu, ref cpuInternal),
            I.ANC => ANC(operationCycle, cpu, ref cpuInternal),
            I.RLA => RLA(operationCycle, cpu, ref cpuInternal),
            I.SRE => SRE(operationCycle, cpu, ref cpuInternal),
            I.ALR => ALR(operationCycle, cpu, ref cpuInternal),
            I.RRA => RRA(operationCycle, cpu, ref cpuInternal),
            I.SHY => SHY(operationCycle, cpu, ref cpuInternal),
            I.ARR => ARR(operationCycle, cpu, ref cpuInternal),
            I.SHX => SHX(operationCycle, cpu, ref cpuInternal),
            I.SAX => SAX(operationCycle, cpu, ref cpuInternal),
            I.XAA => XXX(operationCycle, cpu, ref cpuInternal),
            I.AHX => AHX(operationCycle, cpu, ref cpuInternal),
            I.TAS => XXX(operationCycle, cpu, ref cpuInternal),
            I.LAX => LAX(operationCycle, cpu, ref cpuInternal),
            I.LAS => XXX(operationCycle, cpu, ref cpuInternal),
            I.DCP => DCP(operationCycle, cpu, ref cpuInternal),
            I.AXS => AXS(operationCycle, cpu, ref cpuInternal),
            I.ISC => ISC(operationCycle, cpu, ref cpuInternal),

            _ => XXX(operationCycle, cpu, ref cpuInternal)
        };
    }

    [MethodImpl(HotPath)]
    private static bool NmiHandler(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return InterruptSequence(operationCycle, cpu, ref cpuInternal, CpuConstants.NmiVector, false);
    }

    #endregion

    #region Transfer Instructions

    [MethodImpl(HotPath)]
    public static bool LDA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool LDX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool LDY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.Y = cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.Y);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool STA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        if (operationCycle == AfterAddressingFinished) {
            cpuInternal.Read = false;
            cpuInternal.DataBus = cpu.A;
            return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool STX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        if (operationCycle == AfterAddressingFinished) {
            cpuInternal.Read = false;
            cpuInternal.DataBus = cpu.X;
            return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool STY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        if (operationCycle == AfterAddressingFinished) {
            cpuInternal.Read = false;
            cpuInternal.DataBus = cpu.Y;
            return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool TAX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpu.A;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool TAY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.Y = cpu.A;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.Y);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool TSX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpu.SP;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool TXA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpu.X;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool TXS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.SP = cpu.X;

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool TYA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpu.Y;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    #endregion

    #region Stack Instructions

    [MethodImpl(HotPath)]
    public static bool PHA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);

                cpuInternal.DataBus = cpu.A;
                cpu.SP--;
                break;
            case 0:
                cpuInternal.Read = true;
                cpuInternal.AddressBus = cpu.PC;
                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool PHP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);

                cpuInternal.DataBus = (byte) (cpu.P.AsFlags | CpuStatusRegisterPackedFlags.BreakFlag);
                cpu.SP--;
                break;
            case 0:
                cpuInternal.Read = true;
                cpuInternal.AddressBus = cpu.PC;
                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool PLP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);

                break;
            case 0:
                cpu.SP++;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                return true;
            case AfterNextOpcodeFetched:
                cpu.P = CpuStatusRegisterFlags.FromFlags((CpuStatusRegisterPackedFlags) cpuInternal.DataBus);
                cpu.P.BreakFlag = false;

                cpuInternal.AddressBus.Value = cpu.PC.Value;
                break;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool PLA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 0:
                cpu.SP++;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                return true;
            case AfterNextOpcodeFetched:
                cpu.A = cpuInternal.DataBus;

                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

                cpuInternal.AddressBus.Value = cpu.PC.Value;
                break;
        }

        return false;
    }

    #endregion

    #region Decrements & Increments

    [MethodImpl(HotPath)]
    public static bool INC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
                cpuInternal.Read = false;
                cpuInternal.Temporary.Low = (byte) (cpuInternal.DataBus + 1);
                break;
            case 1:
                cpuInternal.DataBus = cpuInternal.Temporary.Low;
                return true;
            case AfterNextOpcodeFetched:
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpuInternal.Temporary.Low);
                break;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool DEC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
                cpuInternal.Read = false;
                cpuInternal.Temporary.Low = (byte) (cpuInternal.DataBus - 1);
                break;
            case 1:
                cpuInternal.DataBus = cpuInternal.Temporary.Low;
                return true;
            case AfterNextOpcodeFetched:
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpuInternal.Temporary.Low);
                break;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool DEX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return DEX_DEY_Internal(operationCycle, cpu, ref cpuInternal, ref cpu.X);
    }

    [MethodImpl(HotPath)]
    public static bool DEY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return DEX_DEY_Internal(operationCycle, cpu, ref cpuInternal, ref cpu.Y);
    }

    [MethodImpl(HotPath)]
    public static bool INX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return INX_INY_Internal(operationCycle, cpu, ref cpuInternal, ref cpu.X);
    }

    [MethodImpl(HotPath)]
    public static bool INY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return INX_INY_Internal(operationCycle, cpu, ref cpuInternal, ref cpu.Y);
    }

    [MethodImpl(HotPath)]
    private static bool INX_INY_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, ref byte value) {
        value++;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);

        return false;
    }

    [MethodImpl(HotPath)]
    private static bool DEX_DEY_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, ref byte value) {
        value--;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);

        return false;
    }

    #endregion

    #region Arithmetic Operations

    [MethodImpl(HotPath)]
    public static bool ADC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return ADC_SBC_Internal(operationCycle, cpu, ref cpuInternal, cpuInternal.DataBus);
    }

    [MethodImpl(HotPath)]
    public static bool SBC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return ADC_SBC_Internal(operationCycle, cpu, ref cpuInternal, (byte) ~cpuInternal.DataBus);
    }

    [MethodImpl(HotPath)]
    private static bool ADC_SBC_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, byte data) {
        ushort result16 = (ushort) (cpu.A + data + (cpu.P.Carry ? 1 : 0));
        byte result = (byte) result16;

        cpu.P.Carry = result16 > Byte.MaxValue;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, result);

        cpu.P.Overflow = ((cpu.A ^ result) & (data ^ result) & 1 << 7) != 0;
        cpu.A = result;

        return false;
    }

    #endregion

    #region Logical Operations

    [MethodImpl(HotPath)]
    public static bool AND(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A &= cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool EOR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A ^= cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool ORA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A |= cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return true;
    }

    #endregion

    #region Shift & Rotate Instructions

    [MethodImpl(HotPath)]
    public static bool LSR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        return LSR_Internal(operationCycle, cpu, ref cpuInternal, impliedAddressing);
    }

    [MethodImpl(HotPath)]
    public static bool ASL(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        ref byte value = ref GetReadWriteModifyInstructionTargetReference(
            operationCycle,
            cpu,
            ref cpuInternal,
            impliedAddressing,
            out bool completed,
            out bool mustHandle
        );

        if (mustHandle) {
            byte input = value;
            value = (byte) (value << 1);

            SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);
            cpu.P.Carry = (input & 1 << 7) != 0;
        }

        return completed;
    }

    [MethodImpl(HotPath)]
    public static bool ROL(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        ref byte value = ref GetReadWriteModifyInstructionTargetReference(
            operationCycle,
            cpu,
            ref cpuInternal,
            impliedAddressing,
            out bool completed,
            out bool mustHandle
        );

        if (mustHandle) {
            byte input = value;
            value = (byte) ((value << 1) | (cpu.P.Carry ? 1 : 0));

            cpu.P.Carry = (input & 1 << 7) != 0;
            cpu.P.NegativeOrSign = (input & 1 << 6) != 0;
            cpu.P.Zero = value == 0;
        }

        return completed;
    }

    [MethodImpl(HotPath)]
    public static bool ROR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        return ROR_Internal(operationCycle, cpu, ref cpuInternal, impliedAddressing);
    }

    [MethodImpl(HotPath)]
    private static bool ROR_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, bool impliedAddressing) {
        ref byte value = ref GetReadWriteModifyInstructionTargetReference(
            operationCycle,
            cpu,
            ref cpuInternal,
            impliedAddressing,
            out bool completed,
            out bool mustHandle
        );

        if (mustHandle) {
            byte input = value;
            value = (byte) (value >> 1);
            value |= (byte) (cpu.P.Carry ? 1 << 7 : 0);

            cpu.P.NegativeOrSign = cpu.P.Carry;
            cpu.P.Carry = (input & 1 << 0) != 0;
            cpu.P.Zero = value == 0;
        }

        return completed;
    }

    [MethodImpl(HotPath)]
    private static bool LSR_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, bool impliedAddressing) {
        ref byte value = ref GetReadWriteModifyInstructionTargetReference(
            operationCycle,
            cpu,
            ref cpuInternal,
            impliedAddressing,
            out bool completed,
            out bool mustHandle
        );

        if (mustHandle) {
            byte input = value;
            value = (byte) (value >> 1);

            cpu.P.NegativeOrSign = false;
            cpu.P.Zero = value == 0;
            cpu.P.Carry = (input & 1 << 0) != 0;
        }

        return completed;
    }

    #endregion

    #region Flag Instructions

    [MethodImpl(HotPath)]
    public static bool CLC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Carry = false;
        return true;
    }

    [MethodImpl(HotPath)]
    public static bool CLD(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.DecimalMode = false;
        return true;
    }

    [MethodImpl(HotPath)]
    public static bool CLI(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.InterruptDisable = false;
        return true;
    }

    [MethodImpl(HotPath)]
    public static bool CLV(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Overflow = false;
        return true;
    }

    [MethodImpl(HotPath)]
    public static bool SEC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Carry = true;
        return true;
    }

    [MethodImpl(HotPath)]
    public static bool SED(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.DecimalMode = true;
        return true;
    }

    [MethodImpl(HotPath)]
    public static bool SEI(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.InterruptDisable = true;
        return true;
    }

    #endregion

    #region Comparisons

    [MethodImpl(HotPath)]
    public static bool CMP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, cpu, ref cpuInternal, cpu.A);
    }

    [MethodImpl(HotPath)]
    public static bool CPX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, cpu, ref cpuInternal, cpu.X);
    }

    [MethodImpl(HotPath)]
    public static bool CPY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, cpu, ref cpuInternal, cpu.Y);
    }

    [MethodImpl(HotPath)]
    private static bool CMP_Internal(
        int operationCycle,
        CpuState cpu,
        ref CpuInternalState cpuInternal,
        byte comparedValue
    ) {
        CMP_Internal_SetCompareFlags(operationCycle, cpu, ref cpuInternal, comparedValue);
        return false;
    }

    [MethodImpl(HotPath)]
    private static byte CMP_Internal_SetCompareFlags(
        int operationCycle,
        CpuState cpu,
        ref CpuInternalState cpuInternal,
        byte comparedValue
    ) {
        byte result = (byte) (comparedValue - cpuInternal.DataBus);

        cpu.P.Carry = cpuInternal.DataBus <= comparedValue;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, result);

        return result;
    }

    #endregion

    #region Conditional Branch Instructions

    [MethodImpl(HotPath)]
    public static bool BCS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, cpu.P.Carry);

    [MethodImpl(HotPath)]
    public static bool BCC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, !cpu.P.Carry);

    [MethodImpl(HotPath)]
    public static bool BEQ(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, cpu.P.Zero);

    [MethodImpl(HotPath)]
    public static bool BNE(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, !cpu.P.Zero);

    [MethodImpl(HotPath)]
    public static bool BMI(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, cpu.P.NegativeOrSign);

    [MethodImpl(HotPath)]
    public static bool BPL(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, !cpu.P.NegativeOrSign);

    [MethodImpl(HotPath)]
    public static bool BVS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, cpu.P.Overflow);

    [MethodImpl(HotPath)]
    public static bool BVC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, !cpu.P.Overflow);

    [MethodImpl(HotPath)]
    private static bool BranchInternal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, bool condition) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                cpuInternal.Temporary.Low = cpuInternal.DataBus; // offset
                return !condition;
            case 0: // branch taken
                cpuInternal.Temporary.Value = (ushort) (cpu.PC.Value + (sbyte) cpuInternal.Temporary.Low);
                cpuInternal.PageBoundaryCrossedCarry = (sbyte) (cpuInternal.Temporary.High - cpu.PC.High);
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                if (cpuInternal.PageBoundaryCrossedCarry == 0) {
                    cpu.PC = cpuInternal.AddressBus;
                }

                return cpuInternal.PageBoundaryCrossedCarry == 0;
            case 1: // page boundary crossed
                cpuInternal.Temporary.High = (byte) (cpu.PC.High + cpuInternal.PageBoundaryCrossedCarry);
                cpuInternal.AddressBus = cpuInternal.Temporary;
                cpu.PC = cpuInternal.AddressBus;
                cpuInternal.State = CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode;
                return true;
            case AfterNextOpcodeFetched: // fixup PC after page boundary cross
                cpu.PC.High = cpuInternal.Temporary.High;
                cpuInternal.PageBoundaryCrossedCarry = 0;
                break;
        }

        return false;
    }

    #endregion

    #region Jumps & Subroutines

    [MethodImpl(HotPath)]
    public static bool JMP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.PC = cpuInternal.AddressBus;
        return true;
    }

    [MethodImpl(HotPath)]
    public static bool JSR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 0:
                cpuInternal.Read = false;

                cpuInternal.DataBus = cpu.PC.High;
                cpu.SP--;
                break;
            case 1:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.PC.Low;
                cpu.SP--;
                break;
            case 2:
                cpuInternal.Read = true;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 3:
                // subroutine address
                cpuInternal.AddressBus.High = cpuInternal.DataBus;
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpu.PC = cpuInternal.AddressBus;
                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool RTS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpu.SP++;

                break;
            case 0:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpu.SP++;
                break;
            case 1:
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 2:
                cpuInternal.Temporary.High = cpuInternal.DataBus;
                cpuInternal.AddressBus = cpuInternal.Temporary;
                cpu.PC = cpuInternal.AddressBus;
                break;
            case 3:
                cpu.PC.Value++;
                cpuInternal.AddressBus.Value = cpu.PC.Value;
                return true;
        }

        return false;
    }

    #endregion

    #region Interrupts

    [MethodImpl(NoInlining)]
    public static bool BRK(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return InterruptSequence(
            operationCycle,
            cpu,
            ref cpuInternal,
            CpuConstants.IrqVector,
            true
        );
    }

    [MethodImpl(HotPath)]
    public static bool RTI(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpu.SP++;
                break;
            case 0:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpu.SP++;
                break;
            case 1:
                cpu.P = CpuStatusRegisterFlags.FromFlags((CpuStatusRegisterPackedFlags) cpuInternal.DataBus);
                cpu.P.BreakFlag = false;

                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpu.SP++;
                break;
            case 2:
                cpu.PC.Low = cpuInternal.DataBus;

                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 3:
                cpu.PC.High = cpuInternal.DataBus;
                cpuInternal.AddressBus = cpu.PC;
                return true;
        }

        return false;
    }

    #endregion

    #region Other

    [MethodImpl(HotPath)]
    public static bool BIT(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        byte result = (byte) (cpu.A & cpuInternal.DataBus);
        cpu.P.NegativeOrSign = (cpuInternal.DataBus & 1 << 7) != 0;
        cpu.P.Overflow = (cpuInternal.DataBus & 1 << 6) != 0;
        cpu.P.Zero = result == 0;

        return false;
    }

    [MethodImpl(HotPath)]
    public static bool NOP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    #endregion

    #region Unofficial

    [MethodImpl(NoInlining)]
    public static bool LAX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpuInternal.DataBus;
        cpu.X = cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return true;
    }

    [MethodImpl(NoInlining)]
    public static bool SAX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        if (operationCycle == AfterAddressingFinished) {
            cpuInternal.Read = false;
            cpuInternal.DataBus = (byte) (cpu.A & cpu.X);
            return true;
        }

        return false;
    }

    [MethodImpl(NoInlining)]
    public static bool DCP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return DEC(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                DEC(operationCycle, cpu, ref cpuInternal);
                CMP_Internal_SetCompareFlags(operationCycle, cpu, ref cpuInternal, cpu.A);
                return false;
        }
    }

    [MethodImpl(NoInlining)]
    public static bool ISC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return INC(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                INC(operationCycle, cpu, ref cpuInternal);
                SBC(operationCycle, cpu, ref cpuInternal);
                return false;
        }
    }

    [MethodImpl(NoInlining)]
    public static bool RLA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ROL(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ROL(operationCycle, cpu, ref cpuInternal);
                AND(operationCycle, cpu, ref cpuInternal);
                return false;
        }
    }

    [MethodImpl(NoInlining)]
    public static bool RRA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ROR(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ROR(operationCycle, cpu, ref cpuInternal);
                ADC(operationCycle, cpu, ref cpuInternal);
                return false;
        }
    }

    [MethodImpl(NoInlining)]
    public static bool SLO(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ASL(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ASL(operationCycle, cpu, ref cpuInternal);
                ORA(operationCycle, cpu, ref cpuInternal);
                return false;
        }
    }

    [MethodImpl(NoInlining)]
    public static bool SRE(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return LSR(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                LSR(operationCycle, cpu, ref cpuInternal);
                EOR(operationCycle, cpu, ref cpuInternal);
                return false;
        }
    }

    [MethodImpl(NoInlining)]
    public static bool ANC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return AND(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                AND(operationCycle, cpu, ref cpuInternal);
                cpu.P.Carry = (cpu.A & 1 << 7) != 0;
                return false;
        }
    }

    [MethodImpl(NoInlining)]
    public static bool ARR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        // https://forums.nesdev.org/viewtopic.php?t=3831&start=15
        AND(operationCycle, cpu, ref cpuInternal);
        ROR_Internal(operationCycle, cpu, ref cpuInternal, true);

        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
        cpu.P.Carry = (cpu.A & 1 << 6) != 0;
        cpu.P.Overflow = (((cpu.A & 1 << 5) >> 5) ^ ((cpu.A & 1 << 6) >> 6)) != 0;

        return true;
    }

    [MethodImpl(NoInlining)]
    public static bool SHX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return SHX_SHY_Internal(operationCycle, cpu, ref cpuInternal, cpu.X);
    }

    [MethodImpl(NoInlining)]
    public static bool SHY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return SHX_SHY_Internal(operationCycle, cpu, ref cpuInternal, cpu.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SHX_SHY_Internal(
        int operationCycle,
        CpuState cpu,
        ref CpuInternalState cpuInternal,
        byte indexRegister
    ) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                if (cpuInternal.PageBoundaryCrossedCarry != 0) {
                    cpuInternal.AddressBus.High &= indexRegister;
                }

                return false;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.DataBus = (byte) (indexRegister & (cpuInternal.AddressBus.High + 1));
                return true;
        }

        return false;
    }

    [MethodImpl(NoInlining)]
    public static bool AHX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        // A AND X AND (H+1) -> M
        switch (operationCycle) {
            case 0:
                cpuInternal.Read = false;
                cpuInternal.DataBus = (byte) (cpu.A & cpu.X & (cpu.PC.High + 1));
                return true;
        }

        return false;
    }

    [MethodImpl(NoInlining)]
    public static bool ALR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                AND(operationCycle, cpu, ref cpuInternal);
                LSR_Internal(operationCycle, cpu, ref cpuInternal, true);
                return true;
        }

        return false;
    }

    /////////////

    [MethodImpl(NoInlining)]
    public static bool STP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpuInternal.Halted = true;

        return true;
    }

    [MethodImpl(NoInlining)]
    public static bool AXS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        byte andResult = (byte) (cpu.A & cpu.X);
        cpu.X = CMP_Internal_SetCompareFlags(operationCycle, cpu, ref cpuInternal, andResult);

        return true;
    }

    [MethodImpl(NoInlining)]
    public static bool XXX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        //Debug.WriteLine($"!!! Unhandled opcode ({cpuInternal.Instruction})");
        throw new NotImplementedException($"!!! Unhandled opcode ({cpuInternal.Instruction})");
        //return true;
    }

    #endregion

    [MethodImpl(HotPath)]
    private static void SetGenericZeroAndNegativeSignFlags(ref CpuStatusRegisterFlags status, byte value) {
        status.Zero = value == 0;
        status.NegativeOrSign = (value & 1 << 7) != 0;
    }

    [MethodImpl(HotPath)]
    private static ushort CalculateStackAddress(byte stackPointer)
        => (ushort) (0x100 + stackPointer);

    [MethodImpl(HotPath)]
    private static bool InterruptSequence(
        int operationCycle,
        CpuState cpu,
        ref CpuInternalState cpuInternal,
        ushort vector,
        bool isBrk
    ) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;

                cpuInternal.Temporary.Value = isBrk ? (ushort) (cpu.PC.Value + 1) : (ushort) (cpu.PC.Value + 0);
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpuInternal.Temporary.High;

                cpu.SP--;
                break;
            case 0:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpuInternal.Temporary.Low;
                cpu.SP--;
                break;
            case 1:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);

                CpuStatusRegisterPackedFlags flags = cpu.P.AsFlags;
                if (isBrk) {
                    flags |= CpuStatusRegisterPackedFlags.BreakFlag;
                } else {
                    flags &= ~CpuStatusRegisterPackedFlags.BreakFlag;
                }

                cpuInternal.DataBus = (byte) flags;
                cpu.SP--;
                break;
            case 2:
                cpu.P.InterruptDisable = true;

                cpuInternal.Read = true;
                cpuInternal.AddressBus.Value = vector;
                break;
            case 3:
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = (ushort) (vector + 1);
                break;
            case 4:
                cpu.PC.Low = cpuInternal.Temporary.Low;
                cpu.PC.High = cpuInternal.DataBus;
                cpuInternal.AddressBus = cpu.PC;

                cpuInternal.NmiFlipFlop = false;
                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    private static ref byte GetReadWriteModifyInstructionTargetReference(
        int operationCycle,
        CpuState cpu,
        ref CpuInternalState cpuInternal,
        bool impliedAddressing,
        out bool completed,
        out bool mustHandle
    ) {
        completed = false;
        mustHandle = false;

        if (impliedAddressing) {
            if (operationCycle == AfterNextOpcodeFetched) {
                mustHandle = true;
                return ref cpu.A;
            }
        } else {
            switch (operationCycle) {
                case 0:
                    cpuInternal.Read = false;
                    cpuInternal.Temporary.Low = cpuInternal.DataBus;

                    mustHandle = true;
                    return ref cpuInternal.Temporary.Low;
                case 1:
                    cpuInternal.DataBus = cpuInternal.Temporary.Low;
                    completed = true;
                    break;
            }
        }

        // Just return something, it doesn't matter since we only
        // use the return value if mustHandle == true
        return ref cpu.A;
    }
}
