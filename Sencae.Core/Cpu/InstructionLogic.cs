using System;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Local

namespace Sencae;

using I = InstructionType;

public static class InstructionLogic {
    public const int AfterAddressingFinished = -1;
    public const int AfterNextOpcodeFetched = 100;

    #region Dispatcher

    // Enabling inlining here actually reduces perf on x64 by about 25%!
    //[MethodImpl(MethodOptions.Options)]
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

            // Conditional Branch Instructions

            I.BCS => BCS(operationCycle, ref cpu, ref cpuInternal), // branch on carry set
            I.BCC => BCC(operationCycle, ref cpu, ref cpuInternal), // branch on carry clear
            I.BEQ => BEQ(operationCycle, ref cpu, ref cpuInternal), // branch on equal (zero set)
            I.BNE => BNE(operationCycle, ref cpu, ref cpuInternal), // branch on not equal (zero clear)
            I.BMI => BMI(operationCycle, ref cpu, ref cpuInternal), // branch on minus (negative set)
            I.BPL => BPL(operationCycle, ref cpu, ref cpuInternal), // branch on plus (negative clear)
            I.BVS => BVS(operationCycle, ref cpu, ref cpuInternal), // branch on overflow set 
            I.BVC => BVC(operationCycle, ref cpu, ref cpuInternal), // branch on overflow clear

            // Jumps & Subroutines

            I.JMP => JMP(operationCycle, ref cpu, ref cpuInternal), // jump
            I.JSR => JSR(operationCycle, ref cpu, ref cpuInternal), // jump subroutine
            I.RTS => RTS(operationCycle, ref cpu, ref cpuInternal), // return from subroutine 

            // Interrupts

            I.BRK => BRK(operationCycle, ref cpu, ref cpuInternal), // break / software interrupt
            I.RTI => RTI(operationCycle, ref cpu, ref cpuInternal), // return from interrupt 

            // Other

            I.BIT => BIT(operationCycle, ref cpu, ref cpuInternal), // bit test (accumulator & memory)
            I.NOP => NOP(operationCycle, ref cpu, ref cpuInternal), // no operation

            // Unofficial

            I.STP => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.SLO => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.ANC => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.RLA => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.SRE => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.ALR => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.RRA => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.SHY => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.ARR => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.SHX => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.SAX => SAX(operationCycle, ref cpu, ref cpuInternal),
            I.XAA => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.AHX => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.TAS => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.LAX => LAX(operationCycle, ref cpu, ref cpuInternal),
            I.LAS => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.DCP => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.AXS => XXX(operationCycle, ref cpu, ref cpuInternal),
            I.ISC => XXX(operationCycle, ref cpu, ref cpuInternal),

