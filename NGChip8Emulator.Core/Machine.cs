using System;
using System.Runtime.CompilerServices;

namespace NGChip8Emulator;

public unsafe class Machine {
    public MachineState State;
    public CpuMemoryBus CpuBus;
    public Cartridge Cartridge;

    public Machine(Cartridge cartridge) {
        Cartridge = cartridge;
        CpuBus = new CpuMemoryBus(this);
    }

    public void Reset() {
        State = default;

        ref CpuState cpu = ref State.Cpu;
        
        cpu.P = CpuStatusRegisterFlags.IrqDisable | CpuStatusRegisterFlags.BreakFlag | CpuStatusRegisterFlags.Unused;

        cpu.A = cpu.X = cpu.Y = 0x00;
        cpu.SP = 0xFD;

        CpuBus.GetU8(0x4017) = 0; // frame irq enabled
        CpuBus.GetU8(0x4015) = 0; // all APU channels disabled
        
        CpuBus.GetU8(0x4010) = 0;
        CpuBus.GetU8(0x4011) = 0;
        CpuBus.GetU8(0x4012) = 0;
        CpuBus.GetU8(0x4013) = 0;
        
        ushort resetVector = CpuBus.GetU16(0xFFFC);
        cpu.Internal.State = CpuInternalState.ExecutionState.InterpretOpcode;
        cpu.Internal.DataBus = CpuBus.GetU8(resetVector);

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
                    cpuInternal.Instruction =  Instructions.DescriptorByOpcode[instructionOpcode];

                    cpuInternal.State = CpuInternalState.ExecutionState.ExecutingAddressing;
                    cpuInternal.OperationCycle = 0;
                    continue;
                }
                case CpuInternalState.ExecutionState.ExecutingAddressing: {
                    bool completed = 
                        (cpuInternal.Instruction.Flags & InstructionFlags.CustomAddressing) != 0 || 
                        LoadEffectiveAddress(cpuInternal.Instruction.Type, cpuInternal.Instruction.AddressMode);
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
                        cpuInternal.State = CpuInternalState.ExecutionState.FetchNextInstructionOpcode;
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
            InstructionType.STA => InstructionLogic.STA(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.LDA => InstructionLogic.LDA(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.TAX => InstructionLogic.TAX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.INX => InstructionLogic.INX(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.ADC => InstructionLogic.ADC(operationCycle, ref cpu, ref cpuInternal),
            InstructionType.BRK => InstructionLogic.BRK(operationCycle, ref State, ref cpu, ref cpuInternal),
            _ => InstructionLogic.XXX(operationCycle, ref cpu, ref cpuInternal),
        };
        /*switch (instructionType) {
            case InstructionType.JSR:
                ushort stackAddress = (ushort) (0x0100 + cpu.SP);
                switch (operationCycle) {
                    case 0:
                        cpuInternal.Read = true;
                        cpuInternal.Temporary = cpuInternal.DataBus;
                        cpuInternal.AddressBus.Value = stackAddress;
                        cpu.PC.Value++;
                        break;
                    case 1:
                        cpuInternal.Read = true;
                        cpuInternal.AddressBus.Value = stackAddress;
                        break;
                    case 2:
                        cpuInternal.Read = false;
                        cpuInternal.DataBus = cpu.PC.High;
                        cpu.SP--;
                        break;
                    case 3:
                        cpuInternal.Read = false;
                        cpuInternal.AddressBus.Value = stackAddress;
                        cpuInternal.DataBus = cpu.PC.Low;

                        //cpu.PC.Value += 1;
                        cpu.SP--;
                        break;
                    case 4:
                        cpuInternal.Read = true;
                        cpuInternal.AddressBus = cpu.PC;

                        UInt16Bytes u = new() {
                            High = cpuInternal.DataBus,
                            Low = (byte) cpuInternal.Temporary
                        };
                        cpu.PC = u;
                        cpuInternal.AddressBus = cpu.PC;
                        completed = true;
                        break;
                }

                break;
        }*/

        cpuInternal.OperationCycle++;
        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private bool LoadEffectiveAddress(InstructionType instructionDefinitionType, AddressMode addressMode) {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        bool completed = false;
        switch (addressMode) {
            case AddressMode.Implied:
                completed = true;
                break;
            case AddressMode.Immediate:
                switch (cpuInternal.OperationCycle) {
                    case 0:
                        cpu.PC.Value++;
                        cpuInternal.AddressBus = cpu.PC;
                        completed = true;
                        break;
                }

                break;
            case AddressMode.Absolute:
                switch (cpuInternal.OperationCycle) {
                    case 0:
                        //cpuInternal.AddressBus.Low = cpuInternal.DataBus;
                        cpu.PC.Value++;
                        cpuInternal.AddressBus = cpu.PC;
                        break;
                    case 1:
                        cpuInternal.Temporary = cpuInternal.DataBus;
                        cpu.PC.Value++;
                        cpuInternal.AddressBus = cpu.PC;
                        break;
                    case 2:
                        cpuInternal.AddressBus.Low = (byte) cpuInternal.Temporary;
                        cpuInternal.AddressBus.High = cpuInternal.DataBus;

                        //cpu.PC.Value++;
                        completed = true;
                        break;
                }

                break;
            /*case AddressMode.AbsoluteXIndexed:
                operandAddress = ReadU16AtPCAndAdvancePC() + cpu.X;
                break;
            case AddressMode.AbsoluteYIndexed:
                operandAddress = ReadU16AtPCAndAdvancePC() + cpu.Y;
                break;
            case AddressMode.ZeroPage:
                operandAddress = ReadU8AtPCAndAdvancePC();
                break;
            case AddressMode.ZeroPageXIndexed:
                operandAddress = ReadU8AtPCAndAdvancePC() + cpu.X;
                break;
            case AddressMode.ZeroPageYIndexed:
                operandAddress = ReadU8AtPCAndAdvancePC() + cpu.Y;
                break;
            case AddressMode.AbsoluteIndirect:
                operandAddress = CpuBus.GetU16(ReadU16AtPCAndAdvancePC());
                break;
            case AddressMode.ZeroPageIndirectXPreIndexed:
                operandAddress = CpuBus.GetU8(CpuBus.GetU8((ushort) (cpu.PC + cpu.X)));
                cpu.PC += 1;
                break;
            case AddressMode.ZeroPageIndirectYPostIndexed:
                operandAddress = CpuBus.GetU8((ushort) (CpuBus.GetU8(cpu.PC) + cpu.Y));
                cpu.PC += 1;
                break;
            case AddressMode.Relative:
                sbyte offset = (sbyte) ReadU8AtPCAndAdvancePC();
                operandAddress = cpu.PC + offset;
                break;*/
        }

        cpuInternal.OperationCycle++;

        return completed;
    }
}
