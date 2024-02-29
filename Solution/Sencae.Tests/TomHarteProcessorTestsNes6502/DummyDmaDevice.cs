namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

public class DummyDmaDevice(DmaDeviceState state) : DmaDevice(state) {
    protected override void ExecuteCycle() {
    }

    public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        return false;
    }
}
