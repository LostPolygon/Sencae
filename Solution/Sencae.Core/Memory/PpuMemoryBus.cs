using System.Runtime.CompilerServices;
using Sencae.Mappers;
using Sencae.Ppu;

namespace Sencae;

public sealed class PpuMemoryBus : IBusDevice {
    public readonly PpuCpuBusDevice PpuCpuBusDevice;
    public readonly Mapper000.CartridgeToPpuBusDevice CartridgeToPpuBusDevice;

    public PpuMemoryBus(PpuCpuBusDevice ppuCpuBusDevice, Mapper000.CartridgeToPpuBusDevice cartridgeToPpuBusDevice) {
        PpuCpuBusDevice = ppuCpuBusDevice;
        CartridgeToPpuBusDevice = cartridgeToPpuBusDevice;
    }

    [MethodImpl(HotPath)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        if (PpuCpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;

        if (CartridgeToPpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;

        BusDeviceHandlers.AssertNotOpenBus(address);
        return false;
    }

    [MethodImpl(HotPath)]
    public void Write(ushort address, byte data) {
        HandleBus(address, MemoryAccessMode.Write, ref data);
    }

    [MethodImpl(HotPath)]
    public byte Read(ushort address) {
        byte data = 0;
        HandleBus(address, MemoryAccessMode.Read, ref data);

        return data;
    }

    [MethodImpl(HotPath)]
    public ushort GetU16(ushort address) {
        return (ushort) (Read((ushort) (address + 1)) << 8 | Read(address));
    }
}
