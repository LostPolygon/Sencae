using System.Runtime.CompilerServices;

namespace Sencae;

public record struct CpuStatusRegisterUnpackedFlags {
    public bool Carry;
    public bool Zero;
    public bool InterruptDisable;
    public bool DecimalMode;
    public bool BreakFlag;
    public bool Overflow;
    public bool NegativeOrSign;

    //[MethodImpl(MethodOptions.Options)]
    public CpuStatusRegisterFlags ToFlags() {
        byte result = 0;
        result |= (byte) (Carry ? CpuStatusRegisterFlags.Carry : 0);
        result |= (byte) (Zero ? CpuStatusRegisterFlags.Zero : 0);
        result |= (byte) (InterruptDisable ? CpuStatusRegisterFlags.InterruptDisable : 0);
        result |= (byte) (DecimalMode ? CpuStatusRegisterFlags.DecimalMode : 0);
        result |= (byte) (BreakFlag ? CpuStatusRegisterFlags.BreakFlag : 0);
        result |= (byte) (Overflow ? CpuStatusRegisterFlags.Overflow : 0);
        result |= (byte) (NegativeOrSign ? CpuStatusRegisterFlags.NegativeOrSign : 0);
        result |= (byte) CpuStatusRegisterFlags.Unused;
        return (CpuStatusRegisterFlags) result;
    }

    //[MethodImpl(MethodOptions.Options)]
    public static CpuStatusRegisterUnpackedFlags FromFlags(CpuStatusRegisterFlags flags) {
        CpuStatusRegisterUnpackedFlags result = new() {
            Carry = (flags & CpuStatusRegisterFlags.Carry) != 0,
            Zero = (flags & CpuStatusRegisterFlags.Zero) != 0,
            InterruptDisable = (flags & CpuStatusRegisterFlags.InterruptDisable) != 0,
            DecimalMode = (flags & CpuStatusRegisterFlags.DecimalMode) != 0,
            BreakFlag = (flags & CpuStatusRegisterFlags.BreakFlag) != 0,
            Overflow = (flags & CpuStatusRegisterFlags.Overflow) != 0,
            NegativeOrSign = (flags & CpuStatusRegisterFlags.NegativeOrSign) != 0
        };

        return result;
    }
}
