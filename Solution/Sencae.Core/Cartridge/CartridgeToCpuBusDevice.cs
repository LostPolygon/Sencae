using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public readonly struct CartridgeToCpuBusDevice : IBusDevice {
    public readonly byte[] PrgRom;
    public readonly byte[]? PrgRam;

    public readonly ArrayMemoryDevice PrgRomMemoryDevice;
    public readonly ArrayMemoryDevice? PrgRamMemoryDevice;

    public CartridgeToCpuBusDevice(byte[] prgRom, byte[]? prgRam = null) {
        PrgRom = prgRom;
        PrgRam = prgRam;

        PrgRomMemoryDevice = new ArrayMemoryDevice(PrgRom, 0x8000);

        if (PrgRam != null) {
            PrgRamMemoryDevice = new ArrayMemoryDevice(PrgRam, 0x6000);
        }
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, bool isWrite, ref byte bus) {
        switch (address) {
            case >= 0x8000 and <= 0xFFFF: {
                ref byte mappedData = ref PrgRomMemoryDevice.GetU8(address);
                BusDeviceHandlers.ReadOnly(ref mappedData, isWrite, ref bus);

                return true;
            }
            case >= 0x6000 and < 0x8000: {
                if (PrgRam != null) {
                    ref byte mappedData = ref PrgRamMemoryDevice.GetU8(address);
                    BusDeviceHandlers.ReadWrite(ref mappedData, isWrite, ref bus);
                    return true;
                }

                return false;
            }
        }

        return false;
    }
}
