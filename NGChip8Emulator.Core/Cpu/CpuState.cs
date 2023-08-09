namespace NGChip8Emulator; 

public unsafe struct CpuState {
    public fixed byte Memory[2048];
        
    // General purpose registers
    public byte A;
    public byte X;
    public byte Y;

    public CpuStatusRegisterFlags P;

    // Program Counter
    public UInt16Bytes PC;

    // Stack pointer
    public byte SP;

    public bool Halted;

    public CpuInternalState Internal;
}