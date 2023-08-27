using System.Runtime.CompilerServices;

namespace Sencae; 

public static class BusDeviceHandlers {
    [MethodImpl(MethodOptions.Options)]
    public static void ReadWrite(ref byte mappedData, bool isWrite, ref byte bus) {
        if (isWrite) {
            mappedData = bus;
        } else {
            bus = mappedData;
        }
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static void ReadOnly(ref byte mappedData, bool isWrite, ref byte bus) {
        if (!isWrite) {
            bus = mappedData;
        }
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static void WriteOnly(ref byte mappedData, bool isWrite, ref byte bus) {
        if (isWrite) {
            mappedData = bus;
        }
    }
}
