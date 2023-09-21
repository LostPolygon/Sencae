using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public readonly struct CartridgeToPpuBusDevice : IBusDevice, IDisposable {
    public readonly byte[] ChrRom;

    private readonly DirectArray<byte> _chrRomMemory;

    public CartridgeToPpuBusDevice(byte[] chrRom) {
        ChrRom = chrRom;
        _chrRomMemory = new DirectArray<byte>(ChrRom, -ChrRom.Length);
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: {
                return BusDeviceHandlers.ReadOnly(_chrRomMemory, _chrRomMemory.CalculateIndex(address), accessMode, ref bus);
            }
        }

        return false;
    }

    public void Dispose() {
        _chrRomMemory.Dispose();
    }
}
