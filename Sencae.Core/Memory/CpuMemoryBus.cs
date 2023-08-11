using System.Runtime.CompilerServices;

namespace Sencae;

public readonly struct CpuMemoryBus : IMemoryDevice {
    public readonly Machine Machine;

    public CpuMemoryBus(Machine machine) {
        Machine = machine;
    }

    [MethodImpl(MethodOptions.Options)]
    public ref byte GetU8(ushort address) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: return ref Machine.CpuDevice.GetU8(address);
            case >= 0x2000 and <= 0x3FFF: return ref Machine.PpuDevice.GetU8(address);
            case >= 0x4000 and <= 0x401F: return ref Machine.CpuDevice.GetU8(address);
            case >= 0x4020 and <= 0xFFFF: return ref Machine.CartridgeDevice.GetU8(address);
        }
        
        return ref DeviceUtility.OpenBus(address);
    }

    [MethodImpl(MethodOptions.Options)]
    public ref ushort GetU16(ushort address) {
        ref byte data = ref GetU8(address);
        ref ushort data16 = ref Unsafe.As<byte, ushort>(ref data);
        return ref data16;
    }
}
