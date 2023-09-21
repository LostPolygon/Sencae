using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sencae;

public static class BusDeviceHandlers {
    public static void OpenBus(int address) {
        Debug.Fail($"Attempt to access open bus at 0x{address:X4}");
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool ReadWrite(DirectArray<byte> data, int address, MemoryAccessMode accessMode, ref byte bus) {
        if (data.Length <= address)
            return false;
        
        ref byte mappedData = ref data.Data[address];
        ReadWriteRaw(ref mappedData, accessMode, ref bus);

        return true;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool ReadOnly(DirectArray<byte> data, int address, MemoryAccessMode accessMode, ref byte bus) {
        if (accessMode == MemoryAccessMode.Write || data.Length <= address)
            return false;
        
        ref byte mappedData = ref data.Data[address];
        ReadOnlyRaw(ref mappedData, accessMode, ref bus);

        return true;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool WriteOnly(DirectArray<byte> data, int address, MemoryAccessMode accessMode, ref byte bus) {
        if (accessMode != MemoryAccessMode.Write || data.Length <= address)
            return false;
        
        ref byte mappedData = ref data.Data[address];
        WriteOnlyRaw(ref mappedData, accessMode, ref bus);

        return true;
    }

    [MethodImpl(MethodOptions.Options)]
    public static void ReadWriteRaw(ref byte mappedData, MemoryAccessMode accessMode, ref byte bus) {
        if (accessMode == MemoryAccessMode.Write) {
            mappedData = bus;
        } else {
            bus = mappedData;
        }
    }

    [MethodImpl(MethodOptions.Options)]
    public static void ReadOnlyRaw(ref byte mappedData, MemoryAccessMode accessMode, ref byte bus) {
        Debug.Assert(accessMode != MemoryAccessMode.Write, "accessMode != MemoryAccessMode.Write");
        bus = mappedData;
    }

    [MethodImpl(MethodOptions.Options)]
    public static void WriteOnlyRaw(ref byte mappedData, MemoryAccessMode accessMode, ref byte bus) {
        Debug.Assert(accessMode == MemoryAccessMode.Write, "accessMode == MemoryAccessMode.Write");
        mappedData = bus;
    }
}
