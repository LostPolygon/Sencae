using System.Runtime.InteropServices;
using Sencae.Ppu;

namespace Sencae.Desktop;

public readonly unsafe ref struct PpuPreviewStateUpdater(PpuMemoryBus ppuMemoryBus) {
    public readonly PpuMemoryBus PpuMemoryBus = ppuMemoryBus;

    public void FillPalettes(PpuPreviewState.PalettesData palettes) {
        palettes.Background[0] = FillPalette(0x3F01);
        palettes.Background[1] = FillPalette(0x3F05);
        palettes.Background[2] = FillPalette(0x3F09);
        palettes.Background[3] = FillPalette(0x3F0D);

        palettes.Sprite[0] = FillPalette(0x3F11);
        palettes.Sprite[1] = FillPalette(0x3F15);
        palettes.Sprite[2] = FillPalette(0x3F19);
        palettes.Sprite[3] = FillPalette(0x3F1D);
    }

    public void FillPatternTables(PpuPreviewState.PatternTablesData patternTables) {
        FillPatternTable(PpuConstants.PatternTable0.Start.Value, ref patternTables.PatternTable0);
        FillPatternTable(PpuConstants.PatternTable1.Start.Value, ref patternTables.PatternTable1);
    }

    public void FillNametables(PpuPreviewState.NametablesData nametables) {
        Span<PpuPreviewState.NametableData> nametablesSpan = nametables.AsSpan();
        for (int i = 0; i < nametablesSpan.Length; i++) {
            FillNametable(i, ref nametablesSpan[i].Nametable, ref nametablesSpan[i].Attributes);
        }
    }
    
    private static void FillNametableAttributes(
        ref PpuNametable nametable,
        ref PpuPreviewState.PpuNametableAttributesPaletteIndices attributesPaletteIndices
    ) {
        Span<byte> paletteIndicesSpan = attributesPaletteIndices.AsSpan();
        ReadOnlySpan<PpuAttributeTableEntryFlags> attributeTable = nametable.AttributeTable.AsSpan();
        for (int y = 0; y < 8; y++) {
            for (int x = 0; x < 8; x++) {
                int attributeIndex = y * 8 + x;
                (byte bottomRight, byte bottomLeft, byte topRight, byte topLeft) =
                    PpuNametable.PpuAttributeTable.Decode(attributeTable[attributeIndex]);

                int tileBaseIndex = y * 8 * 4 * 4 + x * 4;

                Range CreateRowRange(int index) {
                    int rangeStart = tileBaseIndex + PpuConstants.NametableWidthTiles * index;
                    return new(rangeStart, rangeStart + 4);
                }

                Span<byte> row0 = paletteIndicesSpan[CreateRowRange(0)];
                Span<byte> row1 = paletteIndicesSpan[CreateRowRange(1)];
                Span<byte> row2 = paletteIndicesSpan[CreateRowRange(2)];
                Span<byte> row3 = paletteIndicesSpan[CreateRowRange(3)];

                /*
                 00 01 02 03
                 04 05 06 07
                 08 09 10 11
                 12 13 14 15
                 */

                row0[0] = row0[1] = row1[0] = row1[1] = topLeft;
                row0[2] = row0[3] = row1[2] = row1[3] = topRight;

                row2[0] = row2[1] = row3[0] = row3[1] = bottomLeft;
                row2[2] = row2[3] = row3[2] = row3[3] = bottomRight;
            }
        }
    }

    private void FillNametable(
        int nametableIndex,
        ref PpuNametable nametable,
        ref PpuPreviewState.PpuNametableAttributesPaletteIndices nametableAttributes
    ) {
        int baseAddress = PpuConstants.Nametable0StartAddress + sizeof(PpuNametable) * nametableIndex;

        Span<byte> nametableDataRaw = MemoryMarshal.CreateSpan(ref nametable, 1).AsBytes();
        for (ushort i = 0; i < PpuNametable.Size; i++) {
            nametableDataRaw[i] = PpuMemoryBus.ReadObserve((ushort) (baseAddress + i));
        }

        FillNametableAttributes(ref nametable, ref nametableAttributes);
    }

    private PpuObjectPalette FillPalette(ushort address) {
        PpuObjectPalette palette;

        byte bgColorIndex = PpuMemoryBus.ReadObserve(0x3F00);
        palette.ColorIndices[0] = bgColorIndex;
        for (int i = 0; i < 3; i++) {
            byte colorIndex = PpuMemoryBus.ReadObserve((ushort) (address + i));
            palette.ColorIndices[i + 1] = colorIndex;
        }

        return palette;
    }

    private void FillPatternTable(int vramOffset, ref PpuPreviewState.PpuPatternTableTiles output) {
        Span<PpuPreviewState.PpuTilePaletteColorIndices> outputSpan = output.AsSpan();
        for (int tileIndex = 0; tileIndex < outputSpan.Length; tileIndex++) {
            FillTileData(vramOffset, tileIndex, outputSpan[tileIndex].AsSpan());
        }
    }

    private void FillTileData(int vramOffset, int tileIndex, Span<byte> tile64Bytes) {
        int baseAddress = vramOffset + tileIndex * PpuConstants.BytesPerTile;
        for (int tilePixelIndex = 0; tilePixelIndex < PpuConstants.BytesPerDecodedTile; tilePixelIndex++) {
            int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;

            int bit = ~tilePixelIndex & (8 - 1); // 0..7 range
            ushort address = (ushort) (baseAddress + tilePixelRow);

            byte pixelPaletteValue = (byte) (
                ((PpuMemoryBus.ReadObserve(address) >> bit) & 1) |
                (((PpuMemoryBus.ReadObserve((ushort) (address + 8)) >> bit) & 1) << 1)
            );

            tile64Bytes[tilePixelIndex] = pixelPaletteValue;
        }
    }
}
