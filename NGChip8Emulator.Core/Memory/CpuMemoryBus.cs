using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NGChip8Emulator; 

public unsafe class CpuMemoryBus : IMemory {
    public readonly Machine Machine;

    private readonly byte[] _dummyValue = new byte[2];

    public CpuMemoryBus(Machine machine) {
        Machine = machine;
    }
    
     

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ref byte GetU8(ushort address) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: return ref Machine.State.Cpu.Memory[address % 0x800];
            case >= 0x4020 and <= 0xFFFF: return ref Machine.Cartridge.PrgRomMemory.GetU8(address);
            default: return ref _dummyValue[0]; // open bus
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ref ushort GetU16(ushort address) {
        return ref Unsafe.As<byte, ushort>(ref GetU8(address));
    }
}
