namespace NGChip8Emulator;

public readonly record struct InstructionDescriptor(
    string Mnemonic,
    InstructionType Type,
    AddressMode AddressMode,
    InstructionFlags Flags,
    byte Opcode
);
