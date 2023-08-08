using System;
using System.Runtime.InteropServices;

namespace NGChip8Emulator; 

public unsafe class CpuMemoryBus : IMemory {
    public Machine Machine;

    private byte[] _dummyValue = new byte[2];

    public CpuMemoryBus(Machine machine) {
        Machine = machine;
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
    
    public Span<byte> GetU8ItemPointer(ushort address) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: return MemoryMarshal.CreateSpan(ref Machine.State.Cpu.Memory[address % 0x800], 2);
            case >= 0x4020 and <= 0xFFFF: return MemoryMarshal.CreateSpan(ref Machine.Cartridge.PrgRomMemory.GetU8(address), 2);
            default: return MemoryMarshal.CreateSpan(ref _dummyValue[0], 2);
        }
    }
}
