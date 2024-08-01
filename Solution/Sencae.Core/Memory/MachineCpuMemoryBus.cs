using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae;

public sealed class MachineCpuMemoryBus : CpuMemoryBusBase {
    public readonly CpuBusDevice CpuBusDevice;
    public readonly PpuCpuBusDevice PpuCpuBusDevice;
    public readonly IBusDevice CartridgeToCpuBusDevice;
    public readonly StandardControllersDevice StandardControllersDevice;
    public OamDmaDevice OamDmaDevice;

    public MachineCpuMemoryBus(
        CpuBusDevice cpuBusDevice,
        PpuCpuBusDevice ppuCpuBusDevice,
        IBusDevice cartridgeToCpuBusDevice,
        StandardControllersDevice standardControllersDevice
    ) {
        CpuBusDevice = cpuBusDevice;
        PpuCpuBusDevice = ppuCpuBusDevice;
        CartridgeToCpuBusDevice = cartridgeToCpuBusDevice;
        StandardControllersDevice = standardControllersDevice;
    }

    [MethodImpl(HotPath)]
    public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        /*if (address == 0x2001 && accessMode == MemoryAccessMode.Read) {
            Debug.WriteLine("Read from 0x2001");
        }
        if (address == 0x2003 && accessMode == MemoryAccessMode.Write) {
            Debug.WriteLine($"Write {bus} to 0x2003");
        }*/
        bool standardControllersDevice = StandardControllersDevice.HandleBus(address, accessMode, ref bus);
        bool cpuBusDevice = CpuBusDevice.HandleBus(address, accessMode, ref bus);
        bool cartridgeToCpuBusDevice = CartridgeToCpuBusDevice.HandleBus(address, accessMode, ref bus);
        bool oamDmaDevice = OamDmaDevice.HandleBus(address, accessMode, ref bus);
        //if (standardControllersDevice || cpuBusDevice || cartridgeToCpuBusDevice || oamDmaDevice)
        //    return true;
        
        bool ppuCpuBusDevice = PpuCpuBusDevice.HandleBus(address, accessMode, ref bus);

        //BusDeviceHandlers.OpenBus(address);
        return standardControllersDevice || cpuBusDevice || cartridgeToCpuBusDevice || ppuCpuBusDevice || oamDmaDevice;
    }
}
