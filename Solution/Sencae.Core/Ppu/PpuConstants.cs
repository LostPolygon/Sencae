using System;

namespace Sencae.Ppu;

public static class PpuConstants {
    public const int TileSize = 8;
    public const int BytesPerTile = 16;
    public const int BitsPerTilePixel = 2;
    public const int BytesPerDecodedTile = TileSize * TileSize * sizeof(byte);
    public const int PatternTableWidthTiles = 16;
    public const int PatternTableHeightTiles = 16;
    public const int PatternTableTotalTiles = PatternTableWidthTiles * PatternTableHeightTiles;
    public const int PatternTableWidth = PatternTableWidthTiles * TileSize;
    public const int PatternTableHeight = PatternTableHeightTiles * TileSize;
    public const int PatternTableSize = 0x1000;
    public const int PatternTableBitsPerPixel = 2;
    public const int PatternTablePixelCount = PatternTableSize * 8 / PatternTableBitsPerPixel;

    public const int Nametable0StartAddress = 0x2000;

    public const int NametableWidthTiles = 32;
    public const int NametableHeightTiles = 30; // +0.5
    
    public const int NametableWidthPixels = NametableWidthTiles * TileSize;
    public const int NametableHeightPixels = 30 * TileSize + TileSize / 2;
    
    public static readonly Range PatternTable0 = new(0x0000, 0x1000);
    public static readonly Range PatternTable1 = new(0x1000, 0x2000);

    public static class RegisterAddresses {
        public const ushort FirstRegisterAddress = PPUCTRL;
        
        public const ushort PPUCTRL = 0x2000;
        public const ushort PPUMASK = 0x2001;
        public const ushort PPUSTATUS = 0x2002;
        public const ushort OAMADDR = 0x2003;
        public const ushort OAMDATA = 0x2004;
        public const ushort PPUSCROLL = 0x2005;
        public const ushort PPUADDR = 0x2006;
        public const ushort PPUDATA = 0x2007;
    }
}
