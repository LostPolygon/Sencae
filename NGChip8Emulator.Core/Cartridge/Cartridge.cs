namespace NGChip8Emulator; 

public class Cartridge {
    public byte[] PrgRom;
    public byte[] ChrRom;

    public ArrayMemory PrgRomMemory;
    public ArrayMemory ChrRomMemory;

    public Cartridge(byte[] prgRom, byte[] chrRom) {
        PrgRom = prgRom;
        ChrRom = chrRom;

        PrgRomMemory = new ArrayMemory(PrgRom, PrgRom.Length);
        ChrRomMemory = new ArrayMemory(ChrRom, ChrRom.Length);
    }
}
