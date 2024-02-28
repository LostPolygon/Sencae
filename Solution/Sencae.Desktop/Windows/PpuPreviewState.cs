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
        public ReadOnlySpan<PpuPatternTableTiles> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in PatternTable0, 2);
    }
    
    public class NametablesData {
        public NametableData Nametable0;
        public NametableData Nametable1;
        public NametableData Nametable2;
        public NametableData Nametable3;

        public Span<NametableData> AsSpan() => MemoryMarshal.CreateSpan(ref Nametable0, 4);
        public ReadOnlySpan<NametableData> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in Nametable0, 4);
    }
    
    public struct NametableData {
        public PpuNametable Nametable;
        public PpuNametableAttributesPaletteIndices Attributes;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = Length)]
    public struct PpuNametableAttributesPaletteIndices {
        private byte _element0;

        public const int Length = PpuNametable.AttributeTableSize * 4 * 4;
        
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element0, Length).AsBytes();
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in _element0, Length).AsBytes();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = Length)]
    public struct PpuTilePaletteColorIndices {
        private byte _element0;

        public const int Length = PpuConstants.BytesPerDecodedTile;
        
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element0, Length).AsBytes();
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in _element0, Length).AsBytes();
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = ByteLength)]
    public struct PpuPatternTableTiles {
        private PpuTilePaletteColorIndices _element0;
        
        public const int Length = PpuConstants.PatternTableTotalTiles;
        public const int ByteLength = PpuTilePaletteColorIndices.Length * PpuConstants.PatternTableTotalTiles;
        
        public Span<PpuTilePaletteColorIndices> AsSpan() => MemoryMarshal.CreateSpan(ref _element0, Length);
        public readonly ReadOnlySpan<PpuTilePaletteColorIndices> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in _element0, Length);
    }
}
