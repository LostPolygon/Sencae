using System;
using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

using static PpuConstants;
using static PpuConstants.RegisterAddresses;

public sealed class PpuCpuBusDevice : IBusDevice, IDisposable {
    private readonly MachineState _machineState;
    private readonly PpuState _ppuState;
    private readonly PpuMemoryBus _ppuMemoryBus;

    public PpuCpuBusDevice(MachineState machineState, PpuState ppuState, PpuMemoryBus ppuBusDevice) {
        _machineState = machineState;
        _ppuState = ppuState;
        _ppuMemoryBus = ppuBusDevice;
    }

    [MethodImpl(HotPath)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        /*
        $2000–$2007 ($0008) NES PPU registers
        $2008–$3FFF ($1FF8) Mirrors of $2000–$2007 (repeats every 8 bytes)
        */

        //bool isPpuAddress = (address & 0b1110_0000_0000_0000) == 0b0010_0000_0000_0000; // 0x2000-0x3FFF
        bool isPpuAddress = address is >= 0x2000 and <= 0x3FFF;
        if (!isPpuAddress)
            return false;

        ref PpuState.PpuRegisterState registerState = ref _ppuState.Registers;
        ref PpuState.PpuInternalState internalState = ref _ppuState.Internal;

        // Normalize mirrored addresses
        address = (ushort) (address % 0x8 + FirstRegisterAddress);

        switch (accessMode) {
            case MemoryAccessMode.Read: {
                bus = internalState.CpuDataBusLatch;
                bool result = HandleBusInternal(address, accessMode, ref internalState.CpuDataBusLatch, ref registerState, ref internalState);
                return result;
            }
            case MemoryAccessMode.Write: {
                internalState.CpuDataBusLatch = bus;

                // Writes to the following registers are ignored if earlier than ~29658 CPU clocks after reset:
                // PPUCTRL, PPUMASK, PPUSCROLL, PPUADDR
                if (internalState.Cycles <= 29658 * 3) {
                    switch (address) {
                        case PPUCTRL: return true;
                        case PPUMASK: return true;
                        case PPUSCROLL: return true;
                        case PPUADDR: return true;
                    }
                }

                bool result = HandleBusInternal(address, accessMode, ref internalState.CpuDataBusLatch, ref registerState, ref internalState);

                return result;
            }
            case MemoryAccessMode.ReadObserve:
                return HandleBusInternal(address, accessMode, ref bus, ref registerState, ref internalState);
            default:
                return false;
        }
        
        /*ref byte cpuDataBusLatch = ref internalState.CpuDataBusLatch;

if (accessMode == MemoryAccessMode.Write) {
    cpuDataBusLatch = bus;
}

bool result = HandleBusInternal(address, accessMode, ref cpuDataBusLatch, out bool _, ref registerState, ref internalState);

bus = cpuDataBusLatch;*/

        //PPUCTRL, PPUMASK, PPUSCROLL, PPUADDR, the PPUSCROLL/PPUADDR latch, and the PPUDATA
        /*if (internalState.Reset) {
            registerState.PPUCTRL = 0;
            registerState.PPUMASK = 0;
            registerState.PPUSTATUS = 0;
            registerState.OAMADDR = 0;

            internalState.Address.Value = 0;
            internalState.PpuAddressRegisterLatch = false;
            internalState.PpuScrollRegisterLatch = false;
            internalState.FineScrollX = 0;
            internalState.FineScrollY = 0;
        }*/
    }

    [MethodImpl(HotPath)]
    private bool HandleBusInternal(
        ushort address,
        MemoryAccessMode accessMode,
        ref byte bus,
        ref PpuState.PpuRegisterState registerState,
        ref PpuState.PpuInternalState internalState
    ) {
        switch (address) {
            // 0x2000
            case PPUCTRL: {
                switch (accessMode) {
                    case MemoryAccessMode.Write:
                    case MemoryAccessMode.ReadObserve:
                        ref byte mappedData = ref Unsafe.As<PpuControlRegisterFlags, byte>(ref registerState.PPUCTRL);
                        BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);

                        //Debug.WriteLine($"Set PPUCTRL to: {registerState.PPUCTRL}");
                        return true;
                }

                break;
            }

            // 0x2001
            case PPUMASK: {
                switch (accessMode) {
                    case MemoryAccessMode.Write:
                    case MemoryAccessMode.ReadObserve:
                        ref byte mappedData = ref Unsafe.As<PpuMaskRegisterFlags, byte>(ref registerState.PPUMASK);
                        BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);

                        return true;
                }

                break;
            }

