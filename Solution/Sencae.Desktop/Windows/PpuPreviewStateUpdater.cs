using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sencae.Ppu;

namespace Sencae.Desktop;

public readonly unsafe ref struct PpuPreviewStateUpdater {
    public readonly PpuBusDevice PpuBusDevice;
    public readonly PpuPreviewState PreviewState;

    public PpuPreviewStateUpdater(PpuBusDevice ppuBusDevice, PpuPreviewState previewState) {
        PpuBusDevice = ppuBusDevice;
        PreviewState = previewState;
    }

    public void Update() {
        FillPatternTable(PpuConstants.PatternTable0.Start.Value, PreviewState.PatternTable0);
        FillPatternTable(PpuConstants.PatternTable1.Start.Value, PreviewState.PatternTable1);

        PreviewState.BackgroundPalettes[0] = FillPalette(0x3F01);
        PreviewState.BackgroundPalettes[1] = FillPalette(0x3F05);
        PreviewState.BackgroundPalettes[2] = FillPalette(0x3F09);
        PreviewState.BackgroundPalettes[3] = FillPalette(0x3F0D);

        PreviewState.SpritePalettes[0] = FillPalette(0x3F11);
        PreviewState.SpritePalettes[1] = FillPalette(0x3F15);
        PreviewState.SpritePalettes[2] = FillPalette(0x3F19);
        PreviewState.SpritePalettes[3] = FillPalette(0x3F1D);

        FillNametable(0, ref PreviewState.Nametable0, ref PreviewState.Nametable0Attributes);
        FillNametable(1, ref PreviewState.Nametable1, ref PreviewState.Nametable1Attributes);
    }

    private void FillNametableAttributes(
        in PpuNametable nametable,
        ref PpuPreviewState.PpuNametableAttributesDecoded attributes
    ) {
        fixed (byte* paletteIndicesPtr = attributes.PaletteIndices) {
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    int attributeIndex = y * 8 + x;
                    (byte bottomRight, byte bottomLeft, byte topRight, byte topLeft) =
                        nametable.AttributeTable.Decode(nametable.AttributeTable[attributeIndex]);

                    int tileBaseIndex = y * 8 * 4 * 4 + x * 4;

                    byte* row0 = paletteIndicesPtr + tileBaseIndex;
                    byte* row1 = row0 + PpuConstants.NametableWidthTiles;
                    byte* row2 = row1 + PpuConstants.NametableWidthTiles;
                    byte* row3 = row2 + PpuConstants.NametableWidthTiles;

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
    }

    private void FillNametable(
        int nametableIndex,
        ref PpuNametable nametable,
        ref PpuPreviewState.PpuNametableAttributesDecoded nametableAttributes
    ) {
        int baseAddress = PpuConstants.Nametable0StartAddress + sizeof(PpuNametable) * nametableIndex;

        Span<byte> nametableDataRaw = MemoryMarshal.CreateSpan(ref Unsafe.As<PpuNametable, byte>(ref nametable), PpuNametable.Size);
        for (ushort i = 0; i < PpuNametable.Size; i++) {
            nametableDataRaw[i] = PpuBusDevice.ReadObserve((ushort) (baseAddress + i));
        }

        nametable = Unsafe.As<byte, PpuNametable>(ref nametableDataRaw[0]);
        FillNametableAttributes(nametable, ref nametableAttributes);
    }

    private PpuObjectPalette FillPalette(ushort address) {
        byte bgColorIndex = PpuBusDevice.ReadObserve(0x3F00);

        PpuObjectPalette palette;
        
        palette.ColorIndices[0] = bgColorIndex;
        
        for (int i = 0; i < 3; i++) {
            byte colorIndex = PpuBusDevice.ReadObserve((ushort) (address + i));
            palette.ColorIndices[i + 1] = colorIndex;
        }

        return palette;
    }

    private void FillPatternTable(int vramOffset, PpuPreviewState.PpuPatternTableDecoded output) {
        for (int tileIndex = 0; tileIndex < PpuConstants.PatternTableTotalTiles; tileIndex++) {
            fixed (byte* paletteIndices = output.Tiles[tileIndex].PaletteColorIndices) {
                FillTileData(vramOffset, tileIndex, new Span<byte>(paletteIndices, PpuConstants.BytesPerDecodedTile));
            }
        }
    }

    private void FillTileData(int vramOffset, int tileIndex, Span<byte> tile64Bytes) {
        int baseAddress = vramOffset + tileIndex * PpuConstants.BytesPerTile;
        for (int tilePixelIndex = 0; tilePixelIndex < PpuConstants.BytesPerDecodedTile; tilePixelIndex++) {
            int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;

            int bit = 7 - tilePixelIndex % 8;
            ushort address = (ushort) (baseAddress + tilePixelRow);

            byte pixelPaletteValue = (byte) (
                ((PpuBusDevice.ReadObserve(address) >> bit) & 1) |
                (((PpuBusDevice.ReadObserve((ushort) (address + 8)) >> bit) & 1) << 1)
            );

            tile64Bytes[tilePixelIndex] = pixelPaletteValue;
        }
    }
}
