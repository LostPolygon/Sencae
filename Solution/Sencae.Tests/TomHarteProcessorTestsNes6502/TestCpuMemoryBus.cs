namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

public class TestCpuMemoryBus : CpuMemoryBusBase, IDisposable {
    public readonly Test64kRamCpuBusDevice Test64KRamCpuBusDevice;

    public TestCpuMemoryBus(
        Test64kRamCpuBusDevice test64KRamCpuBusDevice
    ) {
        Test64KRamCpuBusDevice = test64KRamCpuBusDevice;
    }

    public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        if (Test64KRamCpuBusDevice.HandleBus(address, accessMode, ref bus))
            return true;

        BusDeviceHandlers.AssertNotOpenBus(address);
        return false;
    }

    public void Dispose() {
        Test64KRamCpuBusDevice.Dispose();
    }
}
