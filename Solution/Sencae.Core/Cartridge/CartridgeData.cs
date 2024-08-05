using Sencae.Ppu;

namespace Sencae;

public sealed record CartridgeData(
    int Mapper,
    byte[] PrgRom,
    byte[]? ChrRom,
    byte[]? ChrRam,
    PpuNametableMirroringType PpuNametableMirroringType
);
