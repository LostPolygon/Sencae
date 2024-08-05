using System.Runtime.CompilerServices;
using Sencae.Mappers;
using Sencae.Ppu;

namespace Sencae;

public sealed class PpuMemoryBus(
    PpuBusDevice ppuBusDevice,
    IBusDevice cartridgeToPpuBusDevice
)
    : IBusDevice {
    public readonly PpuBusDevice PpuBusDevice = ppuBusDevice;
    public readonly IBusDevice CartridgeToPpuBusDevice = cartridgeToPpuBusDevice;

    [MethodImpl(HotPath)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        //bool ppuBusDevice = PpuBusDevice.HandleBus(address, accessMode, ref bus);
        bool cartridgeToPpuBusDevice = CartridgeToPpuBusDevice.HandleBus(address, accessMode, ref bus);

        //BusDeviceHandlers.AssertNotOpenBus(address);
        return cartridgeToPpuBusDevice;
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
