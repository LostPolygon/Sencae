namespace Sencae;

public class CpuState {
    public byte[] Memory = new byte[2048];
    public byte[] IORegisters = new byte[32];

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