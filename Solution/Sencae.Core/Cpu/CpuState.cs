namespace Sencae;

public sealed class CpuState {
    public byte[] Memory = null!;
    public byte[] IORegisters = null!;

    // General purpose registers
    public byte A = 0;
    public byte X = 0;
    public byte Y = 0;

    public CpuStatusRegisterFlags P = default;

    // Program Counter
    public UInt16Bytes PC = default;

    // Stack pointer
    public byte SP = 0;

    public DmaDeviceState OamDmaDevice = new();

    public CpuInternalState Internal = default;

    public CpuState() {
        Reset();
    }

    public void Reset() {
        Memory = new byte[2048];
        IORegisters = new byte[32];

        A = X = Y = SP = 0;
        P = default;
        PC = default;

        Internal = default;
    }
}