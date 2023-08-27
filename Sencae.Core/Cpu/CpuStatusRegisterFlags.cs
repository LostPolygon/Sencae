using System.Runtime.CompilerServices;

namespace Sencae;

public record struct CpuStatusRegisterFlags {
    public bool Carry;
    public bool Zero;
    public bool InterruptDisable;
    public bool DecimalMode;
    public bool BreakFlag;
    public bool Overflow;
    public bool NegativeOrSign;

    [MethodImpl(MethodOptions.Options)]
    public CpuStatusRegisterPackedFlags ToFlags() {
        byte result = 0;
        result |= (byte) (Carry ? CpuStatusRegisterPackedFlags.Carry : 0);
        result |= (byte) (Zero ? CpuStatusRegisterPackedFlags.Zero : 0);
        result |= (byte) (InterruptDisable ? CpuStatusRegisterPackedFlags.InterruptDisable : 0);
        result |= (byte) (DecimalMode ? CpuStatusRegisterPackedFlags.DecimalMode : 0);
        result |= (byte) (BreakFlag ? CpuStatusRegisterPackedFlags.BreakFlag : 0);
        result |= (byte) (Overflow ? CpuStatusRegisterPackedFlags.Overflow : 0);
        result |= (byte) (NegativeOrSign ? CpuStatusRegisterPackedFlags.NegativeOrSign : 0);
        result |= (byte) CpuStatusRegisterPackedFlags.Unused;
        return (CpuStatusRegisterPackedFlags) result;
    }

    [MethodImpl(MethodOptions.Options)]
    public static CpuStatusRegisterFlags FromFlags(CpuStatusRegisterPackedFlags flags) {
        CpuStatusRegisterFlags result = new() {
            Carry = (flags & CpuStatusRegisterPackedFlags.Carry) != 0,
            Zero = (flags & CpuStatusRegisterPackedFlags.Zero) != 0,
            InterruptDisable = (flags & CpuStatusRegisterPackedFlags.InterruptDisable) != 0,
            DecimalMode = (flags & CpuStatusRegisterPackedFlags.DecimalMode) != 0,
            BreakFlag = (flags & CpuStatusRegisterPackedFlags.BreakFlag) != 0,
            Overflow = (flags & CpuStatusRegisterPackedFlags.Overflow) != 0,
            NegativeOrSign = (flags & CpuStatusRegisterPackedFlags.NegativeOrSign) != 0
        };

        return result;
    }
}