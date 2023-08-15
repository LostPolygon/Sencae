using System.Runtime.CompilerServices;

namespace Sencae;

public static class CpuStatusRegisterFlagsExtensions {
    [MethodImpl(MethodOptions.Options)]
    public static bool HasFlagFast(this CpuStatusRegisterFlags value, CpuStatusRegisterFlags flag) {
        return (value & flag) != 0;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool HasNoFlag(this CpuStatusRegisterFlags value, CpuStatusRegisterFlags flag) {
        return (value & flag) == 0;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static void SetFlag(this ref CpuStatusRegisterFlags value, CpuStatusRegisterFlags flag, bool flagSet) {
        if (flagSet) {
            SetFlag(ref value, flag);
        } else {
            RemoveFlag(ref value, flag);
        }
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static void SetFlag(this ref CpuStatusRegisterFlags value, CpuStatusRegisterFlags flag) {
        value |= flag;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static void RemoveFlag(this ref CpuStatusRegisterFlags value, CpuStatusRegisterFlags flag) {
        value &= ~flag;
    }
}