            _ => XXX(operationCycle, ref cpu, ref cpuInternal)
        };
    }

    #endregion

    #region Transfer Instructions

    [MethodImpl(MethodOptions.Options)]
    public static bool LDA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.A = cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.X = cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.Y = cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.Y);
                break;
        }

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
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.X = cpu.A;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TAY(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.Y = cpu.A;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.Y);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TSX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.X = cpu.SP;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.X);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TXA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.A = cpu.X;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TXS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.SP = cpu.X;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TYA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
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
            case AfterAddressingFinished:
                //cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.A;
                cpu.SP--;

                //cpu.PC.Value++;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PLA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                //cpu.PC.Value++;
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
    public static bool PHP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                //cpu.PC.Value++;
                //cpuInternal.AddressBus = cpu.PC;
                break;
            case 0:
                cpuInternal.Read = false;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);

                CpuStatusRegisterFlags statusFlags = cpu.P | CpuStatusRegisterFlags.BreakFlag;
                cpuInternal.DataBus = (byte) statusFlags;
                cpu.SP--;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool PLP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                //cpu.PC.Value++;
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
            case AfterAddressingFinished:
                break;
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
    public static bool DEC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterAddressingFinished:
                break;
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
            case AfterNextOpcodeFetched:
                value++;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, value);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool DEX_DEY_Internal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, ref byte value) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
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
            case AfterNextOpcodeFetched:
                ushort result16 = (ushort) (cpu.A + data + (cpu.P.HasFlagFast(CpuStatusRegisterFlags.Carry) ? 1 : 0));
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
            case AfterNextOpcodeFetched:
                cpu.A &= cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool EOR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.A ^= cpuInternal.DataBus;
                SetGenericZeroAndNegativeSignFlags(ref cpu.P, cpu.A);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ORA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
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
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        bool completed = RmwWithImpliedInternal(
            operationCycle,
            impliedAddressing,
            ref cpuInternal,
            out bool mustHandle
        );

        if (!completed && mustHandle) {
            ref CpuStatusRegisterFlags status = ref cpu.P;
            ref byte value = ref cpu.A;
            if (!impliedAddressing) {
                value = ref cpuInternal.Temporary.Low;
            }

            byte input = value;

            value = (byte) (value >> 1);
            status.RemoveFlag(CpuStatusRegisterFlags.NegativeOrSign);
            status.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);
            status.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 0) != 0);
        }

        return completed;
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
            ref CpuStatusRegisterFlags status = ref cpu.P;
            ref byte value = ref cpu.A;
            if (!impliedAddressing) {
                value = ref cpuInternal.Temporary.Low;
            }

            byte input = value;

            value = (byte) (value << 1);
            SetGenericZeroAndNegativeSignFlags(ref status, value);
            status.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 7) != 0);
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
            ref CpuStatusRegisterFlags status = ref cpu.P;
            ref byte value = ref cpu.A;
            if (!impliedAddressing) {
                value = ref cpuInternal.Temporary.Low;
            }

            byte input = value;

            value = (byte) ((value << 1) | (status.HasFlagFast(CpuStatusRegisterFlags.Carry) ? 1 : 0));

            status.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 7) != 0);
            status.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (input & 1 << 6) != 0);
            status.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool ROR(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        bool impliedAddressing = cpuInternal.Instruction.AddressingMode == AddressingMode.Implied;
        bool completed = RmwWithImpliedInternal(
            operationCycle,
            impliedAddressing,
            ref cpuInternal,
            out bool mustHandle
        );

        if (mustHandle) {
            ref CpuStatusRegisterFlags status = ref cpu.P;
            ref byte value = ref cpu.A;
            if (!impliedAddressing) {
                value = ref cpuInternal.Temporary.Low;
            }

            byte input = value;

            status.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, status.HasFlagFast(CpuStatusRegisterFlags.Carry));
            value = (byte) (value >> 1);
            value |= (byte) (status.HasFlagFast(CpuStatusRegisterFlags.Carry) ? 1 << 7 : 0);
            status.SetFlag(CpuStatusRegisterFlags.Carry, (input & 1 << 0) != 0);

            status.SetFlag(CpuStatusRegisterFlags.Zero, value == 0);
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
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.P.RemoveFlag(CpuStatusRegisterFlags.Carry);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLD(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.P.RemoveFlag(CpuStatusRegisterFlags.DecimalMode);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.P.RemoveFlag(CpuStatusRegisterFlags.InterruptDisable);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool CLV(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.P.RemoveFlag(CpuStatusRegisterFlags.Overflow);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SEC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.P.SetFlag(CpuStatusRegisterFlags.Carry);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SED(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                cpu.P.SetFlag(CpuStatusRegisterFlags.DecimalMode);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool SEI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
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
            case AfterNextOpcodeFetched:
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
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlagFast(CpuStatusRegisterFlags.Carry));

    [MethodImpl(MethodOptions.Options)]
    public static bool BCC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasNoFlag(CpuStatusRegisterFlags.Carry));

    [MethodImpl(MethodOptions.Options)]
    public static bool BEQ(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlagFast(CpuStatusRegisterFlags.Zero));

    [MethodImpl(MethodOptions.Options)]
    public static bool BNE(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasNoFlag(CpuStatusRegisterFlags.Zero));

    [MethodImpl(MethodOptions.Options)]
    public static bool BMI(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlagFast(CpuStatusRegisterFlags.NegativeOrSign));

    [MethodImpl(MethodOptions.Options)]
    public static bool BPL(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasNoFlag(CpuStatusRegisterFlags.NegativeOrSign));

    [MethodImpl(MethodOptions.Options)]
    public static bool BVS(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasFlagFast(CpuStatusRegisterFlags.Overflow));

    [MethodImpl(MethodOptions.Options)]
    public static bool BVC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        BranchInternal(operationCycle, ref cpu, ref cpuInternal, cpu.P.HasNoFlag(CpuStatusRegisterFlags.Overflow));

    [MethodImpl(MethodOptions.Options)]
    private static bool BranchInternal(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal, bool condition) {
        switch (operationCycle) {
            case 0:
                cpu.PC.Value++;
                if (condition) {
                    cpu.PC.Value = (ushort) (cpu.PC.Value + (sbyte) cpuInternal.DataBus);
                }

                cpuInternal.AddressBus = cpu.PC;
                return true;
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
        ushort stackAddress = CalculateStackAddress(cpu.SP);
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = stackAddress;
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
        switch (operationCycle) {
            case AfterAddressingFinished:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;

                break;
            case 0:
                cpuInternal.Read = false;

                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.PC.High;
                cpu.P.SetFlag(CpuStatusRegisterFlags.BreakFlag);

                cpu.SP--;
                break;
            case 1:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = cpu.PC.Low;
                cpu.SP--;
                break;
            case 2:
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                cpuInternal.DataBus = (byte) cpu.P;
                cpu.SP--;
                break;
            case 3:
                cpuInternal.Read = true;
                cpuInternal.AddressBus.Value = 0xFFFE;
                break;
            case 4:
                cpu.PC.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = 0xFFFF;
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
                cpu.SP++;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 1:
                cpu.P = (CpuStatusRegisterFlags) cpuInternal.DataBus &
                    ~CpuStatusRegisterFlags.BreakFlag |
                    CpuStatusRegisterFlags.Unused;

                cpu.SP++;
                cpuInternal.AddressBus.Value = CalculateStackAddress(cpu.SP);
                break;
            case 2:
                cpu.PC.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus = cpu.PC;
                cpu.SP++;
                return true;
        }

        return false;
    }

    #endregion

    #region Other

    [MethodImpl(MethodOptions.Options)]
    public static bool BIT(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case AfterNextOpcodeFetched:
                byte result = (byte) (cpu.A & cpuInternal.DataBus);
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpuInternal.DataBus & 1 << 7) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Overflow, (cpuInternal.DataBus & 1 << 6) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, result == 0);
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool NOP(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
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
            case AfterNextOpcodeFetched:
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
            case AfterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = (byte) (cpu.A & cpu.X);
                return true;
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
