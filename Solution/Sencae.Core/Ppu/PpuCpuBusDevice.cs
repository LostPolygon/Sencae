using System;
using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

using static PpuConstants;
using static PpuConstants.RegisterAddresses;

public unsafe class PpuCpuBusDevice : IBusDevice, IDisposable {
    public readonly Machine Machine;

    private readonly PpuState _state;
    private readonly PpuBusDevice _ppuBusDevice;

    public PpuCpuBusDevice(Machine machine) {
        Machine = machine;

        _state = machine.State.Ppu;
        _ppuBusDevice = machine.PpuBusDevice;
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        switch (accessMode) {
            case MemoryAccessMode.Read:
            case MemoryAccessMode.Write:
                ref byte ppuBus = ref _state.Internal.DataBus;

                if (accessMode == MemoryAccessMode.Write) {
                    ppuBus = bus;
                }

                bool result = HandleBusInternal(address, accessMode, ref ppuBus, out bool _);

                bus = ppuBus;
                return result;
            case MemoryAccessMode.ReadObserve:
                return HandleBusInternal(address, accessMode, ref bus, out bool _);
            default:
                throw new ArgumentOutOfRangeException(nameof(accessMode), accessMode, null);
        }
    }

    private bool HandleBusInternal(ushort address, MemoryAccessMode accessMode, ref byte bus, out bool mustUpdateBus) {
        /*
        $2000–$2007 ($0008) NES PPU registers
        $2008–$3FFF ($1FF8) Mirrors of $2000–$2007 (repeats every 8 bytes)
        */

        mustUpdateBus = true;
        switch (address) {
            case >= 0x2000 and <= 0x3FFF: {
                int registerIndex = address % 0x8;

                // Writes to the following registers are ignored if earlier than ~29658 CPU clocks after reset:
                // PPUCTRL, PPUMASK, PPUSCROLL, PPUADDR
                if (accessMode == MemoryAccessMode.Write && Machine.State.CpuCycles <= 29658) {
                    switch (registerIndex) {
                        case PPUCTRL_Index: return true;
                        case PPUMASK_Index: return true;
                        case PPUSCROLL_Index: return true;
                        case PPUADDR_Index: return true;
                    }
                }

                switch (registerIndex) {
                    // 0x2000
                    case PPUCTRL_Index: {
                        switch (accessMode) {
                            case MemoryAccessMode.Write:
                            case MemoryAccessMode.ReadObserve:
                                ref byte mappedData = ref Unsafe.As<PpuControlRegisterFlags, byte>(ref _state.PPUCTRL);
                                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                                return true;
                        }

                        break;
                    }

                    // 0x2001
                    case PPUMASK_Index: {
                        switch (accessMode) {
                            case MemoryAccessMode.ReadObserve:
                            case MemoryAccessMode.Write:
                                ref byte mappedData = ref Unsafe.As<PpuMaskRegisterFlags, byte>(ref _state.PPUMASK);
                                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                                if (accessMode == MemoryAccessMode.Write) {
                                    _state.PPUSTATUS |= PpuStatusRegisterFlags.VerticalBlankStarted;
                                }

                                return true;
                        }

                        break;
                    }

                    // 0x2002
                    case PPUSTATUS_Index: {
                        ref byte mappedData = ref Unsafe.As<PpuStatusRegisterFlags, byte>(ref _state.PPUSTATUS);
                        byte oldBus = bus;
                        if (accessMode != MemoryAccessMode.Write) {
                            BusDeviceHandlers.ReadOnlyRaw(ref mappedData, accessMode, ref bus);
                        }

                        bus = (byte) (
                            (bus & (int) ~PpuStatusRegisterFlags.OpenBusMask) |
                            (oldBus & (int) PpuStatusRegisterFlags.OpenBusMask)
                        );

                        if (accessMode == MemoryAccessMode.Read) {
                            _state.Internal.PpuAddressRegisterLatch = false;
                            _state.Internal.PpuScrollRegisterLatch = false;
                            _state.Internal.Address = default;
                            mappedData &= (byte) ~PpuStatusRegisterFlags.VerticalBlankStarted;
                        }

                        return true;
                    }

                    // 0x2003
                    case OAMADDR_Index: {
                        switch (accessMode) {
                            case MemoryAccessMode.Write:
                            case MemoryAccessMode.ReadObserve:
                                ref byte mappedData = ref _state.OAMADDR;
                                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                                return true;
                        }

                        break;
                    }

                    // 0x2004
                    case OAMDATA_Index: {
                        ref byte mappedData = ref _state.OamMemory[_state.OAMADDR];
                        BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                        if (accessMode == MemoryAccessMode.Write) {
                            _state.OAMADDR++;
                        }

                        return true;
                    }

                    // 0x2005
                    case PPUSCROLL_Index: {
                        byte dummyByte = 0;
                        ref byte mappedData = ref dummyByte;

                        if (_state.Internal.PpuScrollRegisterLatch) {
                            mappedData = ref _state.Internal.FineScrollY;
                        } else {
                            mappedData = ref _state.Internal.FineScrollX;
                        }

                        switch (accessMode) {
                            case MemoryAccessMode.Write:
                                mappedData = bus;
                                _state.Internal.PpuScrollRegisterLatch = !_state.Internal.PpuScrollRegisterLatch;
                                _state.Internal.PpuAddressRegisterLatch = !_state.Internal.PpuAddressRegisterLatch;
                                return true;
                            case MemoryAccessMode.ReadObserve:
                                bus = mappedData;
                                return true;
                        }

                        break;
                    }

                    // 0x2006
                    case PPUADDR_Index: {
                        byte dummyByte = 0;
                        ref byte mappedData = ref dummyByte;

                        if (_state.Internal.PpuAddressRegisterLatch) {
                            mappedData = ref _state.Internal.Address.Low;
                        } else {
                            mappedData = ref _state.Internal.AddressHighTemp;
                        }

                        switch (accessMode) {
                            case MemoryAccessMode.Write:
                                mappedData = bus;
                                if (_state.Internal.PpuAddressRegisterLatch) {
                                    _state.Internal.Address.High = _state.Internal.AddressHighTemp;
                                }

                                _state.Internal.PpuAddressRegisterLatch = !_state.Internal.PpuAddressRegisterLatch;
                                if ((_state.PPUSTATUS & PpuStatusRegisterFlags.VerticalBlankStarted) == 0) {
                                }

                                return true;
                            case MemoryAccessMode.ReadObserve:
                                bus = mappedData;
                                return true;
                        }

                        break;
                    }

                    // 0x2007
                    case PPUDATA_Index: {
                        // Contents in range 0–$3EFF (before pallete RAM) are buffered
                        ref ushort vramAddress = ref _state.Internal.Address.Value;
                        if (accessMode != MemoryAccessMode.Write) {
                            if (vramAddress <= 0x3EFF) {
                                bus = _state.Internal.DataBusLatch;

                                _ppuBusDevice.HandleBus(vramAddress, accessMode, ref _state.Internal.DataBusLatch);
                            } else if (vramAddress <= 0x3FFF) {
                                /*
                                 * "Reading palette data from $3F00–$3FFF works differently.
                                 * The palette data is placed immediately on the data bus,
                                 * and hence no priming read is required.
                                 * Reading the palettes still updates the internal buffer though,
                                 * but the data placed in it is the mirrored nametable data
                                 * that would appear "underneath" the palette."
                                 */

                                _ppuBusDevice.HandleBus(vramAddress, accessMode, ref bus);
                                _ppuBusDevice.HandleBus(
                                    (ushort) (vramAddress % 0x1000 + Nametable0StartAddress), // FIXME: is this correct?
                                    accessMode,
                                    ref _state.Internal.DataBusLatch
                                );

                                mustUpdateBus = false;
                            }
                        } else {
                            _ppuBusDevice.HandleBus(vramAddress, accessMode, ref bus);
                        }

                        if (accessMode != MemoryAccessMode.ReadObserve) {
                            int addressIncrement =
                                (_state.PPUCTRL & PpuControlRegisterFlags.VramAddressIncrement) == 0 ?
                                    1 :
                                    32;

                            vramAddress = (ushort) (vramAddress + addressIncrement);
                        }

                        return true;
                    }
                    default:
                        throw new IndexOutOfRangeException($"Unexpected PPU register at address {address}");
                }

                break;
            }
            case 0x4014: {
                return true;
            }
        }

        mustUpdateBus = false;
        return false;
    }

    private void ReleaseUnmanagedResources() {
    }

    private void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if (disposing) {
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~PpuCpuBusDevice() {
        Dispose(false);
    }
}
