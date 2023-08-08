using System.Runtime.InteropServices;

namespace NGChip8Emulator; 

public struct CpuInternalState {
    public UInt16Bytes AddressBus;
    public UInt16Bytes AddressBusLatch;
    public UInt16Bytes EffectiveAddress;
    
    public UInt16Bytes BaseAddress;
    
    public byte DataBus;
    public byte DataLatch;
    public bool Read;
    
    public int OperationCycle;

    public byte IR;
    public byte OldIR;

    public ushort T;
    
    public ExecutionState State;

    //public int OperationCycle;

    public enum ExecutionState {
        ExecutingInstruction,
        //FinishingOldInstruction,
        FetchNextInstructionOpcode,
        ExecutingAddressing,
        InterpretOpcode
    }
}
