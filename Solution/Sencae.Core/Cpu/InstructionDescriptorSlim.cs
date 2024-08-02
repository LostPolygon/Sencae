namespace Sencae;

public readonly record struct InstructionDescriptorSlim(
    byte OpCode,
    InstructionType Type,
    AddressingMode AddressingMode,
    bool HasMemoryReadWriteLogic,
    bool HasAfterNextOpcodeFetchedLogic,
    bool FetchesNextOpcode,
    bool MemoryDataOperation,
    bool CanSkipToNextOpcode
);
