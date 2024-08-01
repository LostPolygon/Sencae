namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

public class TestMachine : IDisposable {
    public CpuState CpuState;
    public ulong CpuCycles;

    public TestCpuMemoryBus CpuMemoryBus;
    public CpuDevice<TestCpuMemoryBus, DummyDmaDevice> CpuDevice;

    public byte[] ExtraRam = new byte[0x10000];

    public TestMachine() {
        CpuState = new CpuState();

        CpuMemoryBus = new TestCpuMemoryBus(
            new Test64kRamCpuBusDevice(ExtraRam)
        );
        CpuDevice = new CpuDevice<TestCpuMemoryBus, DummyDmaDevice>(CpuState, CpuMemoryBus, new DummyDmaDevice(CpuState.OamDmaDevice));
        CpuDevice.HardReset(new CpuResetOptions {
            // Initializing memory is very slow in debug,
            // but since we create a new 0-filled array each time,
            // it's fine here
            CpuMemoryResetState = CpuMemoryResetState.Uninitialized,
        });
    }

    public void Clock() {
        CpuDevice.Clock();
    }

    public void Dispose() {
        CpuMemoryBus.Dispose();
    }
}
