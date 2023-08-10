using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae; 

public readonly struct ArrayMemory : IMemory {
    public readonly byte[] Memory;
    public readonly int AddressOffset;

    public ArrayMemory(byte[] memory, int addressOffset) {
        AddressOffset = addressOffset;
        Memory = memory;
    }

    [MethodImpl(MethodOptions.Options)]
    public ref byte GetU8(ushort address) {
        ref byte data = ref MemoryMarshal.GetArrayDataReference(Memory);
        int index = CalculateAddress(address);
        return ref Unsafe.Add(ref data, index);
    }

    [MethodImpl(MethodOptions.Options)]
    private int CalculateAddress(ushort address) => address + AddressOffset;
}
