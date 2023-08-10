using System.Runtime.CompilerServices;

namespace Sencae;

public static class CpuStatusRegisterFlagsExtensions {
    [MethodImpl(MethodOptions.Options)]
    public static bool HasFlag(this CpuStatusRegisterFlags value, CpuStatusRegisterFlags flag) {
        return (value & flag) != 0;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static void SetFlag(this ref CpuStatusRegisterFlags value, CpuStatusRegisterFlags flag, bool flagSet) {
        // https://graphics.stanford.edu/~seander/bithacks.html#ConditionalSetOrClearBitsWithoutBranching
        
        value ^= (CpuStatusRegisterFlags) ((-(flagSet ? 1 : 0) ^ (int) value) & (int) flag);
        
        /*if (flagSet) {
            SetFlag(ref value, flag);
        } else {
            RemoveFlag(ref value, flag);
        }*/
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
