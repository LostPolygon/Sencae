namespace Sencae;

public sealed record DmaDeviceState {
    public bool IsActive;
    public bool IsReadCycle;
    public byte Data;
    public int BytesCopied;
    public UInt16Bytes DmaStartAddress;
}
