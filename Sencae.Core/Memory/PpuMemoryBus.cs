using System.Runtime.CompilerServices;

namespace Sencae;

public sealed class PpuMemoryBus : IBusDevice {
    public readonly Machine Machine;
    public byte LastBusData;

    public PpuMemoryBus(Machine machine) {
        Machine = machine;
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, bool isWrite, ref byte bus) {
        if (Machine.PpuCpuBusDevice.HandleBus(address, isWrite, ref bus))
            return true;

        if (Machine.CartridgeDevice.CartridgeToPpuBusDevice.HandleBus(address, isWrite, ref bus))
            return true;

        DeviceUtility.OpenBus(address);
        return true;
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
}
