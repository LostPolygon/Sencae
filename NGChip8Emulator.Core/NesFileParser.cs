using System;
using System.Runtime.InteropServices;

namespace NGChip8Emulator;

// https://www.nesdev.org/wiki/NES_2.0
public static unsafe class NesFileParser {
    public static NesFile Parse(byte[] data) {
        //BinaryReader reader = new(new MemoryStream(data));

        int position = 0;
        Span<byte> byteSpan = data.AsSpan();
        INesHeader header = MemoryMarshal.Cast<byte, INesHeader>(byteSpan)[0];
        if (header.Magic != INesHeader.ValidMagic)
            throw new Exception("Invalid NES 2.0 magic");
        
        position += sizeof(INesHeader);

        int prgRomSize = header.PrgRomSize * 16 * 1024;
        int chrRomSize = header.ChrRomSize * 8 * 1024;
        Span<byte> prgRom = byteSpan[position..(position + prgRomSize)];

        position += prgRomSize;
        Span<byte> chrRom = byteSpan[position..(position + chrRomSize)];

        return new NesFile(prgRom.ToArray(), chrRom.ToArray());
    }
}
