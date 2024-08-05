namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

public class TestCpuMemoryBus(Test64KRamCpuBusDevice test64KRamCpuBusDevice) : CpuMemoryBusBase, IDisposable {
    public readonly Test64KRamCpuBusDevice Test64KRamCpuBusDevice = test64KRamCpuBusDevice;

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
