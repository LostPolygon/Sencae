using System;

namespace Sencae;

[Flags]
public enum CpuStatusRegisterFlags : byte {
    Carry = 1 << 0,            // 0 - C, Carry (0=No Carry, 1=Carry)
    Zero = 1 << 1,             // 1 - Z, Zero (0=Nonzero, 1=Zero)
    InterruptDisable = 1 << 2, // 2 - I, IRQ Disable (0=IRQ Enable, 1=IRQ Disable)
    DecimalMode = 1 << 3,      // 3 - D, Decimal Mode (0=Normal, 1=BCD Mode for ADC/SBC opcodes)
    BreakFlag = 1 << 4,        // 4 - B, Break Flag (0=IRQ/NMI, 1=BRK/PHP opcode)
    Unused = 1 << 5,           // 5 - -, Not used (Always 1)
    Overflow = 1 << 6,         // 6 - V, Overflow (0=No Overflow, 1=Overflow)
    NegativeOrSign = 1 << 7,   // 7 - N, Negative/Sign (0=Positive, 1=Negative)
}
