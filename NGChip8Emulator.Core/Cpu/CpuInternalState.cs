namespace NGChip8Emulator; 

public struct CpuInternalState {
    public UInt16Bytes AddressBus;
    
    public byte DataBus;
    public bool Read;
    
    public int OperationCycle;

    //public byte InstructionOpcode;
    public InstructionDescriptor Instruction;

    public ushort Temporary;
    
    public ExecutionState State;

    public enum ExecutionState {
        ExecutingInstruction,
        FetchNextInstructionOpcode,
        ExecutingAddressing,
        InterpretOpcode
    }
}
