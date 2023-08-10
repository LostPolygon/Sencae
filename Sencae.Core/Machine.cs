using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public unsafe class Machine {
    public MachineState State;
    public CpuMemoryBus CpuBus;
    public Cartridge Cartridge;

    public Machine(Cartridge cartridge) {
        Cartridge = cartridge;
        CpuBus = new CpuMemoryBus(this);
    }

    public void Reset(ushort? overrideResetVector = null) {
        //State = default;

        ref CpuState cpu = ref State.Cpu;

        cpu.P = CpuStatusRegisterFlags.InterruptDisable | CpuStatusRegisterFlags.BreakFlag | CpuStatusRegisterFlags.Unused;

        cpu.A = cpu.X = cpu.Y = 0x00;
        cpu.SP = 0xFF;

        CpuBus.GetU8(0x4017) = 0; // frame irq enabled
        CpuBus.GetU8(0x4015) = 0; // all APU channels disabled

        CpuBus.GetU8(0x4010) = 0;
        CpuBus.GetU8(0x4011) = 0;
        CpuBus.GetU8(0x4012) = 0;
        CpuBus.GetU8(0x4013) = 0;
        
        ushort resetVector = overrideResetVector ?? CpuBus.GetU16(0xFFFC);
        cpu.Internal.State = CpuInternalState.ExecutionState.InterpretOpcode;
        cpu.Internal.DataBus = CpuBus.GetU8(resetVector);
        cpu.Internal.Instruction = Instructions.DescriptorByOpcode[cpu.Internal.DataBus];
        
        cpu.PC.Value = (ushort) (resetVector + 0);
        cpu.Internal.AddressBus.Value = (ushort) (resetVector + 0);
        cpu.Internal.Read = true;
    }

    public ref MachineState GetState() {
        return ref State;
    }

    [MethodImpl(MethodOptions.Options)]
    public void Clock() {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        ref byte dataBus = ref CpuBus.GetU8(cpuInternal.AddressBus);
        if (!cpuInternal.Read) {
            dataBus = cpuInternal.DataBus;
        } else {
            cpuInternal.DataBus = dataBus;
        }

        ExecuteState();

        State.Cycle++;
    }

    [MethodImpl(MethodOptions.Options)]
    private void ExecuteState() {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        while (true) {
            switch (cpuInternal.State) {
                case CpuInternalState.ExecutionState.FetchNextInstructionOpcode: {
                    cpuInternal.OperationCycle = 100;
                    bool completed = InstructionCycle(cpuInternal.Instruction.Type);

                    cpu.PC.Value++;
                    cpuInternal.AddressBus = cpu.PC;
                    cpuInternal.Read = true;

                    cpuInternal.State = CpuInternalState.ExecutionState.InterpretOpcode;
                    break;
                }
                case CpuInternalState.ExecutionState.InterpretOpcode: {
                    byte instructionOpcode = cpuInternal.DataBus;
                    cpuInternal.Instruction = Instructions.DescriptorByOpcode[instructionOpcode];

                    cpuInternal.State = CpuInternalState.ExecutionState.ExecutingAddressing;
                    cpuInternal.OperationCycle = 0;
                    continue;
                }
                case CpuInternalState.ExecutionState.ExecutingAddressing: {
                    bool completed =
                        (cpuInternal.Instruction.Flags & InstructionFlags.SpecialAddressing) != 0 ||
                        LoadEffectiveAddress(cpuInternal.Instruction.Type, cpuInternal.Instruction.AddressingMode);
                    if (completed) {
                        cpuInternal.OperationCycle = -1;
                        cpuInternal.State = CpuInternalState.ExecutionState.ExecutingInstruction;

                        continue;
                    }

                    break;
                }
                case CpuInternalState.ExecutionState.ExecutingInstruction: {
                    bool completed = InstructionCycle(cpuInternal.Instruction.Type);
                    if (completed) {
                        // Flow control instructions already setup PC and AB to needed values,
                        // other instructions can just reuse common logic
                        // for incrementing PC and fetching the next opcode
                        cpuInternal.State =
                            (cpuInternal.Instruction.Flags & InstructionFlags.FetchesNextOpcode) != 0 ?
                                CpuInternalState.ExecutionState.InterpretOpcode :
                                CpuInternalState.ExecutionState.FetchNextInstructionOpcode;

                        cpuInternal.OperationCycle = 0;
                    }

                    break;
                }
            }

            break;
        }
    }

    [MethodImpl(MethodOptions.Options)]
    private bool InstructionCycle(InstructionType instructionType) {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        int operationCycle = cpuInternal.OperationCycle;

        bool completed = instructionType switch {
            // Transfer Instructions

            InstructionType.LDA => InstructionLogic.LDA(operationCycle, ref cpu, ref cpuInternal), // load accumulator
            InstructionType.LDX => InstructionLogic.LDX(operationCycle, ref cpu, ref cpuInternal), // load X
            InstructionType.LDY => InstructionLogic.LDY(operationCycle, ref cpu, ref cpuInternal), // load Y
            InstructionType.STA => InstructionLogic.STA(operationCycle, ref cpu, ref cpuInternal), // store accumulator
            InstructionType.STX => InstructionLogic.STX(operationCycle, ref cpu, ref cpuInternal), // store X
            InstructionType.STY => InstructionLogic.STY(operationCycle, ref cpu, ref cpuInternal), // store Y
            InstructionType.TAX => InstructionLogic.TAX(operationCycle, ref cpu, ref cpuInternal), // transfer accumulator to X
            InstructionType.TAY => InstructionLogic.TAY(operationCycle, ref cpu, ref cpuInternal), // transfer accumulator to Y
            InstructionType.TSX => InstructionLogic.TSX(operationCycle, ref cpu, ref cpuInternal), // transfer stack pointer to X
            InstructionType.TXA => InstructionLogic.TXA(operationCycle, ref cpu, ref cpuInternal), // transfer X to accumulator
            InstructionType.TXS => InstructionLogic.TXS(operationCycle, ref cpu, ref cpuInternal), // transfer X to stack pointer
            InstructionType.TYA => InstructionLogic.TYA(operationCycle, ref cpu, ref cpuInternal), // transfer Y to accumulator

            // Stack Instructions

            InstructionType.PHA => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal), // push accumulator
            InstructionType.PHP => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal), // push processor status register (with break flag set)
            InstructionType.PLA => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal), // pull accumulator
            InstructionType.PLP => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal), // pull processor status register 

            // Decrements & Increments

            InstructionType.DEC => InstructionLogic.DEC(operationCycle, ref cpu, ref cpuInternal), // decrement (memory)
            InstructionType.DEX => InstructionLogic.DEX(operationCycle, ref cpu, ref cpuInternal), // decrement X
            InstructionType.DEY => InstructionLogic.DEY(operationCycle, ref cpu, ref cpuInternal), // decrement Y
            InstructionType.INC => InstructionLogic.INC(operationCycle, ref cpu, ref cpuInternal), // increment (memory)
            InstructionType.INX => InstructionLogic.INX(operationCycle, ref cpu, ref cpuInternal), // increment X
            InstructionType.INY => InstructionLogic.INY(operationCycle, ref cpu, ref cpuInternal), // increment Y

            // Arithmetic Operations

            InstructionType.ADC => InstructionLogic.ADC(operationCycle, ref cpu, ref cpuInternal), // add with carry (prepare by CLC)
            InstructionType.SBC => InstructionLogic.SBC(operationCycle, ref cpu, ref cpuInternal), // subtract with carry (prepare by SEC)

            // Logical Operations

            InstructionType.AND => InstructionLogic.AND(operationCycle, ref cpu, ref cpuInternal), // and (with accumulator)
            InstructionType.EOR => InstructionLogic.EOR(operationCycle, ref cpu, ref cpuInternal), // exclusive or (with accumulator)
            InstructionType.ORA => InstructionLogic.ORA(operationCycle, ref cpu, ref cpuInternal), // (inclusive) or with accumulator

            // Shift & Rotate Instructions

            InstructionType.ASL => InstructionLogic.ASL(operationCycle, ref cpu, ref cpuInternal), // arithmetic shift left (shifts in a zero bit on the right)
            InstructionType.LSR => InstructionLogic.LSR(operationCycle, ref cpu, ref cpuInternal), // logical shift right (shifts in a zero bit on the left)
            InstructionType.ROL => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal), // rotate left (shifts in carry bit on the right)
            InstructionType.ROR => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal), // rotate right (shifts in zero bit on the left)

            // Flag Instructions

            InstructionType.CLC => InstructionLogic.CLC(operationCycle, ref cpu, ref cpuInternal), // clear carry
            InstructionType.CLD => InstructionLogic.CLD(operationCycle, ref cpu, ref cpuInternal), // clear decimal (BCD arithmetics disabled)
            InstructionType.CLI => InstructionLogic.CLI(operationCycle, ref cpu, ref cpuInternal), // clear interrupt disable
            InstructionType.CLV => InstructionLogic.CLV(operationCycle, ref cpu, ref cpuInternal), // clear overflow
            InstructionType.SEC => InstructionLogic.SEC(operationCycle, ref cpu, ref cpuInternal), // set carry
            InstructionType.SED => InstructionLogic.SED(operationCycle, ref cpu, ref cpuInternal), // set decimal (BCD arithmetics enabled)
            InstructionType.SEI => InstructionLogic.SEI(operationCycle, ref cpu, ref cpuInternal), // set interrupt disable

            // Comparisons
            InstructionType.CMP => InstructionLogic.CMP(operationCycle, ref cpu, ref cpuInternal), // compare (with accumulator)
            InstructionType.CPX => InstructionLogic.CPX(operationCycle, ref cpu, ref cpuInternal), // compare with X
            InstructionType.CPY => InstructionLogic.CPY(operationCycle, ref cpu, ref cpuInternal), // compare with Y 

            // Conditional Branch Instructions

            InstructionType.BCS => InstructionLogic.BCS(operationCycle, ref cpu, ref cpuInternal), // branch on carry set
            InstructionType.BCC => InstructionLogic.BCC(operationCycle, ref cpu, ref cpuInternal), // branch on carry clear
            InstructionType.BEQ => InstructionLogic.BEQ(operationCycle, ref cpu, ref cpuInternal), // branch on equal (zero set)
            InstructionType.BNE => InstructionLogic.BNE(operationCycle, ref cpu, ref cpuInternal), // branch on not equal (zero clear)
            InstructionType.BMI => InstructionLogic.BMI(operationCycle, ref cpu, ref cpuInternal), // branch on minus (negative set)
            InstructionType.BPL => InstructionLogic.BPL(operationCycle, ref cpu, ref cpuInternal), // branch on plus (negative clear)
            InstructionType.BVS => InstructionLogic.BVS(operationCycle, ref cpu, ref cpuInternal), // branch on overflow set 
            InstructionType.BVC => InstructionLogic.BVC(operationCycle, ref cpu, ref cpuInternal), // branch on overflow clear

            // Jumps & Subroutines

            InstructionType.JMP => InstructionLogic.JMP(operationCycle, ref cpu, ref cpuInternal), // jump
            InstructionType.JSR => InstructionLogic.JSR(operationCycle, ref cpu, ref cpuInternal), // jump subroutine
            InstructionType.RTS => InstructionLogic.RTS(operationCycle, ref cpu, ref cpuInternal), // return from subroutine 

            // Interrupts

            InstructionType.BRK => InstructionLogic.BRK(operationCycle, ref State, ref cpu, ref cpuInternal), // break / software interrupt
            InstructionType.RTI => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal), // return from interrupt 

            // Other

            InstructionType.BIT => InstructionLogic.BIT(operationCycle, ref cpu, ref cpuInternal), // bit test (accumulator & memory)
            InstructionType.NOP => InstructionLogic.NOP(operationCycle, ref State, ref cpu, ref cpuInternal), // no operation

            // Unofficial

            InstructionType.STP => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.SLO => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.ANC => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.RLA => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.SRE => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.ALR => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.RRA => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.SHY => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.ARR => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.SHX => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.SAX => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.XAA => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.AHX => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.TAS => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.LAX => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.LAS => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.DCP => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.AXS => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.ISC => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
            //_ => true
        };

        cpuInternal.OperationCycle++;
        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private bool LoadEffectiveAddress(InstructionType instructionDefinitionType, AddressingMode addressingMode) {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        bool completed = AddressingModeLogic.Execute(ref cpu, ref cpuInternal, addressingMode);
        cpuInternal.OperationCycle++;

        return completed;
    }
}
