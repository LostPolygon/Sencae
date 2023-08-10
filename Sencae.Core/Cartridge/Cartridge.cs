namespace Sencae; 

public class Cartridge {
    public byte[] PrgRom;
    public byte[] ChrRom;

    public ArrayMemory PrgRomMemory;
    public ArrayMemory ChrRomMemory;

    public Cartridge(byte[] prgRom, byte[] chrRom) {
        PrgRom = prgRom;
        ChrRom = chrRom;

        PrgRomMemory = new ArrayMemory(PrgRom, PrgRom.Length - 0xFFFF - 1);
        ChrRomMemory = new ArrayMemory(ChrRom, ChrRom.Length);
    }
}
