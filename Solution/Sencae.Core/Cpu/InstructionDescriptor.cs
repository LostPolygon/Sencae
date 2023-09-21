using System;

namespace Sencae;

public record InstructionDescriptor(
    string Mnemonic,
    InstructionType Type,
    AddressingMode AddressingMode,
    InstructionFlags Flags,
    byte Opcode,
    byte OpcodeSize
);

/*public readonly struct InstructionDescriptor : IEquatable<InstructionDescriptor> {
    public readonly string Mnemonic;
    public readonly InstructionType Type;
    public readonly AddressingMode AddressingMode;
    public readonly InstructionFlags Flags;
    public readonly byte Opcode;
    public readonly byte OpcodeSize;

    public InstructionDescriptor(
        string mnemonic,
        InstructionType type,
        AddressingMode addressingMode,
        InstructionFlags flags,
        byte opcode,
        byte opcodeSize
    ) {
        Mnemonic = mnemonic;
        Type = type;
        AddressingMode = addressingMode;
        Flags = flags;
        Opcode = opcode;
        OpcodeSize = opcodeSize;
    }

    public bool Equals(InstructionDescriptor other) =>
        Mnemonic == other.Mnemonic &&
        Type == other.Type &&
        AddressingMode == other.AddressingMode &&
        Flags == other.Flags &&
        Opcode == other.Opcode &&
        OpcodeSize == other.OpcodeSize;

    public override bool Equals(object? obj) =>
        obj is InstructionDescriptor other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Mnemonic, (int) Type, (int) AddressingMode, (int) Flags, Opcode, OpcodeSize);

    public override string ToString() =>
        $"{nameof(Mnemonic)}: {Mnemonic}, " +
        $"{nameof(Type)}: {Type}, " +
        $"{nameof(AddressingMode)}: {AddressingMode}, " +
        $"{nameof(Flags)}: {Flags}, " +
        $"{nameof(Opcode)}: {Opcode}, " +
        $"{nameof(OpcodeSize)}: {OpcodeSize}";

    public static bool operator ==(InstructionDescriptor left, InstructionDescriptor right) => left.Equals(right);

    public static bool operator !=(InstructionDescriptor left, InstructionDescriptor right) => !left.Equals(right);
}*/
