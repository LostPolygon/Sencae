using System.Runtime.CompilerServices;
using Sencae.Mappers;
using Sencae.Ppu;

namespace Sencae;

public sealed class MachineCpuMemoryBus : CpuMemoryBusBase {
    public readonly CpuBusDevice CpuBusDevice;
    public readonly PpuCpuBusDevice PpuCpuBusDevice;
    public readonly Mapper000.CartridgeToCpuBusDevice CartridgeToCpuBusDevice;

    public MachineCpuMemoryBus(
        CpuBusDevice cpuBusDevice,
        PpuCpuBusDevice ppuCpuBusDevice,
        Mapper000.CartridgeToCpuBusDevice cartridgeToCpuBusDevice
    ) {
        CpuBusDevice = cpuBusDevice;
        PpuCpuBusDevice = ppuCpuBusDevice;
        CartridgeToCpuBusDevice = cartridgeToCpuBusDevice;
    }

    [MethodImpl(HotPath)]
    public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        if (CpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;

        if (CartridgeToCpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;

        if (PpuCpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;

        //BusDeviceHandlers.OpenBus(address);
        return false;
    }
}
