namespace Sencae;

public sealed class CpuState {
    public byte[] Memory = null!;
    public byte[] IORegisters = null!;

    public CpuRegistersState Registers;
    public CpuInternalState Internal;
    
    public DmaDeviceState OamDmaDevice = new();

    public CpuState() {
        Reset();
    }

    public void Reset() {
        Memory = new byte[2048];
        IORegisters = new byte[32];

        Registers = default;
        Internal = default;
    }
}