using System;
using System.Runtime.CompilerServices;

namespace Sencae.Mappers;

public static class Mapper000 {
    public readonly struct CartridgeToCpuBusDevice : IBusDevice, IDisposable {
        private readonly DirectArray<byte> _prgRomMemory;
        private readonly DirectArray<byte>? _prgRamMemory;

        public CartridgeToCpuBusDevice(byte[] prgRom, byte[]? prgRam = null) {
            _prgRomMemory = new DirectArray<byte>(prgRom, 0x8000);

            if (prgRam != null) {
                _prgRamMemory = new DirectArray<byte>(prgRam, 0x6000);
            }
        }

        [MethodImpl(HotPath)]
        public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            switch (address) {
                case >= 0x6000 and < 0x8000: {
                    return BusDeviceHandlers.ReadWrite(_prgRamMemory, address, accessMode, ref bus);
                }
                case >= 0x8000 and <= 0xFFFF: {
                    return BusDeviceHandlers.ReadWrite(_prgRomMemory, address, accessMode, ref bus);
                }
            }

            return false;
        }

        public void Dispose() {
            _prgRomMemory.Dispose();
            _prgRamMemory?.Dispose();
        }
    }

    public readonly struct CartridgeToPpuBusDevice : IBusDevice, IDisposable {
        public readonly byte[] ChrRom;

        private readonly DirectArray<byte> _chrRomMemory;

        public CartridgeToPpuBusDevice(byte[] chrRom) {
            ChrRom = chrRom;
            _chrRomMemory = new DirectArray<byte>(ChrRom, -ChrRom.Length);
        }

        [MethodImpl(HotPath)]
        public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            return address switch {
                >= 0x0000 and <= 0x1FFF => BusDeviceHandlers.ReadOnly(_chrRomMemory, address, accessMode, ref bus),
                _ => false
            };
        }

        public void Dispose() {
            _chrRomMemory.Dispose();
        }
    }
}
