using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae; 

public readonly struct ArrayMemoryDevice : MemoryDevice {
    public readonly byte[] Memory;
    public readonly int AddressOffset;

    public ArrayMemoryDevice(byte[] memory, int addressOffset) {
        AddressOffset = addressOffset;
        Memory = memory;
    }

    [MethodImpl(MethodOptions.Options)]
    public ref byte GetU8(ushort address) {
        int index = CalculateAddress(address) % Memory.Length;
        Debug.Assert(index is >= 0 and <= UInt16.MaxValue, "index is >= 0 and <= UInt16.MaxValue", index.ToString());
        ref byte data = ref MemoryMarshal.GetArrayDataReference(Memory);
        data = ref Unsafe.Add(ref data, index);
        return ref data;
    }

    [MethodImpl(MethodOptions.Options)]
    private int CalculateAddress(ushort address) => address - AddressOffset;
}
