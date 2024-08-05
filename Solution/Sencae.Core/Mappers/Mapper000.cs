using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae.Mappers;

public static class Mapper000 {
    public sealed class CartridgeToCpuBusDevice(byte[] prgRom, byte[]? prgRam = null)
        : CartridgeToCpuBusDeviceBase(prgRom, prgRam) {
        [MethodImpl(HotPathBlock)]
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            return address switch {
                >= 0x6000 and < 0x8000 => BusDeviceHandlers.ReadWrite(PrgRamMemory, address, accessMode, ref bus),
                >= 0x8000 and <= 0xFFFF => BusDeviceHandlers.ReadOnly(PrgRomMemory, address, accessMode, ref bus),
                _ => false
            };
        }
    }

    public sealed class CartridgeToPpuBusDevice(byte[]? chrRom, byte[]? chrRam, PpuNametableMirroringType mirroringType)
        : CartridgeToPpuBusDeviceBase(chrRom, chrRam) {
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) =>
            DefaultPpuHandler(address, address, accessMode, ref bus, mirroringType);
    }
}
