using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae.Mappers;

public static class Mapper003 {
    public class State : IMapperState {
        public byte BankSelect;

        public void Reset() {
            BankSelect = 0;
        }
    }

    public sealed class CartridgeToCpuBusDevice(State state, byte[] prgRom, byte[]? prgRam = null)
        : CartridgeToCpuBusDeviceBase(prgRom, prgRam) {
        [MethodImpl(HotPath)]
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            switch (address) {
                case >= 0x6000 and < 0x8000:
                    return BusDeviceHandlers.ReadWrite(PrgRamMemory, address, accessMode, ref bus);
                case >= 0x8000 and <= 0xFFFF:
                    switch (accessMode) {
                        case MemoryAccessMode.Read or MemoryAccessMode.ReadObserve:
                            return BusDeviceHandlers.ReadOnly(PrgRomMemory, address, accessMode, ref bus);
                        case MemoryAccessMode.Write:
                            state.BankSelect = bus;
                            return false;
                    }

                    break;
            }

            return false;
        }

        public override void Reset() {
            base.Reset();
            state.Reset();
        }
    }

    public sealed class CartridgeToPpuBusDevice(State state, byte[]? chrRom, byte[]? chrRam, PpuNametableMirroringType mirroringType)
        : CartridgeToPpuBusDeviceBase(chrRom, chrRam) {
        [MethodImpl(HotPathBlock)]
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            int bankedAddress;
            switch (address) {
                case >= 0x0000 and <= 0x1FFF:
                    int bank = state.BankSelect & (0x04 - 1);
                    bankedAddress = 0x2000 * bank + address;
                    break;
                default:
                    bankedAddress = address;
                    break;
            }

            return DefaultPpuHandler(bankedAddress, address, accessMode, ref bus, mirroringType);
        }
    }
}
