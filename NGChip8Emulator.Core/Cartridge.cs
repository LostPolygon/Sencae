using System;
using System.Runtime.InteropServices;

namespace NGChip8Emulator; 

public class Cartridge {
    public byte[] PrgRom;
    public byte[] ChrRom;

    public ArrayMemory PrgRomMemory;
    public ArrayMemory ChrRomMemory;

    public Cartridge(byte[] prgRom, byte[] chrRom) {
        PrgRom = prgRom;
        ChrRom = chrRom;

        PrgRomMemory = new ArrayMemory(PrgRom);
        ChrRomMemory = new ArrayMemory(ChrRom);
    }
}
