using System;

namespace Sencae; 

[Flags]
public enum InstructionFlags : byte {
    None = 0,
    Unofficial = 1 << 0,
    FetchesNextOpcode = 1 << 2,
    HasMemoryReadWriteLogic = 1 << 3,
    MemoryDataOperation = 1 << 4,
    StoreOperation = 1 << 5,
    ReadModifyWriteOperation = 1 << 6,
    BranchOperation = 1 << 7,
}
