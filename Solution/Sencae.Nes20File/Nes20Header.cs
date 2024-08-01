using System;
using System.Runtime.InteropServices;

namespace Sencae.Nes20File;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Nes20Header {
    public const uint ValidMagic = 0x1a53454e;

    public uint Magic; // bytes 0..3
    public byte PrgRomSize; // byte 4
    public byte ChrRomSize; // byte 5
    public Flags.Flags6 Flags6; // byte 6
    public Flags.Flags7 Flags7; // byte 7
    public MapperNumberMsbAndSubmapperPacked MapperNumberMsbAndSubmapper; // byte 8
    public PrgRomAndChrRomSizeMsbPacked PrgRomAndChrRomSizeMsb; // byte 9
    public PrgRamAndEepromSizePacked PrgRamAndEepromSize; // byte 10
    public ChrRamSizePacked ChrRamSize; // byte 11
    public Flags.CpuPpuTiming CpuPpuTiming; // byte 12
    public Flags.VsSystemTypeOrExtendedConsoleType VsSystemTypeOrExtendedConsoleType; // byte 13
    public MiscellaneousRomsCountPacked MiscellaneousRomsCount; // byte 14
    public DefaultExpansionDevice DefaultExpansionDevice; // byte 15

    public enum MiscellaneousRomsCountPacked : byte {
        MiscellaneousRomsCount = 0b_00000011,
    }
    
    public enum ChrRamSizePacked : byte {
        ChrRamSizeShiftCount = 0b_00001111,
        ChrNvramSizeShiftCount = 0b_11110000
    }

    public enum PrgRamAndEepromSizePacked : byte {
        PrgRamShiftCount = 0b_00001111,
        PrgNvramOrEepromShiftCount = 0b_11110000
    }

    public enum PrgRomAndChrRomSizeMsbPacked : byte {
        PrgRomSizeMsb = 0b_00001111,
        ChrRomSizeMsb = 0b_11110000
    }

    public enum MapperNumberMsbAndSubmapperPacked : byte {
        MapperNumberMsbLowerNybble = 0b_00001111, // D11..D8
        SubmapperNumber = 0b_11110000
    }

    public static class Flags {
        [Flags]
        public enum VsSystemTypeOrExtendedConsoleType : byte {
            // When Byte 7 AND 3 =1: Vs. System Type
            VsPpuType = 0b_00001111,
            VsHardwareType = 0b_11110000,
            
            // When Byte 7 AND 3 =3: Extended Console Type
            ExtendedConsoleType = 0b_00001111,
        }

        [Flags]
        public enum CpuPpuTiming : byte {
            Ntsc = 0, // RP2C02 ("NTSC NES")
            LicensedPal = 1, // RP2C07 ("Licensed PAL NES")
            MultiRegion = 2, // Multiple-region
            Dendy = 3 // UA6538 ("Dendy")
        }

        [Flags]
        public enum Flags6 : byte {
            VerticalMirroring = 0b_00000001, // 0: horizontal (vertical arrangement), 1: vertical (horizontal arrangement)
            NonVolatileMemoryPresent = 0b_00000010, // Cartridge contains battery-backed PRG RAM ($6000-7FFF) or other persistent memory
            Has512ByteTrainer = 0b_00000100, // 512-byte trainer at $7000-$71FF (stored before PRG data)
            AlternativeNametables = 0b_00001000, // Ignore mirroring control or above mirroring bit; instead provide four-screen VRAM
            MapperNumberLsbLowerNybble = 0b_11110000 // D3..D0
        }

        [Flags]
        public enum Flags7 : byte {
            ConsoleType = 0b_00000011,
            Nes20FormatIdentifier = 0b_00001100, // if equal to 2, flags 8-15 are in NES 2.0 format
            MapperNumberLsbUpperNybble = 0b_11110000 // D7..D4
        }
    }
}
