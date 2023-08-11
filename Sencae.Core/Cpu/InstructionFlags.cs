using System;

namespace Sencae; 

[Flags]
public enum InstructionFlags : byte {
    None = 0,
    Unofficial = 1 << 0,
    SpecialAddressing = 1 << 1,
    FetchesNextOpcode = 1 << 2,
    ModifiesAddressingLogic = 1 << 3,
}
