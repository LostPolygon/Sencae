using System;

namespace NGChip8Emulator; 

[Flags]
public enum InstructionFlags : byte {
    None = 0,
    Unofficial = 1 << 0,
    CustomAddressing = 1 << 1
}
