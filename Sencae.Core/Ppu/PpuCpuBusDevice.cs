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

    public void Reset() {
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, bool isWrite, ref byte bus) {
        /*
        $2000–$2007 ($0008) NES PPU registers
        $2008–$3FFF ($1FF8) Mirrors of $2000–$2007 (repeats every 8 bytes)
        */
        switch (address) {
            case >= 0x2000 and <= 0x3FFF: {
                int registerIndex = address % 0x8;
                switch (registerIndex) {
                    case PPUCTRL_Index: {
                        ref byte mappedData = ref Unsafe.As<PpuControlRegisterFlags, byte>(ref _state.PPUCTRL);
                        BusDeviceHandlers.WriteOnly(ref mappedData, isWrite, ref bus);
                        return true;
                    }
                    case PPUMASK_Index: {
                        ref byte mappedData = ref Unsafe.As<PpuMaskRegisterFlags, byte>(ref _state.PPUMASK);
                        BusDeviceHandlers.WriteOnly(ref mappedData, isWrite, ref bus);
                        return true;
                    }
                    case PPUSTATUS_Index: {
                        ref byte mappedData = ref Unsafe.As<PpuStatusRegisterFlags, byte>(ref _state.PPUSTATUS);
                        byte oldBus = bus;
                        BusDeviceHandlers.ReadOnly(ref mappedData, isWrite, ref bus);
                        bus = (byte) (
                            (bus & (int) ~PpuStatusRegisterFlags.OpenBusMask) |
                            (oldBus & (int) PpuStatusRegisterFlags.OpenBusMask)
                        );

                        if (!isWrite) {
                            _state.Internal.PpuAddressRegisterLatch = false;
                            _state.Internal.PpuScrollRegisterLatch = false;
                            _state.Internal.Address = default;
                        }
                        
                        return true;
                    }
                    case OAMADDR_Index: {
                        ref byte mappedData = ref _state.OAMADDR;
                        BusDeviceHandlers.WriteOnly(ref mappedData, isWrite, ref bus);
                        return true;
                    }
                    case OAMDATA_Index: {
                        ref byte mappedData = ref _state.OAMDATA;
                        BusDeviceHandlers.ReadWrite(ref mappedData, isWrite, ref bus);
                        if (isWrite) {
                            _state.OAMADDR++;
                        }

                        return true;
                    }
                    case PPUSCROLL_Index: {
                        byte dummyByte = 0;
                        ref byte mappedData = ref dummyByte;

                        if (_state.Internal.PpuScrollRegisterLatch) {
                            mappedData = ref _state.Internal.FineScrollY;
                        } else {
                            mappedData = ref _state.Internal.FineScrollX;
                        }

                        if (isWrite) {
                            mappedData = bus;
                            _state.Internal.PpuScrollRegisterLatch = !_state.Internal.PpuScrollRegisterLatch;
                        }

                        return true;
                    }
                    case PPUADDR_Index: {
                        if (isWrite) {
                            byte dummyByte = 0;
                            ref byte mappedData = ref dummyByte;

                            if (_state.Internal.PpuAddressRegisterLatch) {
                                mappedData = ref _state.Internal.Address.Low;
                            } else {
                                mappedData = ref _state.Internal.Address.High;
                            }
                            
                            mappedData = bus;
                            _state.Internal.PpuAddressRegisterLatch = !_state.Internal.PpuAddressRegisterLatch;
                        }

                        return true;
                    }
                    case PPUDATA_Index: {
                        // Contents in range 0–$3EFF (before pallete RAM) are buffered
                        ref ushort vramAddress = ref _state.Internal.Address.Value;
                        if (!isWrite) {
                            if (vramAddress <= 0x3EFF) {
                                bus = _state.Internal.DataBusLatch;

                                _ppuBusDevice.HandleBus(vramAddress, isWrite, ref _state.Internal.DataBusLatch);
                            } else if (vramAddress <= 0x3FFF) {
                                /*
                                 * "Reading palette data from $3F00–$3FFF works differently.
                                 * The palette data is placed immediately on the data bus,
                                 * and hence no priming read is required.
                                 * Reading the palettes still updates the internal buffer though,
                                 * but the data placed in it is the mirrored nametable data
                                 * that would appear "underneath" the palette."
                                 */

                                _ppuBusDevice.HandleBus(vramAddress, isWrite, ref bus);
                                _ppuBusDevice.HandleBus((ushort) (vramAddress % 0x3EFF), isWrite, ref _state.Internal.DataBusLatch);
                            }
                        } else {
                            _ppuBusDevice.HandleBus(vramAddress, isWrite, ref bus);
                        }

                        int addressIncrement =
                            (_state.PPUCTRL & PpuControlRegisterFlags.VramAddressIncrement) == 0 ?
                                1 :
                                32;

                        vramAddress = (ushort) (vramAddress + addressIncrement);

                        return true;
                    }
                    default:
                        throw new IndexOutOfRangeException($"Unexpected PPU register at address {address}");
                }
            }
            case 0x4014: {
                return true;
            }
        }

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
