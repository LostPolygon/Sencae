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

    public readonly struct CartridgeToPpuBusDevice(byte[]? chrRom, byte[]? chrRam) : IMapperDevice {
        private readonly DirectArray<byte>? _chrRomMemory = DirectArray.FromNullable(chrRom);
        private readonly DirectArray<byte>? _chrRamMemory = DirectArray.FromNullable(chrRam);

        [MethodImpl(HotPath)]
        public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            switch (address) {
                case >= 0x0000 and <= 0x1FFF:
                    if (_chrRomMemory != null)
                        return BusDeviceHandlers.ReadOnlyIndexed(_chrRomMemory.Value, address, accessMode, ref bus);

                    if (_chrRamMemory != null)
                        return BusDeviceHandlers.ReadWriteIndexed(_chrRamMemory.Value, address, accessMode, ref bus);

                    Debug.Assert(_chrRomMemory != null || _chrRamMemory != null, "_chrRomMemory != null || _chrRamMemory != null");
                    return false;
                default:
                    return false;
            }
        }
        
        public void Reset() {
            _chrRamMemory?.AsSpan().Clear();
        }

        public void Dispose() {
            _chrRomMemory?.Dispose();
        }
    }
}
