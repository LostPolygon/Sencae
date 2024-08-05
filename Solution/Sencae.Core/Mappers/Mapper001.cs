using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace Sencae.Mappers;

public static class Mapper001 {
    public class State {
        public byte ShiftRegisterValue;
        public byte Counter;
        public byte Control = 0x0C;
        public byte ChrBank0;
        public byte ChrBank1;
        public byte PrgBank;

        public(byte mirroring, byte prgRomBankMode, byte chrRomBankMode) DecodeControl() {
            return (
                (byte) (Control & 0b0000_0011),
                (byte) ((Control & 0b0000_1100) >> 2),
                (byte) ((Control & 0b0001_0000) >> 4)
            );
        }
    }

    public sealed class CartridgeToCpuBusDevice(State state, byte[] prgRom, byte[]? prgRam = null) : IMapperDevice {
        private readonly DirectArray<byte> _prgRomMemory = new(prgRom, 0);
        private readonly DirectArray<byte>? _prgRamMemory = DirectArray.FromNullable(prgRam, 0x6000);

        [MethodImpl(HotPath)]
        public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            switch (address) {
                case >= 0x6000 and <= 0x7FFF:
                    return BusDeviceHandlers.ReadWrite(_prgRamMemory, address, accessMode, ref bus);
                case >= 0x8000 and <= 0xFFFF:
                    switch (accessMode) {
                        case MemoryAccessMode.Read or MemoryAccessMode.ReadObserve:
                            (byte mirroring, byte prgRomBankMode, byte chrRomBankMode) = state.DecodeControl();

                            // CPU $8000-$BFFF: 16 KB PRG ROM bank, either switchable or fixed to the first bank
                            // CPU $C000-$FFFF: 16 KB PRG ROM bank, either fixed to the last bank or switchable
                            switch (prgRomBankMode) {
                                case 0:
                                case 1: {
                                    int realAddress = (state.PrgBank & 0b0001_1110) * 0x8000 + address;
                                    realAddress -= 0x8000;
                                    realAddress %= _prgRomMemory.Length;
                                    return BusDeviceHandlers.ReadOnlyIndexed(_prgRomMemory, realAddress, accessMode, ref bus);
                                }
                                case 2: {
                                    int bank;
                                    switch (address) {
                                        // $8000-$BFFF: 16 KB PRG ROM bank, fixed to the first bank
                                        case <= 0xBFFF:
                                            bank = 0;
                                            address = (ushort) (address % 0x4000);

                                            break;

                                        // $C000-$FFFF: 16 KB PRG ROM bank, switchable
                                        case >= 0xC000:
                                            bank = state.PrgBank;
                                            address = (ushort) (address % 0x4000);
                                            break;
                                    }

                                    int realAddress = 0x4000 * bank + address;
                                    realAddress %= _prgRomMemory.Length;
                                    return BusDeviceHandlers.ReadOnlyIndexed(_prgRomMemory, realAddress, accessMode, ref bus);
                                }
                                case 3: {
                                    if (address == 0xA000) {
                                        Console.WriteLine(address);
                                    }

                                    int bank;
                                    switch (address) {
                                        // $8000-$BFFF: 16 KB PRG ROM bank, switchable
                                        case <= 0xBFFF:
                                            bank = state.PrgBank;
                                            address = (ushort) (address % 0x4000);

                                            break;

                                        // $C000-$FFFF: 16 KB PRG ROM bank, fixed to the first bank
                                        case >= 0xC000:
                                            bank = _prgRomMemory.Length / 0x4000 - 1;
                                            address = (ushort) (address % 0x4000);
                                            break;
                                    }

                                    int realAddress = 0x4000 * bank + address;
                                    realAddress %= _prgRomMemory.Length;
                                    return BusDeviceHandlers.ReadOnlyIndexed(_prgRomMemory, realAddress, accessMode, ref bus);
                                }
                                default:
                                    throw new ArgumentOutOfRangeException("prgRomBankMode", prgRomBankMode, "");
                            }
                        case MemoryAccessMode.Write:
                            if ((bus & 0b1000_0000) != 0) {
                                state.ShiftRegisterValue = 0;
                                state.Counter = 0;
                                state.Control |= 0x0C;
                            } else {
                                byte shiftedValue = (byte) ((bus & 0b0000_0001) << 4);
                                state.ShiftRegisterValue = (byte) (((state.ShiftRegisterValue) >> 1) | shiftedValue);
                                state.Counter++;
                                if (state.Counter == 5) {
                                    state.Counter = 0;

                                    //int addressSelector = (address >> 13) & 0b11;
                                    switch (address) {
                                        case >= 0x8000 and <= 0x9FFF:
                                            state.Control = state.ShiftRegisterValue;
                                            break;
                                        case >= 0xA000 and <= 0xBFFF:
                                            state.ChrBank0 = state.ShiftRegisterValue;
                                            break;
                                        case >= 0xC000 and <= 0xDFFF:
                                            state.ChrBank1 = state.ShiftRegisterValue;
                                            break;
                                        case >= 0xE000 and <= 0xFFFF:
                                            state.PrgBank = state.ShiftRegisterValue;
                                            break;
                                    }
                                }
                            }

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
        CartridgeToPpuBusDeviceBase (chrRom, chrRam)
    {
        [MethodImpl(HotPath)]
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            (byte mirroring, byte prgRomBankMode, byte chrRomBankMode) = state.DecodeControl();
            switch (address) {
                // 4 KB switchable CHR bank 1
                case >= 0x0000 and <= 0x0FFF: {
                    int baseAddress;
                    if (chrRomBankMode == 0) {
                        baseAddress = (state.ChrBank0 & 0b0001_1110) * 0x2000;
                    } else {
                        baseAddress = state.ChrBank0 * 0x1000;
                    }

                    int realAddress = baseAddress + address;
                    return HandleChrRomOrRamIndexed(realAddress, accessMode, ref bus);
                }

                // 4 KB switchable CHR bank2
                case >= 0x1000 and <= 0x1FFF: {
                    int baseAddress;
                    if (chrRomBankMode == 0) {
                        baseAddress = (state.ChrBank0 & 0b0001_1110) * 0x2000;
                    } else {
                        baseAddress = state.ChrBank1 * 0x1000;
                    }

                    int realAddress = baseAddress + address;
                    return HandleChrRomOrRamIndexed(realAddress, accessMode, ref bus);
                }

                // Nametable 0
                case >= 0x2000 and <= 0x3EFF: {
                    const int bit10Mask = 0b0000_0100_0000_0000;
                    const int bit11Mask = 0b0000_1000_0000_0000;
                    //Mirroring (0: one-screen, lower bank; 1: one-screen, upper bank; 2: vertical; 3: horizontal)
                    switch (mirroring) {
                        case 0:
                        case 1:
                            // Set bit 10 to 0
                            //address = (ushort) (address & ~bit10Mask); 
                            break;
                        case 2:
                            break;
                        case 3:
                            // Set bit 11 to bit 10, reset bit 10
                            //address = (ushort) ((address & ~bit10Mask) | ((address & bit10Mask) << 1));
                            
                            // Set bit 10 to bit 11, reset bit 11
                            ushort newAddress = (ushort) ((address & ~(bit11Mask | bit10Mask)) | ((address & bit11Mask) >> 1));
                            if (address >= 0x2800) {
                                //Console.WriteLine($"src: 0x{address:X4}, dst: 0x{newAddress:X4}");
                            }
                            address = newAddress;
                            //address = (ushort) ((address) | ((address & bit11Mask) >> 1));
                            break;
                    }

                    //return _machine.PpuBusDevice.HandleBus(address, accessMode, ref bus);
                    //return BusDeviceHandlers.ReadWriteIndexed(_ppuMemory, address % 0x800, accessMode, ref bus);
                    return HandlePpuBus(address, accessMode, ref bus);
                }
                default:
                    return HandlePpuBus(address, accessMode, ref bus);
            }
        }
    }
}
