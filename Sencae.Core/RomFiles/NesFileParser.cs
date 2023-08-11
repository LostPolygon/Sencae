using System;
using System.Runtime.InteropServices;

namespace Sencae;

// https://www.nesdev.org/wiki/NES_2.0
public static unsafe class NesFileParser {
    public static CartridgeData Parse(byte[] data) {
        //BinaryReader reader = new(new MemoryStream(data));

        int position = 0;
        Span<byte> byteSpan = data.AsSpan();
        Nes20Header header = MemoryMarshal.Cast<byte, Nes20Header>(byteSpan)[0];
        if (header.Magic != Nes20Header.ValidMagic)
            throw new Exception("Invalid NES 2.0 magic");
        
        position += sizeof(Nes20Header);

        int mapper = 
            (int) (header.Flags6 & Nes20Header.Flags.Flags6.MapperNumberLowerNybble) >> 4 |
            (int) (header.Flags7 & Nes20Header.Flags.Flags7.MapperNumberUpperNybble);

        int prgRomSize = header.PrgRomSize * 16 * 1024;
        int chrRomSize = header.ChrRomSize * 8 * 1024;
        Span<byte> prgRom = byteSpan[position..(position + prgRomSize)];

        position += prgRomSize;
        Span<byte> chrRom = byteSpan[position..(position + chrRomSize)];

        return new CartridgeData(prgRom.ToArray(), chrRom.ToArray());
    }
}
