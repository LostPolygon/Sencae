using Sencae.Nes20File;

namespace Sencae.Nes20Database;

public record GameRomMetadata(
    
    );

public record GenericRomMetadata(
    int Size,
    int Crc32,
    byte[] Sha1
);

public record PrgRomAndChrRomMetadata(
    int Size,
    int Crc32,
    byte[] Sha1,
    ushort Sum16
) : GenericRomMetadata(Size, Crc32, Sha1);

public record struct PcbMetadata(
    int Mapper,
    int Submapper,
    MirroringType Mirroring,
    bool BatteryPresent
);

public record struct RamSizeMetadata(
    int Size
);

public record struct ConsoleMetadata(
    ExtendedConsoleType Type,
    Nes20Header.Flags.CpuPpuTiming Region
);

public record struct ExpansionMetadata(DefaultExpansionDevice Type);

public enum MirroringType {
    TwoScreenVerticalMirroring,
    TwoScreenHorizontalMirroring,
    FourScreen,
    OneScreenBlk0,
    OneScreenBlk1,
}