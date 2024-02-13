namespace Sencae;

public sealed class CpuState {
    public byte[] Memory = new byte[2048];
    public byte[] IORegisters = new byte[32];

    // General purpose registers
    public byte A = 0;
    public byte X = 0;
    public byte Y = 0;

    public CpuStatusRegisterFlags P = default;

    // Program Counter
    public UInt16Bytes PC = default;

    // Stack pointer
    public byte SP = 0;

    public CpuInternalState Internal = default;

    public CpuState() {
    }
}