using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sencae.Mappers;

public static class Mapper002 {
    public class State {
        public byte BankSelect;
    }

    public sealed class CartridgeToCpuBusDevice(State state, byte[] prgRom, byte[]? prgRam = null) : IMapperDevice {
        private readonly DirectArray<byte> _prgRomMemory = new(prgRom, 0);
        private readonly DirectArray<byte>? _prgRamMemory = DirectArray.FromNullable(prgRam, 0x6000);

        [MethodImpl(HotPath)]
        public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            switch (address) {
                case >= 0x6000 and < 0x8000:
                    return BusDeviceHandlers.ReadWrite(_prgRamMemory, address, accessMode, ref bus);
                case >= 0x8000 and <= 0xFFFF:
                    switch (accessMode) {
                        case MemoryAccessMode.Read or MemoryAccessMode.ReadObserve:
                            int bank;
                            switch (address) {
                                // 0x8000-0xBFFF: 16 KB switchable PRG ROM bank
                                case <= 0xBFFF:
                                    bank = state.BankSelect;
                                    address -= 0x8000;
                                    break;

                                // 0xC000-0xFFFF: 16 KB PRG ROM bank, fixed to the last bank
                                case >= 0xC000:
                                    bank = _prgRomMemory.Length / 0x4000 - 1;
                                    address -= 0xC000;
                                    break;
                            }

                            int realAddress = 0x4000 * bank + address;
                            return BusDeviceHandlers.ReadOnlyIndexed(_prgRomMemory, realAddress, accessMode, ref bus);
                        case MemoryAccessMode.Write:
                            state.BankSelect = bus;
                            return false;
                    }

                    break;
            }

            return false;
        }
        
        public void Reset() {
            _prgRamMemory?.AsSpan().Clear();
        }

        public void Dispose() {
            _prgRomMemory.Dispose();
            _prgRamMemory?.Dispose();
        }
    }
    
    // TODO: PPU mirroring solder pads
}
