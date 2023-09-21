using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public readonly struct CartridgeToCpuBusDevice : IBusDevice, IDisposable {
    public readonly byte[] PrgRom;
    public readonly byte[]? PrgRam;

    private readonly DirectArray<byte> _prgRomMemory;
    private readonly DirectArray<byte>? _prgRamMemory;

    public CartridgeToCpuBusDevice(byte[] prgRom, byte[]? prgRam = null) {
        PrgRom = prgRom;
        PrgRam = prgRam;

        _prgRomMemory = new DirectArray<byte>(PrgRom, 0x8000);

        if (PrgRam != null) {
            _prgRamMemory = new DirectArray<byte>(PrgRam, 0x6000);
        }
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        switch (address) {
            case >= 0x8000 and <= 0xFFFF: {
                return BusDeviceHandlers.ReadOnly(_prgRomMemory, _prgRomMemory.CalculateIndex(address), accessMode, ref bus);
            }
            case >= 0x6000 and < 0x8000: {
                if (_prgRamMemory != null) {
                    BusDeviceHandlers.ReadWrite(_prgRamMemory.Value, _prgRamMemory.Value.CalculateIndex(address), accessMode, ref bus);
                    return true;
                }

                return false;
            }
        }

        return false;
    }

    public void Dispose() {
        _prgRomMemory.Dispose();
        _prgRamMemory?.Dispose();
    }
}
