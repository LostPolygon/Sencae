using System.Runtime.CompilerServices;

namespace Sencae;

public record struct CpuStatusRegisterFlags {
    public ByteBool Carry;
    public ByteBool Zero;
    public ByteBool InterruptDisable;
    public ByteBool DecimalMode;
    public ByteBool BreakFlag;
    public ByteBool Overflow;
    public ByteBool NegativeOrSign;

    public CpuStatusRegisterPackedFlags AsFlags {
        [MethodImpl(AggressiveInlining)]
        get =>
            (Carry.BoolValue ? CpuStatusRegisterPackedFlags.Carry : 0) |
            (Zero.BoolValue ? CpuStatusRegisterPackedFlags.Zero : 0) |
            (InterruptDisable.BoolValue ? CpuStatusRegisterPackedFlags.InterruptDisable : 0) |
            (DecimalMode.BoolValue ? CpuStatusRegisterPackedFlags.DecimalMode : 0) |
            (BreakFlag.BoolValue ? CpuStatusRegisterPackedFlags.BreakFlag : 0) |
            (Overflow.BoolValue ? CpuStatusRegisterPackedFlags.Overflow : 0) |
            (NegativeOrSign.BoolValue ? CpuStatusRegisterPackedFlags.NegativeOrSign : 0) |
            CpuStatusRegisterPackedFlags.Unused;
    }

    [MethodImpl(HotPath)]
    public static CpuStatusRegisterFlags FromFlags(CpuStatusRegisterPackedFlags flags) {
        CpuStatusRegisterFlags result = new() {
            Carry = (byte) (flags & CpuStatusRegisterPackedFlags.Carry),
            Zero = (byte) (flags & CpuStatusRegisterPackedFlags.Zero),
            InterruptDisable = (byte) (flags & CpuStatusRegisterPackedFlags.InterruptDisable),
            DecimalMode = (byte) (flags & CpuStatusRegisterPackedFlags.DecimalMode),
            BreakFlag = (byte) (flags & CpuStatusRegisterPackedFlags.BreakFlag),
            Overflow = (byte) (flags & CpuStatusRegisterPackedFlags.Overflow),
            NegativeOrSign = (byte) (flags & CpuStatusRegisterPackedFlags.NegativeOrSign),
        };

        return result;
    }
}
