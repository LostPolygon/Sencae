using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public readonly struct CartridgeToPpuBusDevice : IBusDevice {
    public readonly byte[] ChrRom;

    public readonly ArrayMemoryDevice ChrRomMemoryDevice;

    public CartridgeToPpuBusDevice(byte[] chrRom) {
        ChrRom = chrRom;
        ChrRomMemoryDevice = new ArrayMemoryDevice(ChrRom, -ChrRom.Length);
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, bool isWrite, ref byte bus) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: {
                ref byte mappedData = ref ChrRomMemoryDevice.GetU8(address);
                BusDeviceHandlers.ReadOnly(ref mappedData, isWrite, ref bus);

                return true;
            }
        }

        return false;
    }
}
