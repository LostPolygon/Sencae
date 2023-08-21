using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public struct CartridgeDevice : IBusDevice {
    public readonly byte[] PrgRom;
    public readonly byte[] ChrRom;
    public readonly byte[]? PrgRam;

    public readonly ArrayMemoryDevice PrgRomMemoryDevice;
    public readonly ArrayMemoryDevice ChrRomMemoryDevice;
    public readonly ArrayMemoryDevice PrgRamMemoryDevice;

    public CartridgeDevice(int mapper, byte[] prgRom, byte[] chrRom, byte[]? prgRam = null) {
        switch (mapper) {
            case 0:
                break;
            default:
                throw new NotImplementedException($"Mapper {mapper} is not supported");
        }

        PrgRom = prgRom;
        ChrRom = chrRom;
        PrgRam = prgRam;

        PrgRomMemoryDevice = new ArrayMemoryDevice(PrgRom, 0x8000);
        ChrRomMemoryDevice = new ArrayMemoryDevice(ChrRom, -ChrRom.Length);
        PrgRamMemoryDevice = new ArrayMemoryDevice(PrgRam, 0x6000);
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, bool isWrite, ref byte bus) {
        switch (address) {
            case >= 0x8000 and <= 0xFFFF: {
                ref byte mappedData = ref PrgRomMemoryDevice.GetU8(address);
                Handle(ref mappedData, isWrite, ref bus);
                if (isWrite) {
                    mappedData = bus;
                } else {
                    bus = mappedData;
                }

                return true;
            }
            case >= 0x6000 and < 0x8000: {
                if (PrgRam != null) {
                    ref byte mappedData = ref PrgRamMemoryDevice.GetU8(address);
                    Handle(ref mappedData, isWrite, ref bus);
                    return true;
                }

                return false;
            }
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static void Handle(ref byte mappedData, bool isWrite, ref byte bus) {
        if (isWrite) {
            mappedData = bus;
        } else {
            bus = mappedData;
        }
    }
}
