namespace Sencae.Nes20Database.Binary;

public struct Header {
    public const int Version = 1;
    
    public static readonly byte[] Magic = "SENCAE"u8.ToArray();
}

[Flags]
public enum GameRomMetadataElementPresence : byte {
    ChrRom = 1 << 0,
    Trainer = 1 << 1,
    MiscRom = 1 << 2,
    PrgRam = 1 << 3,
    ChrRam = 1 << 4,
    ChrNvram = 1 << 5,
    PrgNvram = 1 << 6,
    VsSystem = 1 << 7,
}