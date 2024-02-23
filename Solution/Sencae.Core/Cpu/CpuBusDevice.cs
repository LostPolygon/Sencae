using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public sealed class CpuBusDevice : IBusDevice, IDisposable {
    private readonly DirectArray<byte> _memory;
    private readonly DirectArray<byte> _ioRegisters;

    public DirectArray<byte> Memory => _memory;

    public CpuBusDevice(CpuState cpuState) {
        _memory = new DirectArray<byte>(cpuState.Memory);
        _ioRegisters = new DirectArray<byte>(cpuState.IORegisters);
    }

    [MethodImpl(HotPath)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: {
                ref byte mappedData = ref _memory[address % 0x800];
                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                return true;
            }
            case >= 0x4000 and <= 0x401F: {
                ref byte mappedData = ref _ioRegisters[address % (0x18 + 0x08)];
                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                if (address is 0x4016 or 0x4017 && accessMode == MemoryAccessMode.Read) {
                    bus = 0;
                }

                return true;
            }
        }

        return false;
    }

    private void ReleaseUnmanagedResources() {
        _memory.Dispose();
        _ioRegisters.Dispose();
    }

    private void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if (disposing) {
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~CpuBusDevice() {
        Dispose(false);
    }
}
