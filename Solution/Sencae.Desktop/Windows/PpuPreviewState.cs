using Sencae.Ppu;

namespace Sencae.Desktop;

public unsafe class PpuPreviewState {
    public PpuPatternTableDecoded PatternTable0 = new();
    public PpuPatternTableDecoded PatternTable1 = new();

    public PpuObjectPalette[] BackgroundPalettes = new PpuObjectPalette[4];
    public PpuObjectPalette[] SpritePalettes = new PpuObjectPalette[4];

    public PpuNametable Nametable0 = default;
    public PpuNametableAttributesDecoded Nametable0Attributes = default;
    public PpuNametable Nametable1 = default;
    public PpuNametableAttributesDecoded Nametable1Attributes = default;

    public struct PpuNametableAttributesDecoded {
        public fixed byte PaletteIndices[PpuNametable.AttributeTableSize * 4 * 4];
    }

    public struct PpuTileDecoded {
        public fixed byte PaletteColorIndices[PpuConstants.BytesPerDecodedTile];
    }

    public class PpuPatternTableDecoded {
        public PpuTileDecoded[] Tiles = new PpuTileDecoded[PpuConstants.PatternTableTotalTiles];
    }
}
