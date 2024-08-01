namespace Sencae.Nes20File;

public enum ExtendedConsoleType : byte {
    Regular = 0x00, // [Regular NES/Famicom/Dendy]
    NintendoVsSystem = 0x01, // [Nintendo Vs. System]
    Playchoice10 = 0x02, // [Playchoice 10]
    FamicloneWithDecimalModeSupport = 0x03, // Regular Famiclone, but with CPU that supports Decimal Mode
    RegularWithEpsmModule = 0x04, // Regular NES/Famicom with EPSM module or plug-through cartridge
    VrTechnologyVT01WithRedCyanStnPalette = 0x05, // V.R. Technology VT01 with red/cyan STN palette
    VrTechnologyVT02 = 0x06, // V.R. Technology VT02
    VrTechnologyVT03 = 0x07, // V.R. Technology VT03
    VrTechnologyVT09 = 0x08, // V.R. Technology VT09
    VrTechnologyVT32 = 0x09, // V.R. Technology VT32
    VrTechnologyVT369 = 0x0A, // V.R. Technology VT369
    UmcUM6578 = 0x0B, // UMC UM6578
    FamicomNetworkSystem = 0x0C, // Famicom Network System
    // $D-$F   reserved
}
