using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sencae;

public static class BusDeviceHandlers {
    public static void AssertNotOpenBus(int address) {
        Debug.Fail($"Attempt to access open bus at 0x{address:X4}");
    }

    [MethodImpl(HotPath)]
    public static bool ReadWriteIndexed(DirectArray<byte> array, int arrayIndex, MemoryAccessMode accessMode, ref byte bus) {
        Debug.Assert(array.Length > arrayIndex, $"{ReadWriteIndexed}: array.Length > arrayIndex");

        ref byte mappedData = ref array[arrayIndex];
        ReadWriteRaw(ref mappedData, accessMode, ref bus);

        return true;
    }

    [MethodImpl(HotPath)]
    public static bool ReadOnlyIndexed(DirectArray<byte> array, int arrayIndex, MemoryAccessMode accessMode, ref byte bus) {
        Debug.Assert(array.Length > arrayIndex, $"{ReadOnlyIndexed}: array.Length > arrayIndex");
        if (accessMode == MemoryAccessMode.Write)
            return false;

        ref byte mappedData = ref array[arrayIndex];
        ReadOnlyRaw(ref mappedData, accessMode, ref bus);

        return true;
    }

    [MethodImpl(HotPath)]
    public static bool WriteOnlyIndexed(DirectArray<byte> array, int arrayIndex, MemoryAccessMode accessMode, ref byte bus) {
        Debug.Assert(array.Length > arrayIndex, $"{WriteOnlyIndexed}: array.Length > arrayIndex");
        if (accessMode != MemoryAccessMode.Write)
            return false;

        ref byte mappedData = ref array[arrayIndex];
        WriteOnlyRaw(ref mappedData, accessMode, ref bus);

        return true;
    }

    [MethodImpl(HotPath)]
    public static bool ReadWrite(DirectArray<byte> array, int address, MemoryAccessMode accessMode, ref byte bus) {
        int arrayIndex = array.CalculateIndex(address);
        return ReadWriteIndexed(array, arrayIndex, accessMode, ref bus);
    }

    [MethodImpl(HotPath)]
    public static bool ReadWrite(DirectArray<byte>? array, int address, MemoryAccessMode accessMode, ref byte bus) {
        if (array == null)
            return false;

        return ReadWrite(array.Value, address, accessMode, ref bus);
    }

    [MethodImpl(HotPath)]
    public static bool ReadOnly(DirectArray<byte> array, int address, MemoryAccessMode accessMode, ref byte bus) {
        int arrayIndex = array.CalculateIndex(address);
        return ReadOnlyIndexed(array, arrayIndex, accessMode, ref bus);
    }

    [MethodImpl(HotPath)]
    public static bool WriteOnly(DirectArray<byte> array, int address, MemoryAccessMode accessMode, ref byte bus) {
        int arrayIndex = array.CalculateIndex(address);
        return WriteOnlyIndexed(array, arrayIndex, accessMode, ref bus);
    }

    [MethodImpl(HotPath)]
    public static void ReadWriteRaw(ref byte mappedData, MemoryAccessMode accessMode, ref byte bus) {
        if (accessMode == MemoryAccessMode.Write) {
            mappedData = bus;
        } else {
            bus = mappedData;
        }
    }

    [MethodImpl(HotPath)]
    public static void ReadOnlyRaw(ref byte mappedData, MemoryAccessMode accessMode, ref byte bus) {
        Debug.Assert(accessMode != MemoryAccessMode.Write, "accessMode != MemoryAccessMode.Write");
        bus = mappedData;
    }

    [MethodImpl(HotPath)]
    public static void WriteOnlyRaw(ref byte mappedData, MemoryAccessMode accessMode, ref byte bus) {
        Debug.Assert(accessMode == MemoryAccessMode.Write, "accessMode == MemoryAccessMode.Write");
        mappedData = bus;
    }
}
