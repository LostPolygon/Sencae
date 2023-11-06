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
    public static bool Dispatch(ref CpuState cpu, ref CpuInternalState cpuInternal, InstructionType instructionType) {
        int operationCycle = cpuInternal.OperationCycle;

        return instructionType switch {
            // Transfer Instructions

            I.LDA => LDA(operationCycle, ref cpu, ref cpuInternal), // load accumulator
            I.LDX => LDX(operationCycle, ref cpu, ref cpuInternal), // load X
            I.LDY => LDY(operationCycle, ref cpu, ref cpuInternal), // load Y
            I.STA => STA(operationCycle, ref cpu, ref cpuInternal), // store accumulator
            I.STX => STX(operationCycle, ref cpu, ref cpuInternal), // store X
            I.STY => STY(operationCycle, ref cpu, ref cpuInternal), // store Y
            I.TAX => TAX(operationCycle, ref cpu, ref cpuInternal), // transfer accumulator to X
            I.TAY => TAY(operationCycle, ref cpu, ref cpuInternal), // transfer accumulator to Y
            I.TSX => TSX(operationCycle, ref cpu, ref cpuInternal), // transfer stack pointer to X
            I.TXA => TXA(operationCycle, ref cpu, ref cpuInternal), // transfer X to accumulator
            I.TXS => TXS(operationCycle, ref cpu, ref cpuInternal), // transfer X to stack pointer
            I.TYA => TYA(operationCycle, ref cpu, ref cpuInternal), // transfer Y to accumulator

            // Stack Instructions

            I.PHA => PHA(operationCycle, ref cpu, ref cpuInternal), // push accumulator
            I.PHP => PHP(operationCycle, ref cpu, ref cpuInternal), // push processor status register (with break flag set)
            I.PLA => PLA(operationCycle, ref cpu, ref cpuInternal), // pull accumulator
            I.PLP => PLP(operationCycle, ref cpu, ref cpuInternal), // pull processor status register 

            // Decrements & Increments

            I.DEC => DEC(operationCycle, ref cpu, ref cpuInternal), // decrement (memory)
            I.DEX => DEX(operationCycle, ref cpu, ref cpuInternal), // decrement X
            I.DEY => DEY(operationCycle, ref cpu, ref cpuInternal), // decrement Y
            I.INC => INC(operationCycle, ref cpu, ref cpuInternal), // increment (memory)
            I.INX => INX(operationCycle, ref cpu, ref cpuInternal), // increment X
            I.INY => INY(operationCycle, ref cpu, ref cpuInternal), // increment Y

            // Arithmetic Operations

            I.ADC => ADC(operationCycle, ref cpu, ref cpuInternal), // add with carry (prepare by CLC)
            I.SBC => SBC(operationCycle, ref cpu, ref cpuInternal), // subtract with carry (prepare by SEC)

            // Logical Operations

            I.AND => AND(operationCycle, ref cpu, ref cpuInternal), // and (with accumulator)
            I.EOR => EOR(operationCycle, ref cpu, ref cpuInternal), // exclusive or (with accumulator)
            I.ORA => ORA(operationCycle, ref cpu, ref cpuInternal), // (inclusive) or with accumulator

            // Shift & Rotate Instructions

            I.ASL => ASL(operationCycle, ref cpu, ref cpuInternal), // arithmetic shift left (shifts in a zero bit on the right)
            I.LSR => LSR(operationCycle, ref cpu, ref cpuInternal), // logical shift right (shifts in a zero bit on the left)
            I.ROL => ROL(operationCycle, ref cpu, ref cpuInternal), // rotate left (shifts in carry bit on the right)
            I.ROR => ROR(operationCycle, ref cpu, ref cpuInternal), // rotate right (shifts in zero bit on the left)

            // Flag Instructions

            I.CLC => CLC(operationCycle, ref cpu, ref cpuInternal), // clear carry
            I.CLD => CLD(operationCycle, ref cpu, ref cpuInternal), // clear decimal (BCD arithmetics disabled)
            I.CLI => CLI(operationCycle, ref cpu, ref cpuInternal), // clear interrupt disable
            I.CLV => CLV(operationCycle, ref cpu, ref cpuInternal), // clear overflow
            I.SEC => SEC(operationCycle, ref cpu, ref cpuInternal), // set carry
            I.SED => SED(operationCycle, ref cpu, ref cpuInternal), // set decimal (BCD arithmetics enabled)
            I.SEI => SEI(operationCycle, ref cpu, ref cpuInternal), // set interrupt disable

            // Comparisons
            I.CMP => CMP(operationCycle, ref cpu, ref cpuInternal), // compare (with accumulator)
            I.CPX => CPX(operationCycle, ref cpu, ref cpuInternal), // compare with X
            I.CPY => CPY(operationCycle, ref cpu, ref cpuInternal), // compare with Y 

            // Jumps & Subroutines

            I.JMP => JMP(operationCycle, ref cpu, ref cpuInternal), // jump
            I.JSR => JSR(operationCycle, ref cpu, ref cpuInternal), // jump subroutine
            I.RTS => RTS(operationCycle, ref cpu, ref cpuInternal), // return from subroutine 

            // Conditional Branch Instructions

            I.BCS => BCS(operationCycle, ref cpu, ref cpuInternal), // branch on carry set
            I.BCC => BCC(operationCycle, ref cpu, ref cpuInternal), // branch on carry clear
            I.BEQ => BEQ(operationCycle, ref cpu, ref cpuInternal), // branch on equal (zero set)
            I.BNE => BNE(operationCycle, ref cpu, ref cpuInternal), // branch on not equal (zero clear)
            I.BMI => BMI(operationCycle, ref cpu, ref cpuInternal), // branch on minus (negative set)
            I.BPL => BPL(operationCycle, ref cpu, ref cpuInternal), // branch on plus (negative clear)
            I.BVS => BVS(operationCycle, ref cpu, ref cpuInternal), // branch on overflow set 
            I.BVC => BVC(operationCycle, ref cpu, ref cpuInternal), // branch on overflow clear

            // Interrupts

            I.BRK => BRK(operationCycle, ref cpu, ref cpuInternal), // break / software interrupt
            I.Internal_BRK_NMI => BRK_WithAddress(operationCycle, ref cpu, ref cpuInternal, CpuConstants.NmiVector),
            I.RTI => RTI(operationCycle, ref cpu, ref cpuInternal), // return from interrupt 

            // Other

            I.BIT => BIT(operationCycle, ref cpu, ref cpuInternal), // bit test (accumulator & memory)
            I.NOP => NOP(operationCycle, ref cpu, ref cpuInternal), // no operation

            // Unofficial

            I.STP => STP(operationCycle, ref cpu, ref cpuInternal),
            I.SLO => SLO(operationCycle, ref cpu, ref cpuInternal),
            I.ANC => ANC(operationCycle, ref cpu, ref cpuInternal),
            I.RLA => RLA(operationCycle, ref cpu, ref cpuInternal),
            I.SRE => SRE(operationCycle, ref cpu, ref cpuInternal),
            I.ALR => ALR(operationCycle, ref cpu, ref cpuInternal),
            I.RRA => RRA(operationCycle, ref cpu, ref cpuInternal),
            I.SHY => SHY(operationCycle, ref cpu, ref cpuInternal),
            I.ARR => ARR(operationCycle, ref cpu, ref cpuInternal),
            I.SHX => SHX(operationCycle, ref cpu, ref cpuInternal),
            I.SAX => SAX(operationCycle, ref cpu, ref cpuInternal),
            I.XAA => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.AHX => AHX(operationCycle, ref cpu, ref cpuInternal),
            I.TAS => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.LAX => LAX(operationCycle, ref cpu, ref cpuInternal),
            I.LAS => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.DCP => DCP(operationCycle, ref cpu, ref cpuInternal),
            I.AXS => AXS(operationCycle, ref cpu, ref cpuInternal),
            I.ISC => ISC(operationCycle, ref cpu, ref cpuInternal),

            _ => XXX(operationCycle, ref cpu, ref cpuInternal)
        };
    }

    #endregion

    #region Transfer Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool LDA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpuInternal.DataBus;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpuInternal.DataBus;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.X);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.Y = cpuInternal.DataBus;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.Y);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.A;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.X;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool STY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.Y;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TAX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpu.A;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.X);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TAY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.Y = cpu.A;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.Y);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TSX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.X = cpu.SP;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.X);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TXA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpu.X;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TXS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.SP = cpu.X;

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TYA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpu.Y;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);

        return false;
    }

    #endregion

    #region Stack Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool PHA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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
    public static bool PLA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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

                SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PHP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);

                UpdateGenericZeroAndNegativeSignFlags(ref cpu);
                cpuInternal.DataBus = (byte) (cpu.P.ToFlags() | CpuStatusRegisterPackedFlags.BreakFlag);
                cpu.SP--;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PLP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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
                cpu.GenericZeroAndNegativeSignFlagsLazy = null;
                cpu.P = CpuStatusRegisterFlags.FromFlags((CpuStatusRegisterPackedFlags) cpuInternal.Temporary.Low);
                cpu.P.BreakFlag = false;

                break;
        }

        return false;
    }

    #endregion

    #region Decrements & Increments

    [MethodImpl(MethodOptions.Options)]
    public static bool INC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
                cpuInternal.Read = false;
                cpuInternal.Temporary.Low = (byte) (cpuInternal.DataBus + 1);
                break;
            case 1:
                cpuInternal.DataBus = cpuInternal.Temporary.Low;
                return true;
            case AfterNextOpcodeFetched:
                SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpuInternal.Temporary.Low);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool DEC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
                cpuInternal.Read = false;
                cpuInternal.Temporary.Low = (byte) (cpuInternal.DataBus - 1);
                break;
            case 1:
                cpuInternal.DataBus = cpuInternal.Temporary.Low;
                return true;
            case AfterNextOpcodeFetched:
                SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpuInternal.Temporary.Low);
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
        value++;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, value);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool DEX_DEY_Internal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, ref byte value) {
        value--;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, value);

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
        ushort result16 = (ushort) (cpu.A + data + (cpu.P.Carry ? 1 : 0));
        byte result = (byte) result16;

        cpu.P.Carry = result16 > Byte.MaxValue;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, result);

        cpu.P.Overflow = ((cpu.A ^ result) & (data ^ result) & 1 << 7) != 0;
        cpu.A = result;

        return false;
    }

    #endregion

    #region Logical Operations

    [MethodImpl(MethodOptions.Options)]
    public static bool AND(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A &= cpuInternal.DataBus;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool EOR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A ^= cpuInternal.DataBus;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ORA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A |= cpuInternal.DataBus;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);

        return false;
    }

    #endregion

    #region Shift & Rotate Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool LSR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        return LSR_Internal(operationCycle, ref cpu, ref cpuInternal, impliedAddressing);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ASL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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

            SetLazyGenericZeroAndNegativeSignFlags(ref cpu, value);
            cpu.P.Carry = (input & 1 << 7) != 0;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ROL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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
            
            cpu.GenericZeroAndNegativeSignFlagsLazy = null;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ROR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        return ROR_Internal(operationCycle, ref cpu, ref cpuInternal, impliedAddressing);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ROR_Internal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, bool impliedAddressing) {
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
            
            cpu.GenericZeroAndNegativeSignFlagsLazy = null;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool LSR_Internal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, bool impliedAddressing) {
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
            
            cpu.GenericZeroAndNegativeSignFlagsLazy = null;
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
    public static bool CLC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Carry = false;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLD(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.DecimalMode = false;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.InterruptDisable = false;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLV(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Overflow = false;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SEC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.Carry = true;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SED(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.DecimalMode = true;
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SEI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.P.InterruptDisable = true;
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
    private static bool CMP_Internal(
        int operationCycle,
        ref CpuState cpu,
        ref CpuInternalState cpuInternal,
        ref byte comparedValue
    ) {
        CMP_Internal_SetCompareFlags(operationCycle, ref cpu, ref cpuInternal, comparedValue);

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static byte CMP_Internal_SetCompareFlags(
        int operationCycle,
        ref CpuState cpu,
        ref CpuInternalState cpuInternal,
        byte comparedValue
    ) {
        byte result = (byte) (comparedValue - cpuInternal.DataBus);

        cpu.P.Carry = cpuInternal.DataBus <= comparedValue;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, result);

        return result;
    }

    #endregion

    #region Conditional Branch Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool BCS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.Carry);

    [MethodImpl(MethodOptions.Options)]
    public static bool BCC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.Carry);

    [MethodImpl(MethodOptions.Options)]
    public static bool BEQ(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        UpdateGenericZeroAndNegativeSignFlags(ref cpu);
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.Zero);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool BNE(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        UpdateGenericZeroAndNegativeSignFlags(ref cpu);
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.Zero);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool BMI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        UpdateGenericZeroAndNegativeSignFlags(ref cpu);
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.NegativeOrSign);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool BPL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        UpdateGenericZeroAndNegativeSignFlags(ref cpu);
        return BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.NegativeOrSign);
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool BVS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.Overflow);

    [MethodImpl(MethodOptions.Options)]
    public static bool BVC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, !cpu.P.Overflow);

    [MethodImpl(MethodOptions.Options)]
    private static bool BranchInternal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, bool condition) {
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
    public static bool JMP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC = cpuInternal.AddressBus;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool JSR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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
    public static bool RTS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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
    public static bool BRK(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return BRK_WithAddress(
            operationCycle,
            ref cpu,
            ref cpuInternal,
            CpuConstants.IrqVector
        );
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool BRK_WithAddress(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, ushort vector) {
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
                UpdateGenericZeroAndNegativeSignFlags(ref cpu);
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
    public static bool RTI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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
                cpu.GenericZeroAndNegativeSignFlagsLazy = null;

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
    public static bool BIT(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        byte result = (byte) (cpu.A & cpuInternal.DataBus);
        cpu.P.NegativeOrSign = (cpuInternal.DataBus & 1 << 7) != 0;
        cpu.P.Overflow = (cpuInternal.DataBus & 1 << 6) != 0;
        cpu.P.Zero = result == 0;
        cpu.GenericZeroAndNegativeSignFlagsLazy = null;

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool NOP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    #endregion

    #region Unofficial

    //[MethodImpl(MethodOptions.Options)]
    public static bool LAX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpu.A = cpuInternal.DataBus;
        cpu.X = cpuInternal.DataBus;
        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SAX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = (byte) (cpu.A & cpu.X);
                return true;
        }

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool DCP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case 0:
            case 1:
                return DEC(operationCycle, ref cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                DEC(operationCycle, ref cpu, ref cpuInternal);
                CMP_Internal_SetCompareFlags(operationCycle, ref cpu, ref cpuInternal, cpu.A);
                break;
        }

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool ISC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return INC(operationCycle, ref cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                INC(operationCycle, ref cpu, ref cpuInternal);
                SBC(operationCycle, ref cpu, ref cpuInternal);
                return false;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool RLA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ROL(operationCycle, ref cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ROL(operationCycle, ref cpu, ref cpuInternal);
                AND(operationCycle, ref cpu, ref cpuInternal);
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool RRA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ROR(operationCycle, ref cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ROR(operationCycle, ref cpu, ref cpuInternal);
                ADC(operationCycle, ref cpu, ref cpuInternal);
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SLO(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return ASL(operationCycle, ref cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                ASL(operationCycle, ref cpu, ref cpuInternal);
                ORA(operationCycle, ref cpu, ref cpuInternal);
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SRE(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return LSR(operationCycle, ref cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                LSR(operationCycle, ref cpu, ref cpuInternal);
                EOR(operationCycle, ref cpu, ref cpuInternal);
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool ANC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            default:
                return AND(operationCycle, ref cpu, ref cpuInternal);
            case AfterNextOpcodeFetched:
                AND(operationCycle, ref cpu, ref cpuInternal);
                cpu.P.Carry = (cpu.A & 1 << 7) != 0;
                return true;
        }
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool ARR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        // https://forums.nesdev.org/viewtopic.php?t=3831&start=15
        AND(operationCycle, ref cpu, ref cpuInternal);
        ROR_Internal(operationCycle, ref cpu, ref cpuInternal, true);

        SetLazyGenericZeroAndNegativeSignFlags(ref cpu, cpu.A);
        cpu.P.Carry = (cpu.A & 1 << 6) != 0;
        cpu.P.Overflow = (((cpu.A & 1 << 5) >> 5) ^ ((cpu.A & 1 << 6) >> 6)) != 0;

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SHX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return SHX_SHY_Internal(operationCycle, ref cpu, ref cpuInternal, cpu.X);
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool SHY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return SHX_SHY_Internal(operationCycle, ref cpu, ref cpuInternal, cpu.Y);
    }

    private static bool SHX_SHY_Internal(
        int operationCycle,
        ref CpuState cpu,
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
    public static bool AHX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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
    public static bool ALR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                AND(operationCycle, ref cpu, ref cpuInternal);
                LSR_Internal(operationCycle, ref cpu, ref cpuInternal, true);
                return true;
        }

        return false;
    }

    /////////////

    //[MethodImpl(MethodOptions.Options)]
    public static bool STP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        cpuInternal.Halted = true;

        return true;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool AXS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        byte andResult = (byte) (cpu.A & cpu.X);
        cpu.X = CMP_Internal_SetCompareFlags(operationCycle, ref cpu, ref cpuInternal, andResult);

        return false;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static bool XXX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        Debug.WriteLine($"!!! Unhandled opcode ({cpuInternal.Instruction})");
        //throw new NotImplementedException($"!!! Unhandled opcode ({cpuInternal.Instruction})");

        return true;
    }

    #endregion
    
    [MethodImpl(MethodOptions.Options)]
    private static void SetLazyGenericZeroAndNegativeSignFlags(ref CpuState cpu, byte value) {
        cpu.GenericZeroAndNegativeSignFlagsLazy = value;
    }

    [MethodImpl(MethodOptions.Options)]
    private static void UpdateGenericZeroAndNegativeSignFlags(ref CpuState cpu) {
        if (cpu.GenericZeroAndNegativeSignFlagsLazy != null) {
            byte value = cpu.GenericZeroAndNegativeSignFlagsLazy.Value;
            cpu.P.Zero = value == 0;
            cpu.P.NegativeOrSign = (value & 1 << 7) != 0;
            
            cpu.GenericZeroAndNegativeSignFlagsLazy = null;
        }
    }

    [MethodImpl(MethodOptions.Options)]
    private static ushort CalculateStackAddress(byte stackPointer)
        => (ushort) (0x100 + stackPointer);
}
