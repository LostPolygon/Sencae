using System.Runtime.InteropServices;

namespace NGChip8Emulator; 

[StructLayout(LayoutKind.Explicit)]
public struct UInt16Bytes {
    [FieldOffset(0)]
    public ushort Value;
    
    [FieldOffset(0)]
    public byte Low;
    
    [FieldOffset(1)]
    public byte High;

    public override string ToString() {
        return Value.ToString();
    }
}
