namespace Sencae;

public record CartridgeData(
    int Mapper,
    byte[] PrgRom,
    byte[] ChrRom,
    bool IsChrRomWriteable
);
