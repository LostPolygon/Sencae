using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae;

public unsafe class Machine : IDisposable {
    public CpuMemoryBus CpuMemoryBus;
    public CpuBusDevice CpuBusDevice;
    public PpuCpuBusDevice PpuCpuBusDevice;

    public PpuDevice PpuDevice;
    public PpuMemoryBus PpuMemoryBus;
    public PpuBusDevice PpuBusDevice;

    public CartridgeDevice CartridgeDevice;

    public MachineState State;

    public Machine(CartridgeDevice cartridgeDevice) {
        CartridgeDevice = cartridgeDevice;

        State = new MachineState();

        PpuDevice = new PpuDevice(this);

        CpuMemoryBus = new CpuMemoryBus(this);
        PpuMemoryBus = new PpuMemoryBus(this);

        CpuBusDevice = new CpuBusDevice(this);
        PpuBusDevice = new PpuBusDevice(this);
        PpuCpuBusDevice = new PpuCpuBusDevice(this);

        PpuMemoryBus = new PpuMemoryBus(this);
    }

    public void Reset(ushort? overrideResetVector = null, bool hardReset = true) {
        State.ResetCounters();

        if (hardReset) {
            CpuBusDevice.HardReset(overrideResetVector);
        } else {
            CpuBusDevice.SoftReset(overrideResetVector);
        }

        PpuDevice.SoftReset();
    }

    [MethodImpl(MethodOptions.Options)]
    public void Clock() {
        if (State.MachineCycles % 3 == 0) {
            CpuClock();
        }

        PpuClock();

        State.MachineCycles++;
    }

    [MethodImpl(MethodOptions.Options)]
    public void PpuClock() {
        if (State.MachineCycles % 28000 == 0) {
            State.Ppu.PPUSTATUS |= PpuStatusRegisterFlags.VerticalBlankStarted;
        }

        if (State.MachineCycles % 29000 == 0) {
            State.Ppu.PPUSTATUS &= ~PpuStatusRegisterFlags.VerticalBlankStarted;
        }
    }

    [MethodImpl(MethodOptions.Options)]
    public void CpuClock() {
        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        ushort address = cpuInternal.AddressBus.Value;
        bool isWrite = !cpuInternal.Read;
        ref byte dataBus = ref cpuInternal.DataBus;

        // Assume that zero page and stack page are never IO mapped
        bool alwaysCpuMemory = address <= 0x01FF;
        if (alwaysCpuMemory) {
            ref byte mappedData = ref CpuBusDevice.Memory.Data[address];
            BusDeviceHandlers.ReadWrite(ref mappedData, isWrite, ref dataBus);
        } else {
            CpuMemoryBus.HandleBus(address, isWrite, ref dataBus);
        }

        ExecuteState(ref cpu, ref cpuInternal);

        State.CpuCycles++;
        State.CpuProgramCycles++;
    }

    [MethodImpl(MethodOptions.Options)]
    private void ExecuteState(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        while (true) {
            switch (cpuInternal.State) {
                case CpuInternalState.ExecutionState.FetchNextInstructionOpcode: {
                    cpuInternal.OperationCycle = InstructionLogic.AfterNextOpcodeFetched;
                    DispatchInstructionLogic(ref cpu, ref cpuInternal, cpuInternal.Instruction.Type);

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
                        DispatchAddressingLogic(ref cpu, ref cpuInternal, cpuInternal.Instruction.AddressingMode);
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
                    bool completed = DispatchInstructionLogic(ref cpu, ref cpuInternal, cpuInternal.Instruction.Type);
                    if (cpuInternal.State == CpuInternalState.ExecutionState.FetchNextInstructionOpcode) {
                        cpuInternal.OperationCycle = InstructionLogic.AfterNextOpcodeFetched;
                        DispatchInstructionLogic(ref cpu, ref cpuInternal, cpuInternal.Instruction.Type);
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

        [MethodImpl(MethodOptions.Options)]
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
    private bool DispatchInstructionLogic(ref CpuState cpu, ref CpuInternalState cpuInternal, InstructionType instructionType) {
        bool completed = InstructionLogic.Dispatch(ref cpu, ref cpuInternal, instructionType);

        cpuInternal.OperationCycle++;
        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private bool DispatchAddressingLogic(ref CpuState cpu, ref CpuInternalState cpuInternal, AddressingMode addressingMode) {
        bool completed = AddressingModeLogic.Dispatch(ref cpu, ref cpuInternal, addressingMode);

        cpuInternal.OperationCycle++;
        return completed;
    }

    private void ReleaseUnmanagedResources() {
        //CpuMemoryBus.Dispose();
        CpuBusDevice.Dispose();
        PpuCpuBusDevice.Dispose();

        //PpuMemoryBus.Dispose();
        PpuBusDevice.Dispose();

        //CartridgeDevice.Dispose();
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Machine() {
        ReleaseUnmanagedResources();
    }
}
