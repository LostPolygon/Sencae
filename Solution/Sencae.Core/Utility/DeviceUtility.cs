using System;
using System.Diagnostics;

namespace Sencae; 

public static unsafe class DeviceUtility {
    public static ref byte OpenBus(ushort address) {
        Debug.Fail($"Attempt to access open bus at 0x{address:X4}");
        Span<byte> dummy = stackalloc byte[2];
        return ref dummy[0];
    }
}
