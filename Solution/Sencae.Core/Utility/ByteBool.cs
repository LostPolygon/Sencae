using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae;

[StructLayout(LayoutKind.Explicit)]
public struct ByteBool {
    [FieldOffset(0)]
    public byte Value;

    [FieldOffset(0)]
    public bool BoolValue;

    public override string ToString() => $"{BoolValue} ({Value})";
    
    [MethodImpl(AggressiveInlining)]
    public static ByteBool Create(bool value) => new() { BoolValue = value };
    
    [MethodImpl(AggressiveInlining)]
    public static ByteBool Create(byte value) => new() { Value = value };

    [MethodImpl(AggressiveInlining)]
    public static implicit operator ByteBool(bool value) => new() { BoolValue = value };

    [MethodImpl(AggressiveInlining)]
    public static implicit operator ByteBool(byte value) => new() { Value = value };
}

/*public struct ByteBool {
    public byte Value;

    public bool BoolValue => Value != 0 ? true : false;

    public override string ToString() => $"{BoolValue} ({Value})";
    
    [MethodImpl(AggressiveInlining)]
    public static ByteBool Create(bool value) => new() { Value = (byte) (value ? 1 : 0) };
    
    [MethodImpl(AggressiveInlining)]
    public static ByteBool Create(byte value) => new() { Value = value };

    [MethodImpl(AggressiveInlining)]
    public static implicit operator ByteBool(bool value) => new() { Value = (byte) (value ? 1 : 0) };

    [MethodImpl(AggressiveInlining)]
    public static implicit operator ByteBool(byte value) => new() { Value = value };
}*/