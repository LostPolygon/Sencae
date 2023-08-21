using System.Runtime.CompilerServices;

namespace Sencae; 

public static class BoolUtility {
    [MethodImpl(MethodOptions.Options)]
    public static byte AsByte(this ref bool b) {
        return Unsafe.As<bool, byte>(ref b);
    }
}
