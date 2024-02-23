using System.Diagnostics;

namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

public class Test64kRamCpuBusDevice : IBusDevice, IDisposable {
    private DirectArray<byte> _extraRamMemory;

    public Test64kRamCpuBusDevice(byte[] extraRam) {
        Debug.Assert(extraRam.Length == 0x10000);
        _extraRamMemory = new DirectArray<byte>(extraRam, 0);
    }
    
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        BusDeviceHandlers.ReadWrite(_extraRamMemory, address, accessMode, ref bus);
        return true;
    }
    
    public void Dispose() {
        _extraRamMemory.Dispose();
    }
}
