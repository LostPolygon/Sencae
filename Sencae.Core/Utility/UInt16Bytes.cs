using System.Runtime.InteropServices;

namespace Sencae; 

[StructLayout(LayoutKind.Explicit)]
public struct UInt16Bytes {
    [FieldOffset(0)]
    public ushort Value;
    
    [FieldOffset(0)]
    public byte Low;
    
    [FieldOffset(1)]
    public byte High;

    public UInt16Bytes(ushort value) => Value = value;

    public override string ToString() => $"{Value} (0x{Value:X00})";

    public static implicit operator ushort(UInt16Bytes value) => value.Value;

    public static implicit operator UInt16Bytes(ushort value) => new(value);
}
