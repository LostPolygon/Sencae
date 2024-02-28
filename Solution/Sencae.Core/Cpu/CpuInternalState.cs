namespace Sencae;

public struct CpuInternalState {
    public UInt16Bytes AddressBus;

    public byte DataBus;
    public bool Read;

    public int OperationCycle;

    public InstructionDescriptor Instruction;

    public UInt16Bytes Temporary;

    public ExecutionState State;
    public sbyte PageBoundaryCrossedCarry;
    public bool Halted;

    public bool IrqPending;
    public bool NmiPulledDown;
    public bool NmiFlipFlop;

    public ulong Cycles;

    public enum ExecutionState {
        ExecutingInstruction,
        FinishingCurrentInstructionAndFetchingNextOpcode,
        ExecutingAddressing,
        InterpretingFetchedOpcode
    }
}
