namespace NGChip8Emulator;

public readonly record struct InstructionDefinition(
    string Mnemonic,
    InstructionType Type,
    AddressMode AddressMode,
    InstructionFlags Flags,
    byte Opcode
);
