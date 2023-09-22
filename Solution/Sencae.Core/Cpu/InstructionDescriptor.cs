namespace Sencae;

public record InstructionDescriptor(
    string Mnemonic,
    InstructionType Type,
    AddressingMode AddressingMode,
    InstructionFlags Flags,
    byte Opcode,
    byte OpcodeSize
);