            // 0x2002
            case PPUSTATUS: {
                ref byte mappedData = ref Unsafe.As<PpuStatusRegisterFlags, byte>(ref registerState.PPUSTATUS);
                byte oldBus = bus;

                switch (accessMode) {
                    case MemoryAccessMode.Read:
                    case MemoryAccessMode.ReadObserve:
                        BusDeviceHandlers.ReadOnlyRaw(ref mappedData, accessMode, ref bus);
                        bus = (byte) (bus | (oldBus & (int) PpuStatusRegisterFlags.StatusMask));
                        
                        if (accessMode == MemoryAccessMode.Read) {
                            internalState.LastPPUSTATUS_ReadCycle = _machineState.MachineCycles;
                            internalState.PpuAddressRegisterLatch = false;
                            internalState.PpuScrollRegisterLatch = false;

                            //internalState.Address = default;

                            mappedData &= (byte) ~PpuStatusRegisterFlags.VerticalBlankStarted;
                        }
                        
                        break;
                }

                //bus = (byte) (bus | (oldBus & (int) PpuStatusRegisterFlags.StatusMask));

                return true;
            }

            // 0x2003
            case OAMADDR: {
                switch (accessMode) {
                    case MemoryAccessMode.Write:
                    case MemoryAccessMode.ReadObserve:
                        ref byte mappedData = ref registerState.OAMADDR;
                        BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                        return true;
                }

                break;
            }

            // 0x2004
            case OAMDATA: {
                ref byte mappedData = ref _ppuState.OamMemory[registerState.OAMADDR];
                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                if (accessMode == MemoryAccessMode.Write) {
                    registerState.OAMADDR++;
                }

                return true;
            }

            // 0x2005
            case PPUSCROLL: {
                byte dummyByte = 0;
                ref byte mappedData = ref dummyByte;

                if (internalState.PpuScrollRegisterLatch) {
                    mappedData = ref internalState.FineScrollY;
                } else {
                    mappedData = ref internalState.FineScrollX;
                }

                switch (accessMode) {
                    case MemoryAccessMode.Write:
                        mappedData = bus;
                        internalState.PpuScrollRegisterLatch = !internalState.PpuScrollRegisterLatch;
                        internalState.PpuAddressRegisterLatch = !internalState.PpuAddressRegisterLatch;
                        return true;
                    case MemoryAccessMode.ReadObserve:
                        bus = mappedData;
                        return true;
                }

                break;
            }

            // 0x2006
            case PPUADDR: {
                byte dummyByte = 0;
                ref byte mappedData = ref dummyByte;

                if (internalState.PpuAddressRegisterLatch) {
                    mappedData = ref internalState.Address.Low;
                } else {
                    mappedData = ref internalState.AddressHighTemp;
                }

                switch (accessMode) {
                    case MemoryAccessMode.Write:
                        mappedData = bus;
                        if (internalState.PpuAddressRegisterLatch) {
                            internalState.Address.High = internalState.AddressHighTemp;
                        }

                        internalState.Address.Value &= 0x7FFF;

                        internalState.PpuAddressRegisterLatch = !internalState.PpuAddressRegisterLatch;
                        if ((registerState.PPUSTATUS & PpuStatusRegisterFlags.VerticalBlankStarted) == 0) {
                        }

                        return true;
                    case MemoryAccessMode.ReadObserve:
                        bus = mappedData;
                        return true;
                }

                break;
            }

            // 0x2007
            case PPUDATA: {
                // Contents in range 0–$3EFF (before pallete RAM) are buffered
                ref ushort vramAddress = ref internalState.Address.Value;
                if (accessMode != MemoryAccessMode.Write) {
                    switch (vramAddress) {
                        case <= 0x3EFF:
                            bus = internalState.DataBusLatch;

                            _ppuMemoryBus.HandleBus(vramAddress, accessMode, ref internalState.DataBusLatch);
                            break;
                        case <= 0x3FFF:
                            /*
                             * "Reading palette data from $3F00–$3FFF works differently.
                             * The palette data is placed immediately on the data bus,
                             * and hence no priming read is required.
                             * Reading the palettes still updates the internal buffer though,
                             * but the data placed in it is the mirrored nametable data
                             * that would appear "underneath" the palette."
                             */
                            _ppuMemoryBus.HandleBus(vramAddress, accessMode, ref bus);
                            _ppuMemoryBus.HandleBus(
                                (ushort) (vramAddress % 0x1000 + Nametable0StartAddress), // FIXME: is this correct?
                                accessMode,
                                ref internalState.DataBusLatch
                            );

                            //mustUpdateBus = false;
                            break;
                    }
                } else {
                    _ppuMemoryBus.HandleBus(vramAddress, accessMode, ref bus);
                }

                if (accessMode != MemoryAccessMode.ReadObserve) {
                    int addressIncrement =
                        (registerState.PPUCTRL & PpuControlRegisterFlags.VramAddressIncrement) == 0 ?
                            1 :
                            32;

                    vramAddress = (ushort) (vramAddress + addressIncrement);
                }

                return true;
            }
            default:
                throw new IndexOutOfRangeException($"Unexpected PPU register at address {address}");
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
