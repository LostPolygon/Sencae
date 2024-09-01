using System.Runtime.InteropServices;
using Sencae.Nes20File;

namespace Sencae.Nes20Database;

public record GameRomMetadata(
    GenericRomMetadata PrgRom,
    GenericRomMetadata? ChrRom,
    GenericRomMetadata? Trainer,
    GenericRomMetadata Rom, 
    MiscRomMetadata? MiscRom,
    RamSizeMetadata? PrgRam,
    RamSizeMetadata? ChrRam,
    RamSizeMetadata? ChrNvram,
    RamSizeMetadata? PrgNvram,
    PcbMetadata Pcb,
    ConsoleMetadata Console,
    ExpansionMetadata Expansion,
    VsSystemMetadata? VsSystem
);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Sha1Hash {
    private const int Size = 20;
    private fixed byte _bytes[Size];

    public ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref _bytes[0], Size);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct GenericRomMetadata(
    int Size,
    Sha1Hash Sha1
);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct MiscRomMetadata(
    int Size,
    Sha1Hash Sha1,
    int Number
);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct PcbMetadata(
    ushort Mapper,
    ushort Submapper,
    MirroringType Mirroring,
    bool BatteryPresent
);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct RamSizeMetadata(
    int Size
);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct ConsoleMetadata(
    ExtendedConsoleType Type,
    Nes20Header.Flags.CpuPpuTiming Region
);

public readonly record struct VsSystemMetadata(
    byte Hardware,
    byte Ppu
);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct ExpansionMetadata(DefaultExpansionDevice DefaultExpansionDevice);

public enum MirroringType : byte {
    TwoScreenVerticalMirroring,
    TwoScreenHorizontalMirroring,
    FourScreen,
    OneScreenBlk0,
    OneScreenBlk1,
}
