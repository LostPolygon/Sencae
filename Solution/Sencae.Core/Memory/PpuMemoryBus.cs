using System.Runtime.CompilerServices;
using Sencae.Mappers;
using Sencae.Ppu;

namespace Sencae;

public sealed class PpuMemoryBus : IBusDevice {
    public readonly PpuBusDevice PpuBusDevice;
    public readonly IBusDevice CartridgeToPpuBusDevice;

    public PpuMemoryBus(
        PpuBusDevice ppuBusDevice,
        IBusDevice cartridgeToPpuBusDevice
    ) {
        PpuBusDevice = ppuBusDevice;
        CartridgeToPpuBusDevice = cartridgeToPpuBusDevice;
    }

    [MethodImpl(HotPath)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        if (PpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;

        if (CartridgeToPpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;

        //BusDeviceHandlers.AssertNotOpenBus(address);
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
    public byte ReadObserve(ushort address) {
        byte data = 0;
        HandleBus(address, MemoryAccessMode.ReadObserve, ref data);

        return data;
    }
}
