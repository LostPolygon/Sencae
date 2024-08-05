using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae.Mappers;

public static class Mapper001 {
    public class State : IMapperState {
        public byte ShiftRegisterValue;
        public byte Counter;

        public byte Control;
        public byte ChrBank0;
        public byte ChrBank1;
        public byte PrgBank;

        public State() {
            Reset();
        }

        public void Reset() {
            ResetShiftRegister();
            Control |= 0x0C;
        }

        public void ResetShiftRegister() {
            ShiftRegisterValue = 0;
            Counter = 0;
        }

        [MethodImpl(HotPath)]
        public MirroringMode GetMirroringMode() => (MirroringMode) (Control & 0b0000_0011);

        [MethodImpl(HotPath)]
        public PrgRomBankMode GetPrgRomBankMode() => (PrgRomBankMode) (Control & 0b0000_1100);

        [MethodImpl(HotPath)]
        public ChrRomBankMode GetChrRomBankMode() => (ChrRomBankMode) (Control & 0b0001_0000);

        public enum MirroringMode : byte {
            OneScreenBank0 = 0, // one-screen, lower bank
            OneScreenBank1 = 1, // one-screen, upper bank
            TwoScreenVerticalMirroring = 2,
            TwoScreenHorizontalMirroring = 3,
        }

        public enum PrgRomBankMode : byte {
            Switch32KbAt0x8000 = 0,
            Switch32KbAt0x8000Alt = 1 << 2,
            FixFirst16Kb = 2 << 2, // fix first bank at $8000 and switch 16 KB bank at $C000
            FixLast16Kb = 3 << 2, // fix last bank at $C000 and switch 16 KB bank at $8000)
        }

        public enum ChrRomBankMode : byte {
            Switch8KbBank = 0,
            SwitchTwo4KbBanks = 1 << 4,
        }
    }

    public sealed class CartridgeToCpuBusDevice(State state, byte[] prgRom, byte[]? prgRam = null)
        : CartridgeToCpuBusDeviceBase(prgRom, prgRam) {
        [MethodImpl(HotPath)]
        public int Calculate16KbBankedAddress(ushort address, int bank0, int bank1) {
            int bank = address switch {
                <= 0xBFFF => bank0,
                >= 0xC000 => bank1
            };

            address = (ushort) (address % 0x4000);
            int bankedAddress = 0x4000 * bank + address;
            bankedAddress %= PrgRomMemory.Length;

            return bankedAddress;
        }

        [MethodImpl(HotPath)]
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            switch (address) {
                case >= 0x6000 and <= 0x7FFF:
                    return BusDeviceHandlers.ReadWrite(PrgRamMemory, address, accessMode, ref bus);
                case >= 0x8000 and <= 0xFFFF:
                    switch (accessMode) {
                        case MemoryAccessMode.Read or MemoryAccessMode.ReadObserve:
                            State.PrgRomBankMode prgRomBankMode = state.GetPrgRomBankMode();

                            switch (prgRomBankMode) {
                                case State.PrgRomBankMode.Switch32KbAt0x8000:
                                case State.PrgRomBankMode.Switch32KbAt0x8000Alt: {
                                    int bankedAddress = (state.PrgBank & 0b0001_1110) * 0x8000 + address;
                                    bankedAddress -= 0x8000;

                                    //bankedAddress %= _prgRomMemory.Length;
                                    return BusDeviceHandlers.ReadOnlyIndexed(PrgRomMemory, bankedAddress, accessMode, ref bus);
                                }
                                case State.PrgRomBankMode.FixFirst16Kb: {
                                    // $8000-$BFFF: 16 KB PRG ROM bank, fixed to the first bank
                                    // $C000-$FFFF: 16 KB PRG ROM bank, switchable
                                    int bankedAddress = Calculate16KbBankedAddress(address, 0, state.PrgBank);
                                    return BusDeviceHandlers.ReadOnlyIndexed(PrgRomMemory, bankedAddress, accessMode, ref bus);
                                }
                                case State.PrgRomBankMode.FixLast16Kb: {
                                    // $8000-$BFFF: 16 KB PRG ROM bank, switchable
                                    // $C000-$FFFF: 16 KB PRG ROM bank, fixed to the first bank
                                    int bankedAddress = Calculate16KbBankedAddress(address, state.PrgBank, PrgRomMemory.Length / 0x4000 - 1);
                                    return BusDeviceHandlers.ReadOnlyIndexed(PrgRomMemory, bankedAddress, accessMode, ref bus);
                                }
                                default:
                                    Debug.Assert(false, $"Unknown prgRomBankMode ${prgRomBankMode}");
                                    return false;
                            }
                        case MemoryAccessMode.Write:
                            if ((bus & 0b1000_0000) != 0) {
                                state.Reset();
                            } else {
                                byte shiftedValue = (byte) ((bus & 0b0000_0001) << 4);
                                state.ShiftRegisterValue = (byte) ((state.ShiftRegisterValue >> 1) | shiftedValue);
                                state.Counter++;
                                if (state.Counter == 5) {
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

                                    state.ResetShiftRegister();
                                }
                            }

                            return true;
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

    public sealed class CartridgeToPpuBusDevice(State state, byte[]? chrRom, byte[]? chrRam) :
        CartridgeToPpuBusDeviceBase(chrRom, chrRam) {
        [MethodImpl(HotPath)]
        public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
            switch (address) {
                // 4 KB switchable CHR bank 1
                case >= 0x0000 and <= 0x0FFF: {
                    int bankedAddress = CalculateBankedAddress(address, state.ChrBank0, state.ChrBank0);
                    return HandleChrRomOrChrRam(bankedAddress, accessMode, ref bus);
                }

                // 4 KB switchable CHR bank2
                case >= 0x1000 and <= 0x1FFF: {
                    int bankedAddress = CalculateBankedAddress(address, state.ChrBank0, state.ChrBank1);
                    return HandleChrRomOrChrRam(bankedAddress, accessMode, ref bus);
                }

                // Nametables
                case >= 0x2000 and <= 0x3EFF: {
                    State.MirroringMode mirroringMode = state.GetMirroringMode();
                    return HandlePpuNametables(
                        address,
                        accessMode,
                        ref bus,
                        mirroringMode switch {
                            State.MirroringMode.OneScreenBank0 => PpuNametableMirroringType.OneScreenBlk0,
                            State.MirroringMode.OneScreenBank1 => PpuNametableMirroringType.OneScreenBlk1,
                            State.MirroringMode.TwoScreenVerticalMirroring => PpuNametableMirroringType.TwoScreenVerticalMirroring,
                            State.MirroringMode.TwoScreenHorizontalMirroring => PpuNametableMirroringType.TwoScreenHorizontalMirroring,
                            _ => PpuNametableMirroringType.OneScreenBlk0
                        }
                    );
                }
                default:
                    return DefaultPpuHandler(address, address, accessMode, ref bus, 0);
            }
        }

        [MethodImpl(HotPath)]
        private int CalculateBankedAddress(ushort address, byte switch8KbBank, byte switch4KbBank) {
            State.ChrRomBankMode chrRomBankMode = state.GetChrRomBankMode();

            int bankStartAddress;
            if (chrRomBankMode == State.ChrRomBankMode.Switch8KbBank) {
                bankStartAddress = (switch8KbBank & 0b0001_1110) * 0x2000;
            } else {
                bankStartAddress = switch4KbBank * 0x1000;
            }

            int bankedAddress = bankStartAddress + address;
            return bankedAddress;
        }
    }
}
