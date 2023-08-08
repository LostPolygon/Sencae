using System.Runtime.CompilerServices;

namespace NGChip8Emulator;

public unsafe class Machine {
    public MachineState State;
    public CpuMemoryBus CpuBus;
    public Cartridge Cartridge;

    private CpuState* _cpu;
    private CpuInternalState* _cpuInternal;

    public Machine(Cartridge cartridge) {
        Cartridge = cartridge;
        CpuBus = new CpuMemoryBus(this);

        _cpu = &State.Cpu;
    }

    public void Reset() {
        State = default;

        ref CpuState cpu = ref State.Cpu;

        cpu.A = 0xAA;
        cpu.X = 0xC0;
        cpu.Y = 0x0;
        cpu.SP = 0xBD;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void ExecuteState() {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;
        
        while (true) {
            switch (cpuInternal.State) {
                case CpuInternalState.ExecutionState.FetchNextInstructionOpcode: {
                    bool completed = InstructionCycle(cpuInternal.Instruction.Type, true);

                    cpuInternal.Read = true;

                    cpu.PC.Value++;
                    cpuInternal.AddressBus = cpu.PC;

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
                    bool completed = InstructionCycle(cpuInternal.Instruction.Type, false);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private bool InstructionCycle(
        InstructionType instructionType,
        bool finishing
    ) {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;
        
        const int AfterAddressingFinished = -1;
        const int AfterNextOpcodeFetched = 100;
        int operationCycle = finishing ? 100 : cpuInternal.OperationCycle;

        bool completed = false;
        switch (instructionType) {
            case InstructionType.BRK:
                if (State.Cycle > 3) {
                    cpu.Halted = true;
                }

                completed = true;
                break;
            case InstructionType.TAX:
                switch (operationCycle) {
                    case AfterAddressingFinished:
                        completed = true;
                        break;
                    case AfterNextOpcodeFetched:
                        cpu.X = cpu.A;
                        break;
                }

                break;
            case InstructionType.INX:
                switch (operationCycle) {
                    case AfterAddressingFinished:
                        completed = true;
                        break;
                    case AfterNextOpcodeFetched:
                        cpu.X++;

                        if ((cpu.X & 0b10000000) != 0) {
                            cpu.P |= CpuState.StatusRegisterFlags.NegativeOrSign;
                        } else {
                            cpu.P &= ~CpuState.StatusRegisterFlags.NegativeOrSign;
                        }
                        
                        if (cpu.X == 0) {
                            cpu.P |= CpuState.StatusRegisterFlags.Zero;
                        } else {
                            cpu.P &= ~CpuState.StatusRegisterFlags.Zero;
                        }
                        break;
                }

                break;
            case InstructionType.STA:
                switch (operationCycle) {
                    case AfterAddressingFinished:
                        cpuInternal.Read = false;
                        cpuInternal.DataBus = cpu.A;
                        completed = true;
                        break;
                    case AfterNextOpcodeFetched:
                        break;
                }

                break;
            case InstructionType.LDA:
                switch (operationCycle) {
                    case AfterAddressingFinished:
                        cpuInternal.Read = true;
                        completed = true;
                        break;
                    case AfterNextOpcodeFetched:
                        cpu.A = cpuInternal.DataBus;
                        break;
                }

                break;
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
            default:
                completed = true;
                break;
        }

        cpuInternal.OperationCycle++;

        return completed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
