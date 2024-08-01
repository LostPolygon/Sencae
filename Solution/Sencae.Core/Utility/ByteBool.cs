using System;
using System.Runtime.InteropServices;

namespace Sencae;

[StructLayout(LayoutKind.Explicit)]
public struct ByteBool {
    [FieldOffset(0)]
    public byte Value;
    
    [FieldOffset(0)]
    public bool BoolValue;

    public ByteBool(byte value) => Value = value;

    public ByteBool(bool boolValue) => BoolValue = boolValue;

    public override string ToString() => $"{BoolValue} ({Value})";

    public static implicit operator ByteBool(bool value) => new(value);

    public static implicit operator ByteBool(byte value) => new(value);
    
    //public static explicit operator bool(ByteBool value) => value.BoolValue;
}
