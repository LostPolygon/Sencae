using System.Runtime.CompilerServices;

namespace Sencae; 

public readonly unsafe struct PpuDevice : IMemoryDevice {
    public readonly Machine Machine;

    public PpuDevice(Machine machine) {
        Machine = machine;
    }

    [MethodImpl(MethodOptions.Options)]
    public ref byte GetU8(ushort address) {
        /*
         $2000–$2007 ($0008) NES PPU registers
         $2008–$3FFF ($1FF8) Mirrors of $2000–$2007 (repeats every 8 bytes)  
         */
        switch (address) {
            case >= 0x2000 and <= 0x3FFF: return ref Machine.State.Ppu.Registers[address % 0x8];
        }
        
        return ref DeviceUtility.OpenBus(address);
    }

    /*public ref ushort GetU16(ushort address) {
        return ref ((MemoryDevice) this).GetU16(address);
    }*/

    public void Reset() {
    }
}
