using System.Runtime.CompilerServices;

namespace Sencae;

public struct CpuMemoryBus : IBusDevice {
    public readonly Machine Machine;
    public byte LastBusData;

    public CpuMemoryBus(Machine machine) {
        Machine = machine;
    }

    [MethodImpl(MethodOptions.Options)]
    public void Write(ushort address, byte data) {
        HandleBus(address, true, ref data);
    }
    
    [MethodImpl(MethodOptions.Options)]
    public byte Read(ushort address) {
        byte data = 0;
        HandleBus(address, false, ref data);

        return data;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public ushort GetU16(ushort address) {
        return (ushort) (Read((ushort) (address + 1)) << 8 | Read(address));
    }
    
    [MethodImpl(MethodOptions.Options)]
    public void HandleBus(ushort address, bool isWrite, ref byte bus) {
        Machine.CpuDevice.HandleBus(address, isWrite, ref bus);
        Machine.PpuDevice.HandleBus(address, isWrite, ref bus);
        Machine.CartridgeDevice.HandleBus(address, isWrite, ref bus);
    }
}
