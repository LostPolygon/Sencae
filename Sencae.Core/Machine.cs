using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae;

public unsafe class Machine : IDisposable {
    public IBusDevice CartridgeDevice;

    public CpuDevice CpuDevice;
    public CpuMemoryBus CpuMemoryBus;

    public PpuDevice PpuDevice;

    public MachineState State;
    
    public Machine(IBusDevice cartridgeDevice) {
        State = new MachineState();
        CartridgeDevice = cartridgeDevice;
        CpuDevice = new CpuDevice(this);
        PpuDevice = new PpuDevice(this);
        CpuMemoryBus = new CpuMemoryBus(this);
    }

    public void Reset(ushort? overrideResetVector = null) {
        CpuDevice.HardReset(overrideResetVector);
        PpuDevice.Reset();

        State.Reset();
    }

    [MethodImpl(MethodOptions.Options)]
    public void Clock() {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        if (!cpuInternal.Read) {
            CpuMemoryBus.Write(cpuInternal.AddressBus.Value, cpuInternal.DataBus);
        } else {
            cpuInternal.DataBus = CpuMemoryBus.Read(cpuInternal.AddressBus.Value);
        }

        ExecuteState();

        State.Cycles++;
        State.ProgramCycles++;
    }

    [MethodImpl(MethodOptions.Options)]
    private void ExecuteState() {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        while (true) {
            switch (cpuInternal.State) {
                case CpuInternalState.ExecutionState.FetchNextInstructionOpcode: {
                    cpuInternal.OperationCycle = InstructionLogic.AfterNextOpcodeFetched;
                    DispatchInstructionLogic(cpuInternal.Instruction.Type);

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
                        DispatchAddressingLogic(cpuInternal.Instruction.AddressingMode);
                    if (completed) {
                        if ((cpuInternal.Instruction.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0) {
                            FinishInstruction(ref cpuInternal);
                        } else {
                            cpuInternal.OperationCycle = InstructionLogic.AfterAddressingFinished;
                            cpuInternal.State = CpuInternalState.ExecutionState.ExecutingInstruction;
                            continue;
                        }
                    }

                    break;
                }
                case CpuInternalState.ExecutionState.ExecutingInstruction: {
                    Debug.Assert(
                        !(cpuInternal.OperationCycle == InstructionLogic.AfterAddressingFinished && 
                            (cpuInternal.Instruction.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0)
                        );
                    bool completed = DispatchInstructionLogic(cpuInternal.Instruction.Type);
                    if (cpuInternal.State == CpuInternalState.ExecutionState.FetchNextInstructionOpcode) {
                        cpuInternal.OperationCycle = InstructionLogic.AfterNextOpcodeFetched;
                        DispatchInstructionLogic(cpuInternal.Instruction.Type);
                    }

                    if (completed) {
                        FinishInstruction(ref cpuInternal);
                    }

                    break;
                }
                default:
                    break;
                    //throw new ArgumentOutOfRangeException();
            }

            break;
        }

        static void FinishInstruction(ref CpuInternalState cpuInternal) {
            // Flow control instructions already setup PC and AB to needed values,
            // other instructions can just reuse common logic
            // for incrementing PC and fetching the next opcode
            cpuInternal.State =
                (cpuInternal.Instruction.Flags & InstructionFlags.FetchesNextOpcode) != 0 ?
                    CpuInternalState.ExecutionState.InterpretOpcode :
                    CpuInternalState.ExecutionState.FetchNextInstructionOpcode;

            cpuInternal.OperationCycle = 0;
        }
    }

    [MethodImpl(MethodOptions.Options)]
    private bool DispatchInstructionLogic(InstructionType instructionType) {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        bool completed = InstructionLogic.Dispatch(ref cpu, ref cpuInternal, instructionType);

        cpuInternal.OperationCycle++;
        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private bool DispatchAddressingLogic(AddressingMode addressingMode) {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        bool completed = AddressingModeLogic.Dispatch(ref cpu, ref cpuInternal, addressingMode);
        
        cpuInternal.OperationCycle++;
        return completed;
    }

    private void ReleaseUnmanagedResources() {
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Machine() {
        ReleaseUnmanagedResources();
    }
}
