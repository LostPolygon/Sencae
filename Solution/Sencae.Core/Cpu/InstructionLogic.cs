using System;
using System.Diagnostics;
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
            I.Internal_BRK_NMI => BRK_WithAddress(operationCycle, cpu, ref cpuInternal, CpuConstants.NmiVector),
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

    #endregion

    #region Transfer Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool LDA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.Y = cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.Y);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.A;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.X;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.Y;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TAX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpu.A;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TAY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.Y = cpu.A;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.Y);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TSX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpu.SP;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TXA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpu.X;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TXS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.SP = cpu.X;

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TYA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpu.Y;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    #endregion

    #region Stack Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool PHA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.A;
                cpu.SP--;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PLA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 0:
                cpu.SP++;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 1:
                cpuInternal.AddressBus = cpu.PC;
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                return true;
            case AfterNextOpcodeFetched:
                cpu.A = cpuInternal.Temporary.Low;

                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PHP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);

                cpuInternal.DataBus = (byte) (cpu.P.ToFlags() | CpuStatusRegisterPackedFlags.BreakFlag);
                cpu.SP--;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PLP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
                cpu.SP++;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 1:
                cpuInternal.AddressBus = cpu.PC;
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                return true;
            case AfterNextOpcodeFetched:
                cpu.P = CpuStatusRegisterFlags.FromFlags((CpuStatusRegisterPackedFlags) cpuInternal.Temporary.Low);
                cpu.P.BreakFlag = false;

                break;
        }

        return false;
    }

    #endregion

    #region Decrements & Increments

    [MethodImpl(MethodOptions.Options)]
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

    [MethodImpl(MethodOptions.Options)]
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

    [MethodImpl(MethodOptions.Options)]
    public static bool DEX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return DEX_DEY_Internal(operationCycle, cpu, ref cpuInternal, ref cpu.X);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool DEY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return DEX_DEY_Internal(operationCycle, cpu, ref cpuInternal, ref cpu.Y);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool INX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return INX_INY_Internal(operationCycle, cpu, ref cpuInternal, ref cpu.X);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool INY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return INX_INY_Internal(operationCycle, cpu, ref cpuInternal, ref cpu.Y);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool INX_INY_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, ref byte value) {
        value++;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool DEX_DEY_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, ref byte value) {
        value--;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);

        return false;
    }

    #endregion

    #region Arithmetic Operations

    [MethodImpl(MethodOptions.Options)]
    public static bool ADC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return ADC_SBC_Internal(operationCycle, cpu, ref cpuInternal, cpuInternal.DataBus);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SBC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return ADC_SBC_Internal(operationCycle, cpu, ref cpuInternal, (byte) ~cpuInternal.DataBus);
    }

    [MethodImpl(MethodOptions.Options)]
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

    [MethodImpl(MethodOptions.Options)]
    public static bool AND(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A &= cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool EOR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A ^= cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ORA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A |= cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    #endregion

    #region Shift & Rotate Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool LSR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        return LSR_Internal(operationCycle, cpu, ref cpuInternal, impliedAddressing);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ASL(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        bool completed = RmwWithImpliedInternal(
            operationCycle,
            impliedAddressing,
            ref cpuInternal,
            out bool mustHandle
        );

        if (mustHandle) {
            ref byte value = ref cpuInternal.Temporary.Low;
            if (impliedAddressing) {
                value = ref cpu.A;
            }

            byte input = value;
            value = (byte) (value << 1);

            SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);
            cpu.P.Carry = (input & 1 << 7) != 0;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ROL(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        bool completed = RmwWithImpliedInternal(
            operationCycle,
            impliedAddressing,
            ref cpuInternal,
            out bool mustHandle
        );

        if (mustHandle) {
            ref byte value = ref cpuInternal.Temporary.Low;
            if (impliedAddressing) {
                value = ref cpu.A;
            }

            byte input = value;
            value = (byte) ((value << 1) | (cpu.P.Carry ? 1 : 0));

            cpu.P.Carry = (input & 1 << 7) != 0;
            cpu.P.NegativeOrSign = (input & 1 << 6) != 0;
            cpu.P.Zero = value == 0;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ROR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        return ROR_Internal(operationCycle, cpu, ref cpuInternal, impliedAddressing);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ROR_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, bool impliedAddressing) {
        bool completed = RmwWithImpliedInternal(
            operationCycle,
            impliedAddressing,
            ref cpuInternal,
            out bool mustHandle
        );

        if (mustHandle) {
            ref CpuStatusRegisterFlags status = ref cpu.P;
            ref byte value = ref cpuInternal.Temporary.Low;
            if (impliedAddressing) {
                value = ref cpu.A;
            }

            byte input = value;
            value = (byte) (value >> 1);
            value |= (byte) (status.Carry ? 1 << 7 : 0);

            status.NegativeOrSign = status.Carry;
            status.Carry = (input & 1 << 0) != 0;
            status.Zero = value == 0;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool LSR_Internal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, bool impliedAddressing) {
        bool completed = RmwWithImpliedInternal(
            operationCycle,
            impliedAddressing,
            ref cpuInternal,
            out bool mustHandle
        );

        if (!completed && mustHandle) {
            ref CpuStatusRegisterFlags status = ref cpu.P;
            ref byte value = ref cpuInternal.Temporary.Low;
            if (impliedAddressing) {
                value = ref cpu.A;
            }

            byte input = value;
            value = (byte) (value >> 1);

            status.NegativeOrSign = false;
            status.Zero = value == 0;
            status.Carry = (input & 1 << 0) != 0;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool RmwWithImpliedInternal(
        int operationCycle,
        bool impliedAddressing,
        ref CpuInternalState cpuInternal,
        out bool mustHandle
    ) {
        mustHandle = false;
        if (impliedAddressing) {
            switch (operationCycle) {
                case AfterNextOpcodeFetched:
                    mustHandle = true;
                    break;
            }
        } else {
            switch (operationCycle) {
                case 0:
                    cpuInternal.Read = false;
                    cpuInternal.Temporary.Low = cpuInternal.DataBus;

                    mustHandle = true;
                    break;
                case 1:
                    cpuInternal.DataBus = cpuInternal.Temporary.Low;
                    return true;
            }
        }

        return false;
    }

    #endregion

    #region Flag Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool CLC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Carry = false;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLD(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.DecimalMode = false;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLI(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.InterruptDisable = false;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLV(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Overflow = false;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SEC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Carry = true;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SED(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.DecimalMode = true;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SEI(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.InterruptDisable = true;
        return false;
    }

    #endregion

    #region Comparisons

    [MethodImpl(MethodOptions.Options)]
    public static bool CMP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, cpu, ref cpuInternal, cpu.A);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CPX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, cpu, ref cpuInternal, cpu.X);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CPY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return CMP_Internal(operationCycle, cpu, ref cpuInternal, cpu.Y);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool CMP_Internal(
        int operationCycle,
        CpuState cpu,
        ref CpuInternalState cpuInternal,
        byte comparedValue
    ) {
        CMP_Internal_SetCompareFlags(operationCycle, cpu, ref cpuInternal, comparedValue);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
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

    [MethodImpl(MethodOptions.Options)]
    public static bool BCS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, cpu.P.Carry);

    [MethodImpl(MethodOptions.Options)]
    public static bool BCC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, !cpu.P.Carry);

    [MethodImpl(MethodOptions.Options)]
    public static bool BEQ(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, cpu.P.Zero);

    [MethodImpl(MethodOptions.Options)]
    public static bool BNE(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, !cpu.P.Zero);

    [MethodImpl(MethodOptions.Options)]
    public static bool BMI(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, cpu.P.NegativeOrSign);

    [MethodImpl(MethodOptions.Options)]
    public static bool BPL(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, !cpu.P.NegativeOrSign);

    [MethodImpl(MethodOptions.Options)]
    public static bool BVS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, cpu.P.Overflow);

    [MethodImpl(MethodOptions.Options)]
    public static bool BVC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, cpu, ref cpuInternal, !cpu.P.Overflow);

    [MethodImpl(MethodOptions.Options)]
    private static bool BranchInternal(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, bool condition) {
        switch (operationCycle) {
            case 0:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                cpuInternal.Temporary.Low = cpuInternal.DataBus; // offset
                return !condition;
            case 1: // branch taken
                cpuInternal.Temporary.Value = (ushort) (cpu.PC.Value + (sbyte) cpuInternal.Temporary.Low);
                cpuInternal.PageBoundaryCrossedCarry = (sbyte) (cpuInternal.Temporary.High - cpu.PC.High);
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                if (cpuInternal.PageBoundaryCrossedCarry == 0) {
                    cpu.PC = cpuInternal.AddressBus;
                }

                return cpuInternal.PageBoundaryCrossedCarry == 0;
            case 2: // page boundary crossed
                cpu.PC.Low = cpuInternal.Temporary.Low;
                cpuInternal.Temporary.High = (byte) (cpu.PC.High + cpuInternal.PageBoundaryCrossedCarry);
                cpuInternal.AddressBus = cpuInternal.Temporary;
                cpuInternal.State = CpuInternalState.ExecutionState.FetchNextInstructionOpcode;
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

    [MethodImpl(MethodOptions.Options)]
    public static bool JMP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC = cpuInternal.AddressBus;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool JSR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpu.PC.Value++;
                break;
            case 1:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
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
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 4:
                // subroutine address
                cpuInternal.AddressBus.High = cpuInternal.DataBus;
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpu.PC = cpuInternal.AddressBus;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool RTS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
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
                cpuInternal.AddressBus = cpu.PC;
                return true;
        }

        return false;
    }

    #endregion

    #region Interrupts

    [MethodImpl(MethodOptions.Options)]
    public static bool BRK(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return BRK_WithAddress(
            operationCycle,
            cpu,
            ref cpuInternal,
            CpuConstants.IrqVector
        );
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool BRK_WithAddress(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal, ushort vector) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.Read = false;

                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.PC.High;
                cpu.P.BreakFlag = true;

                cpu.SP--;
                break;
            case 1:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.PC.Low;
                cpu.SP--;
                break;
            case 2:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = (byte) (cpu.P.ToFlags() | CpuStatusRegisterPackedFlags.BreakFlag);
                cpu.SP--;
                break;
            case 3:
                cpuInternal.Read = true;
                cpuInternal.AddressBus.Value = vector;
                break;
            case 4:
                cpu.PC.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = (ushort) (vector + 1);
                break;
            case 5:
                cpu.PC.High = cpuInternal.DataBus;
                cpuInternal.AddressBus = cpu.PC;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool RTI(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                break;
            case 1:
                cpu.SP++;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 2:
                cpu.P = CpuStatusRegisterFlags.FromFlags((CpuStatusRegisterPackedFlags) cpuInternal.DataBus);
                cpu.P.BreakFlag = false;

                cpu.SP++;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 3:
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpu.SP++;
                break;
            case 4:
                cpu.PC.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus = cpu.PC;
                return true;
        }

        return false;
    }

    #endregion

    #region Other

    [MethodImpl(MethodOptions.Options)]
    public static bool BIT(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        byte result = (byte) (cpu.A & cpuInternal.DataBus);
        cpu.P.NegativeOrSign = (cpuInternal.DataBus & 1 << 7) != 0;
        cpu.P.Overflow = (cpuInternal.DataBus & 1 << 6) != 0;
        cpu.P.Zero = result == 0;

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool NOP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    #endregion

    #region Unofficial

    //[MethodImpl(MethodOptions.Options)]
    public static bool LAX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpuInternal.DataBus;
        cpu.X = cpuInternal.DataBus;
        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SAX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = (byte) (cpu.A & cpu.X);
                return true;
        }

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool DCP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
            case 1:
                return DEC(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                DEC(operationCycle, cpu, ref cpuInternal);
                CMP_Internal_SetCompareFlags(operationCycle, cpu, ref cpuInternal, cpu.A);
                break;
        }

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
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

    //[MethodImpl(MethodOptions.Options)]
    public static bool RLA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ROL(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ROL(operationCycle, cpu, ref cpuInternal);
                AND(operationCycle, cpu, ref cpuInternal);
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool RRA(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ROR(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ROR(operationCycle, cpu, ref cpuInternal);
                ADC(operationCycle, cpu, ref cpuInternal);
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SLO(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ASL(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ASL(operationCycle, cpu, ref cpuInternal);
                ORA(operationCycle, cpu, ref cpuInternal);
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SRE(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return LSR(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                LSR(operationCycle, cpu, ref cpuInternal);
                EOR(operationCycle, cpu, ref cpuInternal);
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool ANC(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return AND(operationCycle, cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                AND(operationCycle, cpu, ref cpuInternal);
                cpu.P.Carry = (cpu.A & 1 << 7) != 0;
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool ARR(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        // https://forums.nesdev.org/viewtopic.php?t=3831&start=15
        AND(operationCycle, cpu, ref cpuInternal);
        ROR_Internal(operationCycle, cpu, ref cpuInternal, true);

        SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
        cpu.P.Carry = (cpu.A & 1 << 6) != 0;
        cpu.P.Overflow = (((cpu.A & 1 << 5) >> 5) ^ ((cpu.A & 1 << 6) >> 6)) != 0;

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SHX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return SHX_SHY_Internal(operationCycle, cpu, ref cpuInternal, cpu.X);
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SHY(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        return SHX_SHY_Internal(operationCycle, cpu, ref cpuInternal, cpu.Y);
    }

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

    //[MethodImpl(MethodOptions.Options)]
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

    //[MethodImpl(MethodOptions.Options)]
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

    //[MethodImpl(MethodOptions.Options)]
    public static bool STP(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        cpuInternal.Halted = true;

        return true;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool AXS(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        byte andResult = (byte) (cpu.A & cpu.X);
        cpu.X = CMP_Internal_SetCompareFlags(operationCycle, cpu, ref cpuInternal, andResult);

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool XXX(int operationCycle, CpuState cpu, ref CpuInternalState cpuInternal) {
        Debug.WriteLine($"!!! Unhandled opcode ({cpuInternal.Instruction})");
        //throw new NotImplementedException($"!!! Unhandled opcode ({cpuInternal.Instruction})");

        return true;
    }

    #endregion

    [MethodImpl(MethodOptions.Options)]
    private static void SetGenericZeroAndNegativeSignFlags(ref CpuStatusRegisterFlags status, byte value) {
        status.Zero = value == 0;
        status.NegativeOrSign = (value & 1 << 7) != 0;
    }

    [MethodImpl(MethodOptions.Options)]
    private static ushort CalculateStackAddress(byte stackPointer)
        => (ushort) (0x100 + stackPointer);
}
