using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 1)]
public struct ByteBool {
    [FieldOffset(0)]
    public byte Value;

    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.I1)]
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