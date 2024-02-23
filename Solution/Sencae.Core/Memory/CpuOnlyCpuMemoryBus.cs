namespace Sencae;

public sealed class CpuOnlyCpuMemoryBus : CpuMemoryBusBase {
    public readonly CpuBusDevice CpuBusDevice;

    public CpuOnlyCpuMemoryBus(CpuBusDevice cpuBusDevice) {
        CpuBusDevice = cpuBusDevice;
    }

    public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        return CpuBusDevice.HandleBus(address, accessMode, ref bus);
    }
}
