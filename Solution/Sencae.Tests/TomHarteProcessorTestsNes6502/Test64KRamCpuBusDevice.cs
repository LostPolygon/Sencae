using System.Diagnostics;

namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

public class Test64KRamCpuBusDevice : IBusDevice, IDisposable {
    private readonly DirectArray<byte> _extraRamMemory;

    public Test64KRamCpuBusDevice(byte[] extraRam) {
        Debug.Assert(extraRam.Length == 0x10000);
        _extraRamMemory = new DirectArray<byte>(extraRam);
    }
    
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        BusDeviceHandlers.ReadWrite(_extraRamMemory, address, accessMode, ref bus);
        return true;
    }
    
    public void Dispose() {
        _extraRamMemory.Dispose();
    }
}
