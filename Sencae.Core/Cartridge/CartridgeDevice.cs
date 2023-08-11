using System.Runtime.CompilerServices;

namespace Sencae;

public readonly struct CartridgeDevice : IMemoryDevice {
    public readonly byte[] PrgRom;
    public readonly byte[] ChrRom;
    public readonly byte[]? PrgRam;

    public readonly ArrayMemoryDevice PrgRomMemoryDevice;
    public readonly ArrayMemoryDevice ChrRomMemoryDevice;
    public readonly ArrayMemoryDevice PrgRamMemoryDevice;

    public CartridgeDevice(byte[] prgRom, byte[] chrRom, byte[]? prgRam = null) {
        PrgRom = prgRom;
        ChrRom = chrRom;
        PrgRam = prgRam;

        PrgRomMemoryDevice = new ArrayMemoryDevice(PrgRom, 0x8000);
        ChrRomMemoryDevice = new ArrayMemoryDevice(ChrRom, -ChrRom.Length);
        PrgRamMemoryDevice = new ArrayMemoryDevice(PrgRam, 0x6000);
    }

    [MethodImpl(MethodOptions.Options)]
    public ref byte GetU8(ushort address) {
        switch (address) {
            case >= 0x8000 and <= 0xFFFF: return ref PrgRomMemoryDevice.GetU8(address);
            case >= 0x6000 and < 0x8000: {
                if (PrgRam != null) {
                    return ref PrgRamMemoryDevice.GetU8(address);
                }

                break;
            }
        }
        
        return ref DeviceUtility.OpenBus(address);
    }
    
    [MethodImpl(MethodOptions.Options)]
    public ref ushort GetU16(ushort address) {
        return ref ((IMemoryDevice) this).GetU16(address);
    }
}
