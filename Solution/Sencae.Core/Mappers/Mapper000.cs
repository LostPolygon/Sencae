using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sencae.Mappers;

public static class Mapper000 {
    public readonly struct CartridgeToCpuBusDevice(byte[] prgRom, byte[]? prgRam = null) : IMapperDevice {
        private readonly DirectArray<byte> _prgRomMemory = new(prgRom, 0x8000);
        private readonly DirectArray<byte>? _prgRamMemory = DirectArray.FromNullable(prgRam, 0x6000);

        [MethodImpl(HotPath)]
        public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            return address switch {
                >= 0x6000 and < 0x8000 => BusDeviceHandlers.ReadWrite(_prgRamMemory, address, accessMode, ref bus),
                >= 0x8000 and <= 0xFFFF => BusDeviceHandlers.ReadOnly(_prgRomMemory, address, accessMode, ref bus),
                _ => false
            };
        }

        public void Reset() {
            _prgRamMemory?.AsSpan().Clear();
        }

        public void Dispose() {
            _prgRomMemory.Dispose();
            _prgRamMemory?.Dispose();
        }
    }

    public sealed class CartridgeToPpuBusDevice(byte[]? chrRom, byte[]? chrRam)
        : CartridgeToPpuBusDeviceBase(chrRom, chrRam) {
        private readonly DirectArray<byte>? _chrRomMemory = DirectArray.FromNullable(chrRom);
        private readonly DirectArray<byte>? _chrRamMemory = DirectArray.FromNullable(chrRam);

        [MethodImpl(HotPath)]
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            return address switch {
                >= 0x0000 and <= 0x1FFF => HandleChrRomOrRamIndexed(address, accessMode, ref bus),
                _ => HandlePpuBus(address, accessMode, ref bus)
            };
        }
    }
}
