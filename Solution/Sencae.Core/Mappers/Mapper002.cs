using System.Runtime.CompilerServices;

namespace Sencae.Mappers;

public static class Mapper002 {
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
                            int bank;
                            switch (address) {
                                // 0x8000-0xBFFF: 16 KB switchable PRG ROM bank
                                case <= 0xBFFF:
                                    bank = state.BankSelect;
                                    address = (ushort) (address & ~0x8000);
                                    break;

                                // 0xC000-0xFFFF: 16 KB PRG ROM bank, fixed to the last bank
                                case >= 0xC000:
                                    bank = PrgRomMemory.Length / 0x4000 - 1;
                                    address -= 0xC000;
                                    //address = (ushort) (address & ~0xC000);
                                    break;
                            }

                            int bankedAddress = bank * 0x4000 + address;
                            bankedAddress %= PrgRomMemory.Length;
                            return BusDeviceHandlers.ReadOnlyIndexed(PrgRomMemory, bankedAddress, accessMode, ref bus);
                        case MemoryAccessMode.Write:
                            //state.BankSelect = (byte) (bus & 0b0000_1111);
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
}
