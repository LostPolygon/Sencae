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
        get {
            byte result = 0;
            result |= Carry.Value;
            result |= Zero.Value;
            result |= InterruptDisable.Value;
            result |= DecimalMode.Value;
            result |= BreakFlag.Value;
            result |= Overflow.Value;
            result |= NegativeOrSign.Value;
            result |= (byte) CpuStatusRegisterPackedFlags.Unused;
            return (CpuStatusRegisterPackedFlags) result;
        }
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
