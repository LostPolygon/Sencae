using System.Runtime.CompilerServices;

namespace Sencae;

public sealed class CpuMemoryBus : IBusDevice {
    public readonly Machine Machine;
    public byte LastBusData;

    public CpuMemoryBus(Machine machine) {
        Machine = machine;
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        if (Machine.CpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;
        
        if (Machine.CartridgeDevice.CartridgeToCpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;
        
        if (Machine.PpuCpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;
        
        //BusDeviceHandlers.OpenBus(address);
        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public void Write(ushort address, byte data) {
        HandleBus(address, MemoryAccessMode.Write, ref data);
    }

    [MethodImpl(MethodOptions.Options)]
    public byte Read(ushort address) {
        byte data = 0;
        HandleBus(address, MemoryAccessMode.Read, ref data);

        return data;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public byte ReadObserve(ushort address) {
        byte data = 0;
        HandleBus(address, MemoryAccessMode.ReadObserve, ref data);

        return data;
    }

    [MethodImpl(MethodOptions.Options)]
    public ushort ReadU16(ushort address) {
        return (ushort) (Read((ushort) (address + 1)) << 8 | Read(address));
    }
}
