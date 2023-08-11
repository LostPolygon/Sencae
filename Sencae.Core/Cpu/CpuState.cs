namespace Sencae; 

public unsafe struct CpuState {
    public fixed byte Memory[2048];
    public fixed byte IORegisters[0x18 + 0x08];
        
    // General purpose registers
    public byte A;
    public byte X;
    public byte Y;

    public CpuStatusRegisterFlags P;

    // Program Counter
    public UInt16Bytes PC;

    // Stack pointer
    public byte SP;

    public CpuInternalState Internal;
}