using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sencae.Ppu;

namespace Sencae;

public sealed class CpuDevice<TCpuMemoryBus, TOamDmaDevice>(CpuState cpu, TCpuMemoryBus cpuMemoryBus, TOamDmaDevice oamDmaDevice)
    where TCpuMemoryBus : CpuMemoryBusBase
    where TOamDmaDevice : DmaDevice {
    public readonly CpuState Cpu = cpu;
    public readonly TCpuMemoryBus CpuMemoryBus = cpuMemoryBus;
    public readonly TOamDmaDevice OamDmaDevice = oamDmaDevice;

    public delegate void CpuBusHandlingDelegate(ushort address, bool isRead, byte busData);

    public event CpuBusHandlingDelegate? CpuBusHandling;

    [MethodImpl(HotPathBlock)]
    public bool Clock() {
        ref CpuInternalState cpuInternal = ref Cpu.Internal;

        if (cpuInternal.Halted)
            return false;

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
            ExecuteState(ref Cpu.Registers, ref cpuInternal);
        }

        cpuInternal.Cycles++;

        return true;
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

        Cpu.Registers.P = new CpuStatusRegisterFlags {
            InterruptDisable = true,
            BreakFlag = true
        };

        Cpu.Registers.A = Cpu.Registers.X = Cpu.Registers.Y = 0x00;
        Cpu.Registers.SP = 0xFD;

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
        Cpu.Registers.P.InterruptDisable = true;
        Cpu.Registers.SP -= 3;

        GenericReset(options);
    }

    public void ApplyResetVector(ushort? overrideResetVector) {
        ushort resetVector = overrideResetVector ?? CpuMemoryBus.ReadU16(CpuConstants.ResetVector);
        Cpu.Registers.PC.Value = (ushort) (resetVector + 0);

        Cpu.Internal.State = CpuInternalState.ExecutionState.InterpretingFetchedOpcode;
        Cpu.Internal.DataBus = CpuMemoryBus.ReadObserve(resetVector);
        Cpu.Internal.Instruction = Instructions.DescriptorSlimByOpcode[0x00];

        Cpu.Internal.AddressBus.Value = (ushort) (resetVector + 0);
        Cpu.Internal.Read = true;
        Cpu.Internal.Halted = false;
    }

    private void GenericReset(CpuResetOptions options) {
        CpuMemoryBus.Write(0x4015, 0); // all APU channels disabled

        ApplyResetVector(options.OverrideResetVector);
    }

    [MethodImpl(HotPathBlock)]
    private static void ExecuteState(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) {
        while (true) {
            switch (cpuInternal.State) {
                case CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode: {
                    // Execute current opcode
                    cpuInternal.OperationCycle = InstructionLogic.AfterNextOpcodeFetched;

                    if (cpuInternal.Instruction.HasAfterNextOpcodeFetchedLogic) {
                        InstructionLogic.Dispatch(ref cpu, ref cpuInternal, cpuInternal.Instruction.Type);
                    }

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
                    cpuInternal.State = CpuInternalState.ExecutionState.ExecutingAddressing;
                    cpuInternal.OperationCycle = 0;

                    if (CheckCpuNmi(ref cpuInternal))
                        break;

                    byte instructionOpcode = cpuInternal.DataBus;
                    cpuInternal.Instruction = Instructions.DescriptorSlimByOpcode[instructionOpcode];
                    
                    cpuInternal.InterruptType = InterruptType.Break;

                    cpu.PC.Value++;
                    cpuInternal.AddressBus = cpu.PC;

                    if (cpuInternal.Instruction.Type == InstructionType.JSR ||
                        cpuInternal.Instruction.AddressingMode is AddressingMode.Implied or AddressingMode.Immediate
                       ) {
                        cpuInternal.OperationCycle++;
                        if (cpuInternal.Instruction.MemoryDataOperation) {
                            cpuInternal.State = CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode;
                        }
                    }

                    break;
                }
                case CpuInternalState.ExecutionState.ExecutingAddressing: {
                    bool completed =
                        AddressingModeLogic.Dispatch(ref cpu, ref cpuInternal, cpuInternal.Instruction.AddressingMode);

                    cpuInternal.OperationCycle++;
                    if (!completed)
                        break;

                    if (cpuInternal.Instruction.MemoryDataOperation) {
                        cpuInternal.State = CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode;
                        break;
                    }

                    if (cpuInternal.Instruction.CanSkipToNextOpcode) {
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
                    bool completed = InstructionLogic.Dispatch(ref cpu, ref cpuInternal, cpuInternal.Instruction.Type);
                    cpuInternal.OperationCycle++;

                    if (completed) {
                        // Flow control instructions already setup PC and AB to needed values,
                        // other instructions can just reuse common logic
                        // for incrementing PC and fetching the next opcode
                        cpuInternal.State =
                            cpuInternal.Instruction.FetchesNextOpcode ?
                                CpuInternalState.ExecutionState.InterpretingFetchedOpcode :
                                CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode;

                        cpuInternal.OperationCycle = 0;
                    }

                    break;
                }
            }

            break;
        }
    }

    private static bool CheckCpuNmi(ref CpuInternalState cpuInternal) {
        if (!cpuInternal.NmiFlipFlop ||
            cpuInternal.Instruction.Type == InstructionType.BRK)
            return false;

        cpuInternal.Instruction = Instructions.DescriptorSlimByOpcode[0x00];
        cpuInternal.InterruptType = InterruptType.Nmi;

        return true;
    }
}
