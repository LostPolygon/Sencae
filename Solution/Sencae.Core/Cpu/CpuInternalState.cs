namespace Sencae;

public struct CpuInternalState {
    public const int OamDmaRegister = 0x4014;
    public const int OamDmaRegisterIndex = 0x14;
    
    public UInt16Bytes AddressBus;

    public byte DataBus;
    public bool Read;

    public int OperationCycle;

    public InstructionDescriptorSlim Instruction;

    public UInt16Bytes Temporary;

    public ExecutionState State;
    public sbyte PageBoundaryCrossedCarry;
    public bool Halted;

    public bool IrqPending;
    public bool NmiPulledDown;
    public bool NmiFlipFlop;
    
    //public bool IsRdyInputAsserted;

    public ulong Cycles;

    public enum ExecutionState {
        FinishingCurrentInstructionAndFetchingNextOpcode,
        InterpretingFetchedOpcode,
        ExecutingAddressing,
        ExecutingInstruction
    }
}