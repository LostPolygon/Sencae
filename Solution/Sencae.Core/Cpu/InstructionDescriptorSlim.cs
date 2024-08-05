namespace Sencae;

using static AddressingMode;

public readonly record struct InstructionDescriptorSlim(
    byte OpCode,
    InstructionType Type,
    AddressingMode AddressingMode,
    bool HasMemoryReadWriteLogic,
    bool HasAfterNextOpcodeFetchedLogic,
    bool FetchesNextOpcode,
    bool MemoryDataOperation,
    bool CanSkipToNextOpcode
) {
    public InstructionDescriptorSlim(InstructionDescriptor descriptor) : this(
        descriptor.Opcode,
        descriptor.Type,
        descriptor.AddressingMode,
        (descriptor.Flags & InstructionFlags.HasMemoryReadWriteLogic) != 0,
        (descriptor.Flags & InstructionFlags.NoAfterNextOpcodeFetchedLogic) == 0,
        (descriptor.Flags & InstructionFlags.FetchesNextOpcode) != 0,
        (descriptor.Flags & InstructionFlags.MemoryDataOperation) != 0,
        descriptor.AddressingMode is Implied or Immediate && (descriptor.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0
    ) {
    }
}

