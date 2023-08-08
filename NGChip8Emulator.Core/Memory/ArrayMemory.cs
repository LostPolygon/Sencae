using System;
using System.Runtime.InteropServices;

namespace NGChip8Emulator; 

public readonly struct ArrayMemory : IMemory {
    public readonly byte[] Memory;
    public readonly int AddressOffset;

    public ArrayMemory(byte[] memory, int addressOffset) {
        AddressOffset = addressOffset;
        Memory = memory;
    }

    public byte ReadU8(ushort address) {
        return GetU8(address);
    }

    public void WriteU8(ushort address, byte value) {
        ref byte destination = ref GetU8(address);
        destination = value;
    }

    public ref byte GetU8(ushort address) {
        return ref GetU8ItemPointer(address)[0];
    }

    public ref ushort GetU16(ushort address) {
        return ref MemoryMarshal.AsRef<ushort>(GetU8ItemPointer(address));
    }
        
    private Span<byte> GetU8ItemPointer(ushort address) {
        return MemoryMarshal.CreateSpan(ref Memory[address + AddressOffset - 0xFFFF], 2);
    }
}
