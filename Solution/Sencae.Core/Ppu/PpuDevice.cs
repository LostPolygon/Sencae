namespace Sencae.Ppu;

public unsafe class PpuDevice {
    public readonly Machine Machine;

    public PpuDevice(Machine machine) {
        Machine = machine;
    }

    public void SoftReset() {
        Machine.PpuMemoryBus.Write(PpuConstants.RegisterAddresses.PPUCTRL, 0);
        Machine.PpuMemoryBus.Write(PpuConstants.RegisterAddresses.PPUMASK, 0);
    }
}
