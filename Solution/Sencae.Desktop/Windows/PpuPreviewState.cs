using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sencae.Ppu;

namespace Sencae.Desktop;

public static class PpuPreviewState {
    public class PalettesData {
        public PpuObjectPalette[] Background = new PpuObjectPalette[4];
        public PpuObjectPalette[] Sprite = new PpuObjectPalette[4];
    }
    
    public class PatternTablesData {
        public PpuPatternTableTiles PatternTable0;
        public PpuPatternTableTiles PatternTable1;
        
        public Span<PpuPatternTableTiles> AsSpan() => MemoryMarshal.CreateSpan(ref PatternTable0, 2);
    }

    public struct NametableData {
        public PpuNametable Nametable;
        public PpuNametableAttributesPaletteIndices Attributes;
    }
    
    public class NametablesData {
        public NametableData Nametable0;
        public NametableData Nametable1;
        public NametableData Nametable2;
        public NametableData Nametable3;

        public Span<NametableData> AsSpan() => MemoryMarshal.CreateSpan(ref Nametable0, 4);
    }
    
    [InlineArray(Length)]
    public struct PpuNametableAttributesPaletteIndices {
        private byte _element0;

        public const int Length = PpuNametable.AttributeTableSize * 4 * 4;
    }

    [InlineArray(PpuConstants.BytesPerDecodedTile)]
    public struct PpuTilePaletteColorIndices {
        private byte _element0;
    }

    [InlineArray(PpuConstants.PatternTableTotalTiles)]
    public struct PpuPatternTableTiles {
        private PpuTilePaletteColorIndices _element0;
    }
}
