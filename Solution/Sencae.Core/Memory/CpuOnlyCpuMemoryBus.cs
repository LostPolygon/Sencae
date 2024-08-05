namespace Sencae;

public sealed class CpuOnlyCpuMemoryBus(CpuBusDevice cpuBusDevice) : CpuMemoryBusBase {
    public readonly CpuBusDevice CpuBusDevice = cpuBusDevice;

    public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        return CpuBusDevice.HandleBus(address, accessMode, ref bus);
    }
}
