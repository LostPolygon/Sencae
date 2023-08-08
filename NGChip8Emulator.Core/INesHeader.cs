using System;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace NGChip8Emulator; 

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct INesHeader {
    public const uint ValidMagic = 0x1a53454e;
    
    public uint Magic;
    public byte PrgRomSize;
    public byte ChrRomSize;
    public Flags.Flags6 Flags6;
    public Flags.Flags7 Flags7;
    public byte PrgRamSize;
    public Flags.Flags9 Flags9;
    public Flags.Flags10 Flags10;

    public fixed byte Padding[5];

    public static class Flags {
        [Flags]
        public enum Flags6 : byte {
            VerticalMirroring = 0b_00000001, // 0: horizontal (vertical arrangement), 1: vertical (horizontal arrangement)
            NonVolatileMemoryPresent = 0b_00000010, // Cartridge contains battery-backed PRG RAM ($6000-7FFF) or other persistent memory
            Has512ByteTrainer = 0b_00000100, // 512-byte trainer at $7000-$71FF (stored before PRG data)
            FourScreenMode = 0b_00001000, // Ignore mirroring control or above mirroring bit; instead provide four-screen VRAM
            MapperNumberLowerNybble = 0b_11110000
        } 
    
        [Flags]
        public enum Flags7: byte {
            IsVSUnisystem = 0b_00000001,
            IsPlaychoice10 = 0b_00000010, //  (8 KB of Hint Screen data stored after CHR data)
            NES20FormatIdentifier =  0b_00001100, // if equal to 2, flags 8-15 are in NES 2.0 format
            MapperNumberUpperNybble = 0b_11110000
        }
    
        [Flags]
        public enum Flags9: byte {
            IsPalTVSystem = 1 << 0, // TV system (0: NTSC; 1: PAL)
            /* Bits 1-7: reserved, set to zero */
        } 
    
        [Flags]
        public enum Flags10: byte {
            /* Bits 1-7: reserved */
            TVSystem = 0b_00000011, // 0: NTSC; 2: PAL; 1/3: dual compatible
            /* Bits 1-7: reserved */
        } 
    }
}
