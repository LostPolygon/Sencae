namespace Sencae; 

public struct CpuInternalState {
    public UInt16Bytes AddressBus;

    public byte DataBus;
    public bool Read;
    
    public int OperationCycle;

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
