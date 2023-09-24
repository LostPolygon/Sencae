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

    public event Action? PpuClocked; 
    public event Action? CpuClocked;

    private bool _disposed;

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

    public void Reset(CpuResetOptions? options = null, bool hardReset = true) {
        AssertNotDisposed();

        State.ResetCounters();

        options ??= CpuResetOptions.Default;
        if (hardReset) {
            CpuBusDevice.HardReset(options.Value);
        } else {
            CpuBusDevice.SoftReset(options.Value);
        }

        PpuDevice.SoftReset();
    }

    [MethodImpl(MethodOptions.Options)]
    public void Clock() {
        AssertNotDisposed();

        if (State.MachineCycles % 3 == 0) {
            CpuClock();
        }

        PpuClock();

        State.MachineCycles++;
    }

    [MethodImpl(MethodOptions.Options)]
    public void PpuClock() {
        AssertNotDisposed();

        //
        if (State.CpuCycles % 27394 == 0) {
            State.Ppu.PPUSTATUS |= PpuStatusRegisterFlags.VerticalBlankStarted;
        }

        if (State.Ppu.Internal.CpuCyclesAtNmi != 0 && 
            State.CpuCycles - State.Ppu.Internal.CpuCyclesAtNmi == 2270) {
            State.Ppu.PPUSTATUS &= ~PpuStatusRegisterFlags.VerticalBlankStarted;
        }
        
        //
        if (State.CpuCycles % 28394 == 0) {
            State.Ppu.PPUSTATUS |= PpuStatusRegisterFlags.Sprite0Hit;
        }

        if (State.CpuCycles % 30000 == 0) {
            State.Ppu.PPUSTATUS &= ~PpuStatusRegisterFlags.Sprite0Hit;
        }

        //
        bool nmi =
            (State.Ppu.PPUSTATUS & PpuStatusRegisterFlags.VerticalBlankStarted) != 0 &&
            (State.Ppu.PPUCTRL & PpuControlRegisterFlags.GenerateNmi) != 0;
        
        if (nmi && nmi != State.Ppu.Internal.NmiPending) {
            State.Cpu.Internal.NmiPending = true;
            State.Ppu.Internal.CpuCyclesAtNmi = State.CpuCycles;
        }

        State.Ppu.Internal.NmiPending = nmi;
        
        PpuClocked?.Invoke();
    }

    [MethodImpl(MethodOptions.Options)]
    public void CpuClock() {
        AssertNotDisposed();

        ref CpuState cpu = ref State.Cpu;
        ref CpuInternalState cpuInternal = ref cpu.Internal;

        ushort address = cpuInternal.AddressBus.Value;
        MemoryAccessMode accessMode =
            cpuInternal.Read ?
                MemoryAccessMode.Read :
                MemoryAccessMode.Write;
        ref byte dataBus = ref cpuInternal.DataBus;

        // Assume that zero page and stack page are never IO mapped
        /*bool alwaysCpuMemory = address <= 0x01FF;
        if (alwaysCpuMemory) {
            ref byte mappedData = ref CpuBusDevice.Memory.Data[address];
            BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref dataBus);
        } else {
            CpuMemoryBus.HandleBus(address, accessMode, ref dataBus);
        }*/
        
        CpuMemoryBus.HandleBus(address, accessMode, ref dataBus);

        ExecuteState(ref cpu, ref cpuInternal);

        State.CpuCycles++;
        State.CpuProgramCycles++;
        
        CpuClocked?.Invoke();
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
        if (cpuInternal.NmiPending) {
            cpuInternal.Instruction = new(
                "BRK", 
                InstructionType.Internal_BRK_NMI,
                AddressingMode.Implied, 
                InstructionFlags.HasMemoryReadWriteLogic, 
                0x00,
                1
            );
            cpuInternal.State = CpuInternalState.ExecutionState.ExecutingAddressing;
            cpuInternal.OperationCycle = 0;
            instructionType = cpuInternal.Instruction.Type;

            cpuInternal.NmiPending = false;
        }
        
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

    [Conditional("DEBUG")]
    private void AssertNotDisposed() {
        Debug.Assert(!_disposed, "!_disposed");
    }

    private void ReleaseUnmanagedResources() {
        //CpuMemoryBus.Dispose();
        CpuBusDevice.Dispose();
        PpuCpuBusDevice.Dispose();

        //PpuMemoryBus.Dispose();
        PpuBusDevice.Dispose();

        CartridgeDevice.Dispose();
    }

    public void Dispose() {
        _disposed = true;

        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Machine() {
        ReleaseUnmanagedResources();
    }
}
