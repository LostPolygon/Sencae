namespace Sencae;

public readonly record struct InstructionDescriptor(
    string Mnemonic,
    InstructionType Type,
    AddressingMode AddressingMode,
    InstructionFlags Flags,
    byte Opcode,
    byte OpcodeSize
);