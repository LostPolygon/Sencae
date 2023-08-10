namespace Sencae; 

public struct CpuInternalState {
    public UInt16Bytes AddressBus;
    //public UInt16Bytes BaseAddress;
    
    public byte DataBus;
    public bool Read;
    
    public int OperationCycle;

    //public byte InstructionOpcode;
    public InstructionDescriptor Instruction;

    public UInt16Bytes Temporary;
    
    public ExecutionState State;

    public enum ExecutionState {
        ExecutingInstruction,
        FetchNextInstructionOpcode,
        ExecutingAddressing,
        InterpretOpcode
    }
}
