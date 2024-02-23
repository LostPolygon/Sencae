namespace Sencae;

public sealed record CartridgeData(
    int Mapper,
    byte[] PrgRom,
    byte[] ChrRom,
    bool IsChrRomWriteable
);
