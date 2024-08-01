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
        ref PpuState.PpuRegisterState registerState = ref _ppuState.Registers;
        ref PpuState.PpuInternalState internalState = ref _ppuState.Internal;

        bool isPpuAddress = address is >= 0x2000 and <= 0x3FFF;
        if (!isPpuAddress)
            return false;
        
        switch (accessMode) {
            case MemoryAccessMode.Read:
            case MemoryAccessMode.Write:
                if (accessMode == MemoryAccessMode.Write) {
                    internalState.CpuDataBusLatch = bus;
                }

                bool result = HandleBusInternal(address, accessMode, ref internalState.CpuDataBusLatch, out bool _, ref registerState, ref internalState);
                if (accessMode == MemoryAccessMode.Read) {
                    bus = internalState.CpuDataBusLatch;
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

                return result;
            case MemoryAccessMode.ReadObserve:
                return HandleBusInternal(address, accessMode, ref bus, out bool _, ref registerState, ref internalState);
            default:
                throw new ArgumentOutOfRangeException(nameof(accessMode), accessMode, null);
        }
    }

    [MethodImpl(HotPath)]
    private bool HandleBusInternal(
        ushort address,
        MemoryAccessMode accessMode,
        ref byte bus,
        out bool mustUpdateBus,
        ref PpuState.PpuRegisterState registerState,
        ref PpuState.PpuInternalState internalState
    ) {
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
                if (accessMode == MemoryAccessMode.Write && internalState.Cycles <= 29658 * 3) {
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
                                ref byte mappedData = ref Unsafe.As<PpuControlRegisterFlags, byte>(ref registerState.PPUCTRL);
                                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);

                                //Debug.WriteLine($"Set PPUCTRL to: {registerState.PPUCTRL}");
                                return true;
                        }

                        break;
                    }

                    // 0x2001
                    case PPUMASK_Index: {
                        switch (accessMode) {
                            case MemoryAccessMode.ReadObserve:
                            case MemoryAccessMode.Write:
                                ref byte mappedData = ref Unsafe.As<PpuMaskRegisterFlags, byte>(ref registerState.PPUMASK);
                                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);

                                return true;
                        }

                        break;
                    }

                    // 0x2002
                    case PPUSTATUS_Index: {
                        ref byte mappedData = ref Unsafe.As<PpuStatusRegisterFlags, byte>(ref registerState.PPUSTATUS);
                        byte oldBus = bus;

                        if (accessMode != MemoryAccessMode.Write) {
                            BusDeviceHandlers.ReadOnlyRaw(ref mappedData, accessMode, ref bus);
                        }

                        bus = (byte) (
                            (bus & (int) ~PpuStatusRegisterFlags.StatusMask) |
                            (oldBus & (int) PpuStatusRegisterFlags.StatusMask)
                        );

                        if (accessMode == MemoryAccessMode.Read) {
                            internalState.LastPPUSTATUS_ReadCycle = _machineState.MachineCycles;
                            internalState.PpuAddressRegisterLatch = false;
                            internalState.PpuScrollRegisterLatch = false;

                            //internalState.Address = default;

                            mappedData &= (byte) ~PpuStatusRegisterFlags.VerticalBlankStarted;
                        }

                        bus = (byte) (
                            (bus & (int) ~PpuStatusRegisterFlags.StatusMask) |
                            (oldBus & (int) PpuStatusRegisterFlags.StatusMask)
                        );

                        return true;
                    }

                    // 0x2003
                    case OAMADDR_Index: {
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
                    case OAMDATA_Index: {
                        ref byte mappedData = ref _ppuState.OamMemory[registerState.OAMADDR];
                        BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                        if (accessMode == MemoryAccessMode.Write) {
                            registerState.OAMADDR++;
                        }

                        return true;
                    }

                    // 0x2005
                    case PPUSCROLL_Index: {
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
                    case PPUADDR_Index: {
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
                    case PPUDATA_Index: {
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

                                    mustUpdateBus = false;
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

                break;
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
