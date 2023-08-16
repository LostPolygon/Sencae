using System.Runtime.CompilerServices;

namespace Sencae; 

public readonly unsafe struct PpuDevice : IBusDevice {
    public readonly Machine Machine;

    public PpuDevice(Machine machine) {
        Machine = machine;
    }
    
    public void Reset() {
    }

    [MethodImpl(MethodOptions.Options)]
    public void HandleBus(ushort address, bool isWrite, ref byte bus) {
        /*
        $2000–$2007 ($0008) NES PPU registers
        $2008–$3FFF ($1FF8) Mirrors of $2000–$2007 (repeats every 8 bytes)
        */

        bool isMapped = true;
        byte dummyByte = 0;
        ref byte mappedData = ref dummyByte;
        switch (address) {
            case >= 0x2000 and <= 0x3FFF: {
                mappedData = ref Machine.State.Ppu.Registers[address % 0x8];
                break;
            }
            default:
                isMapped = false;
                break;
        }

        if (!isMapped)
            return;

        if (isWrite) {
            mappedData = bus;
        } else {
            bus = mappedData;
        }
    }
}
