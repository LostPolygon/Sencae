using System.Runtime.CompilerServices;

namespace Sencae;

public abstract class CpuMemoryBusBase : IBusDevice {
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

    [MethodImpl(HotPath)]
    public ushort ReadU16(ushort address) {
        return (ushort) (Read((ushort) (address + 1)) << 8 | Read(address));
    }

    public abstract bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus);
}
