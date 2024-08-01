namespace Sencae;

public struct CpuRegistersState {
    // General purpose registers
    public byte A;
    public byte X;
    public byte Y;

    public CpuStatusRegisterFlags P;

    // Program Counter
    public UInt16Bytes PC;

    // Stack pointer
    public byte SP;

    public CpuRegistersState() {
        A = X = Y = SP = 0;
        P = default;
        PC = default;
    }
}
