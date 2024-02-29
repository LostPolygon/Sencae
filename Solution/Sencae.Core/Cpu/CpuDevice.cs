using System;
using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae;

public sealed class CpuDevice<TCpuMemoryBus, TOamDmaDevice> 
    where TCpuMemoryBus : CpuMemoryBusBase 
    where TOamDmaDevice : DmaDevice
{
    public readonly CpuState Cpu;
    public readonly TCpuMemoryBus CpuMemoryBus;
    public readonly TOamDmaDevice OamDmaDevice;

    public delegate void CpuBusHandlingDelegate(ushort address, bool isRead, byte busData);

    public event CpuBusHandlingDelegate? CpuBusHandling;

    public CpuDevice(CpuState cpu, TCpuMemoryBus cpuMemoryBus, TOamDmaDevice oamDmaDevice) {
        Cpu = cpu;
        CpuMemoryBus = cpuMemoryBus;
        OamDmaDevice = oamDmaDevice;
    }

    [MethodImpl(HotPath)]
    public void Clock() {
        ref CpuInternalState cpuInternal = ref Cpu.Internal;

        if (cpuInternal.Halted)
            return;

        /*if (cpuCycles < 7) {
            cpuCycles++;
            return;
        }*/

        ushort address = cpuInternal.AddressBus.Value;
        MemoryAccessMode accessMode =
            cpuInternal.Read ?
                MemoryAccessMode.Read :
                MemoryAccessMode.Write;

        bool mustExecuteState = true;
        OamDmaDevice.Execute(accessMode, ref mustExecuteState);
        
        ref byte dataBus = ref cpuInternal.DataBus;
        CpuMemoryBus.HandleBus(address, accessMode, ref dataBus);
        CpuBusHandling?.Invoke(address, accessMode == MemoryAccessMode.Read, dataBus);

        if (mustExecuteState) {
            ExecuteState(Cpu, ref cpuInternal);
        }

        Cpu.Internal.Cycles++;
    }

    public void HardReset(CpuResetOptions options) {
        Cpu.Reset();

        switch (options.CpuMemoryResetState) {
            case CpuMemoryResetState.Random:
                Random random = new();
                for (ushort i = 0x0000; i < 0x2000; i++) {
                    CpuMemoryBus.Write(i, (byte) random.Next());
                }

                break;
            case CpuMemoryResetState.Pattern:
                Span<byte> pattern = [0xDE, 0xAD, 0xBE, 0xEF];

                for (ushort i = 0x0000; i < 0x2000; i++) {
                    CpuMemoryBus.Write(i, pattern[i % 4]);
                }

                break;
            case CpuMemoryResetState.AllZero:
                for (ushort i = 0x0000; i < 0x2000; i++) {
                    CpuMemoryBus.Write(i, 0);
                }

                break;
            case CpuMemoryResetState.AllOne:
                for (ushort i = 0x0000; i < 0x2000; i++) {
                    CpuMemoryBus.Write(i, 0);
                }

                break;
            case CpuMemoryResetState.Uninitialized:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Cpu.P = new CpuStatusRegisterFlags {
            InterruptDisable = true,
            BreakFlag = true
        };

        Cpu.A = Cpu.X = Cpu.Y = 0x00;
        Cpu.SP = 0xFD;

        CpuMemoryBus.Write(0x4017, 0); // frame irq enabled
        CpuMemoryBus.Write(0x4015, 0); // all APU channels disabled

        for (ushort i = 0x4000; i <= 0x400F; i++) {
            CpuMemoryBus.Write(i, 0);
        }

        for (ushort i = 0x4010; i <= 0x4013; i++) {
            CpuMemoryBus.Write(i, 0);
        }

        GenericReset(options);
    }

    public void SoftReset(CpuResetOptions options) {
        Cpu.P.InterruptDisable = true;
        Cpu.SP -= 3;

        GenericReset(options);
    }

    public void ApplyResetVector(ushort? overrideResetVector) {
        ushort resetVector = overrideResetVector ?? CpuMemoryBus.ReadU16(CpuConstants.ResetVector);
        Cpu.PC.Value = (ushort) (resetVector + 0);

        Cpu.Internal.State = CpuInternalState.ExecutionState.InterpretingFetchedOpcode;
        Cpu.Internal.DataBus = CpuMemoryBus.ReadObserve(resetVector);
        Cpu.Internal.Instruction = Instructions.DescriptorByOpcode[0x00];

        Cpu.Internal.AddressBus.Value = (ushort) (resetVector + 0);
        Cpu.Internal.Read = true;
        Cpu.Internal.Halted = false;
    }

    private void GenericReset(CpuResetOptions options) {
        CpuMemoryBus.Write(0x4015, 0); // all APU channels disabled

        ApplyResetVector(options.OverrideResetVector);
    }
    [MethodImpl(HotPath)]
    private static void ExecuteState(CpuState cpu, ref CpuInternalState cpuInternal) {
        while (true) {
            switch (cpuInternal.State) {
                case CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode: {
                    // Execute current opcode
                    cpuInternal.OperationCycle = InstructionLogic.AfterNextOpcodeFetched;
                    InstructionLogic.Dispatch(cpu, ref cpuInternal, cpuInternal.Instruction.Type);

                    // Set up fetching next opcode
                    cpuInternal.Read = true;

                    // Implied mode operations read the same address the second time
                    // and throw the result away
                    if (cpuInternal.Instruction.AddressingMode != AddressingMode.Implied) {
                        cpu.PC.Value++;
                        cpuInternal.AddressBus = cpu.PC;
                    }

                    cpuInternal.State = CpuInternalState.ExecutionState.InterpretingFetchedOpcode;

                    break;
                }
                case CpuInternalState.ExecutionState.InterpretingFetchedOpcode: {
                    byte instructionOpcode = cpuInternal.DataBus;
                    cpuInternal.Instruction = Instructions.DescriptorByOpcode[instructionOpcode];

                    cpuInternal.State = CpuInternalState.ExecutionState.ExecutingAddressing;
                    cpuInternal.OperationCycle = 0;

                    if (CheckCpuNmi(cpu, ref cpuInternal))
                        break;

                    cpu.PC.Value++;
                    cpuInternal.AddressBus = cpu.PC;

                    if ((cpuInternal.Instruction.Flags & InstructionFlags.MemoryDataOperation) != 0 &&
                        cpuInternal.Instruction.AddressingMode == AddressingMode.Immediate) {
                        continue;
                    }

                    break;
                }
                case CpuInternalState.ExecutionState.ExecutingAddressing: {
                    bool completed =
                        cpuInternal.Instruction.Type == InstructionType.JSR ||
                        AddressingModeLogic.Dispatch(cpu, ref cpuInternal, cpuInternal.Instruction.AddressingMode);

                    cpuInternal.OperationCycle++;
                    if (!completed)
                        break;

                    if ((cpuInternal.Instruction.Flags & InstructionFlags.MemoryDataOperation) != 0) {
                        cpuInternal.State = CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode;
                        break;
                    }

                    bool canSkipToNextOpcode =
                        cpuInternal.Instruction.AddressingMode is AddressingMode.Implied or AddressingMode.Immediate &&
                        (cpuInternal.Instruction.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0;

                    if (canSkipToNextOpcode) {
                        cpuInternal.State = CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode;
                    } else {
                        cpuInternal.State = CpuInternalState.ExecutionState.ExecutingInstruction;
                        cpuInternal.OperationCycle = InstructionLogic.AfterAddressingFinished;
                    }

                    continue;
                }
                case CpuInternalState.ExecutionState.ExecutingInstruction: {
                    /*Debug.Assert(
                        !(
                            cpuInternal.OperationCycle == InstructionLogic.AfterAddressingFinished &&
                            (cpuInternal.Instruction.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0 &&
                            (cpuInternal.Instruction.Flags & InstructionFlags.MemoryDataOperation) == 0
                        )
                    );*/
                    bool completed = InstructionLogic.Dispatch(cpu, ref cpuInternal, cpuInternal.Instruction.Type);
                    cpuInternal.OperationCycle++;

                    if (completed) {
                        // Flow control instructions already setup PC and AB to needed values,
                        // other instructions can just reuse common logic
                        // for incrementing PC and fetching the next opcode
                        if ((cpuInternal.Instruction.Flags & InstructionFlags.FetchesNextOpcode) != 0) {
                            cpuInternal.State = CpuInternalState.ExecutionState.InterpretingFetchedOpcode;
                        } else {
                            cpuInternal.State = CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode;
                        }

                        cpuInternal.OperationCycle = 0;
                    }

                    break;
                }
            }

            break;
        }
    }

    private static bool CheckCpuNmi(CpuState cpu, ref CpuInternalState cpuInternal) {
        if (!cpuInternal.NmiFlipFlop ||
            cpuInternal.Instruction.Type == InstructionType.SpecialNmiHandler)
            return false;

        cpuInternal.Instruction = Instructions.DescriptorByOpcode[0x00] with {
            Type = InstructionType.SpecialNmiHandler
        };

        return true;
    }
}
