using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sencae.Mappers;

public static class Mapper003 {
    public class State {
        public byte BankSelect;
    }

    public sealed class CartridgeToCpuBusDevice(State state, byte[] prgRom, byte[]? prgRam = null) : IMapperDevice {
        private readonly DirectArray<byte> _prgRomMemory = new(prgRom, 0x8000);
        private readonly DirectArray<byte>? _prgRamMemory = DirectArray.FromNullable(prgRam, 0x6000);

        [MethodImpl(HotPath)]
        public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            switch (address) {
                case >= 0x6000 and < 0x8000:
                    return BusDeviceHandlers.ReadWrite(_prgRamMemory, address, accessMode, ref bus);
                case >= 0x8000 and <= 0xFFFF:
                    switch (accessMode) {
                        case MemoryAccessMode.Read or MemoryAccessMode.ReadObserve:
                            return BusDeviceHandlers.ReadOnly(_prgRomMemory, address, accessMode, ref bus);
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

    public sealed class CartridgeToPpuBusDevice(State state, byte[]? chrRom, byte[]? chrRam) :
        CartridgeToPpuBusDeviceBase(chrRom, chrRam) {
        [MethodImpl(HotPath)]
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            return address switch {
                >= 0x0000 and <= 0x1FFF => HandleChrRomOrRamIndexed(address, accessMode, ref bus),
                _ => HandlePpuBus(address, accessMode, ref bus)
            };
        }
    }
}
