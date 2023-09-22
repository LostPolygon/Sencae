using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Perfolizer.Horology;
using Sencae.Ppu;

namespace Sencae.Desktop;

public unsafe class PpuTileViewerWindow : AppWindow, IDisposable {
    private readonly DynamicSdlTexture _patternTable1Texture;
    private readonly DynamicSdlTexture _patternTable2Texture;

    private bool _drawAttributeGrid;
    private PpuPreviewState _previewState = new PpuPreviewState();

    public Emulator Emulator { get; }

    public class PpuPreviewState {
        public PpuPatternTableDecoded PatternTable0;
        public PpuPatternTableDecoded PatternTable1;
        
        public PpuObjectPalette[] BackgroundPalettes = new PpuObjectPalette[4];
        public PpuObjectPalette[] SpritePalettes = new PpuObjectPalette[4];

        public PpuNametable Nametable0;
        public PpuNametable Nametable1;

        public struct PpuTileDecoded {
            public fixed byte PaletteIndices[64];
        }
        
        public class PpuPatternTableDecoded {
            public PpuTileDecoded[] Tiles = new PpuTileDecoded[PpuConstants.PatternTableTotalTiles];
        }
    }

    public PpuTileViewerWindow(
        AppWindowContext context,
        Emulator emulator
    ) : base(
        nameof(PpuTileViewerWindow),
        "PPU Tile Viewer",
        context
    ) {
        Emulator = emulator;

        _patternTable1Texture = new DynamicSdlTexture(
            Context.Renderer,
            PpuConstants.PatternTableWidth,
            PpuConstants.PatternTableHeight,
            SDL_PIXELFORMAT_ARGB8888
        );
        _patternTable2Texture = new DynamicSdlTexture(
            Context.Renderer,
            PpuConstants.PatternTableWidth,
            PpuConstants.PatternTableHeight,
            SDL_PIXELFORMAT_ARGB8888
        );
    }

    public void Dispose() {
        _patternTable1Texture.Dispose();
        _patternTable2Texture.Dispose();
    }

    protected override void DrawContent(float deltaTime) {
        ImGui.SetWindowSize(new Vector2(270, 280), ImGuiCond.FirstUseEver);

        if (!Emulator.IsLoaded)
            return;

        {
            using var _ = Profiler.Begin(nameof(PpuTileViewerWindow) + "_Preview", TimeUnit.Millisecond);

            using DynamicSdlTexture.LockedSurface pattern1TableSurface = _patternTable1Texture.Lock();
            FillPatternTableTextureFromVram(PpuConstants.PatternTable1.Start.Value, pattern1TableSurface.Surface);

            using DynamicSdlTexture.LockedSurface pattern2TableSurface = _patternTable2Texture.Lock();
            FillPatternTableTextureFromVram(PpuConstants.PatternTable2.Start.Value, pattern2TableSurface.Surface);
        }

        DrawPatternTable(_patternTable1Texture, "Pattern Table 1");
        ImGui.SameLine();
        DrawPatternTable(_patternTable2Texture, "Pattern Table 2");

        ImGui.Separator();

        ImGui.Checkbox("Draw Attribute Grid", ref _drawAttributeGrid);
        
        DrawNametable(0);
        ImGui.SameLine();
        DrawNametable(1);

        ImGui.Separator();

        const float paletteSpacing = 130;

        DrawPalette(0x3F01, "BG Palette 0");
        ImGui.SameLine(paletteSpacing * 1);
        DrawPalette(0x3F05, "BG Palette 1");
        ImGui.SameLine(paletteSpacing * 2);
        DrawPalette(0x3F09, "BG Palette 2");
        ImGui.SameLine(paletteSpacing * 3);
        DrawPalette(0x3F0D, "BG Palette 3");

        ImGui.Separator();

        DrawPalette(0x3F11, "SPR Palette 0");
        ImGui.SameLine(paletteSpacing * 1);
        DrawPalette(0x3F15, "SPR Palette 1");
        ImGui.SameLine(paletteSpacing * 2);
        DrawPalette(0x3F19, "SPR Palette 2");
        ImGui.SameLine(paletteSpacing * 3);
        DrawPalette(0x3F1D, "SPR Palette 3");

        ImGui.Separator();
    }

    private void DrawPalette(ushort baseAddress, string title) {
        Vector2 colorTileSize = new(24, 16);
        float colorIndexHexFontSize = MathF.Min(colorTileSize.X, colorTileSize.Y) * 0.75f;

        Vector2 previewSize = new Vector2(4 * colorTileSize.X, colorTileSize.Y);

        ImGui.BeginGroup();
        ImGui.Text(title);

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 cursorPos = ImGui.GetCursorPos();
        ImGui.Dummy(Vector2.One * previewSize);
        Vector2 imageMin = ImGui.GetItemRectMin();
        Vector2 imageMax = ImGui.GetItemRectMax();

        // background
        drawList.AddRectFilled(imageMin, imageMax, 0xFF000000);

        ImGui.SetCursorPos(cursorPos);

        PpuBusDevice ppuBusDevice = Emulator.Machine.PpuBusDevice;

        static byte TransformTileIndex(byte index) =>
            index switch {
                3 => 0,
                0 => 1,
                1 => 2,
                2 => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };
        
        static byte InverseTransformTileIndex(byte index) =>
            index switch {
                0 => 3,
                1 => 0,
                2 => 1,
                3 => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };

        ImFontPtr font = Context.SharedResources.FontMonospacedNormal;
        ImGui.PushFont(font);
        for (int i = 0; i < 4; i++) {
            Vector2 tileMin = imageMin + Vector2.UnitX * colorTileSize * TransformTileIndex((byte) i);
            byte colorIndex = (byte) (ppuBusDevice.ReadObserve((ushort) (baseAddress + i)) % 64);

            Color color = Color.FromArgb((int) Context.SharedResources.PpuPalette.Colors[colorIndex]);

            drawList.AddRectFilled(
                tileMin,
                tileMin + colorTileSize,
                (uint) color.ToArgb()
            );

            string colorIndexHex = colorIndex.ToString("X2");

            Vector2 textSize = ImGui.CalcTextSize(colorIndexHex) * colorIndexHexFontSize / font.FontSize;
            Vector2 textPos = tileMin + 0.5f * colorTileSize - new Vector2(0.45f, 0.5f) * textSize;

            Color textColor = Color.White;
            if (ColorUtility.CalculateWcagContrast(textColor, color) < 4.5f) {
                textColor = Color.Black;
            }

            drawList.AddText(
                font.NativePtr,
                colorIndexHexFontSize,
                textPos,
                (uint) textColor.ToArgb(),
                colorIndexHex
            );
            
            /*drawList.AddRectFilled(
                textPos,
                textPos + textSize,
                0xaa0000FF
            );*/
        }

        ImGui.PopFont();

        TileTooltip(imageMin, colorTileSize, 0xFFFF0000, (x, _) => {
            x = InverseTransformTileIndex((byte) x);
            byte colorIndex = ppuBusDevice.ReadObserve((ushort) (baseAddress + x));
            string colorIndexHex = colorIndex.ToString("X2");
            ImGui.Text(colorIndexHex);
        });

        ImGui.EndGroup();
    }

    private void DrawNametable(int nametableIndex) {
        ImGui.Text($"Nametable {nametableIndex}");

        const float tileSize = 12;

        PpuBusDevice ppuBusDevice = Emulator.Machine.PpuBusDevice;

        Vector2 nametablePreviewSize = new Vector2(32 * tileSize, 30.5f * tileSize);
        Vector2 cursorPos = ImGui.GetCursorPos();
        ImGui.Dummy(nametablePreviewSize);

        Vector2 nametableRectMin = ImGui.GetItemRectMin();
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        int baseAddress = 0x2000 + sizeof(PpuNametable) * nametableIndex;
        Span<byte> nametableDataRaw = stackalloc byte[PpuNametable.Size];
        for (ushort i = 0; i < PpuNametable.Size; i++) {
            nametableDataRaw[i] = ppuBusDevice.ReadObserve((ushort) (baseAddress + i));
        }

        ref PpuNametable nametableData = ref MemoryMarshal.AsRef<PpuNametable>(nametableDataRaw);
        for (int y = 0; y < 30; y++) {
            for (int x = 0; x < 32; x++) {
                Vector2 cellMin = nametableRectMin + new Vector2(x, y) * tileSize;
                Vector2 cellMax = cellMin + Vector2.One * tileSize;

                if (!ImGui.IsRectVisible(cellMin, cellMax))
                    continue;

                byte patternTableTileIndex = nametableData.Rows[y][x];

                int tileRow = patternTableTileIndex / 16;
                int tileColumn = patternTableTileIndex % 16;

                const float uvInset = 0.001f;
                Vector2 uv0 = new Vector2(tileColumn, tileRow) * (1f / 16f) + Vector2.One * uvInset;
                Vector2 uv1 = uv0 + Vector2.One * (1f / 16f - uvInset);

                IntPtr patternTableTexture =
                    (Emulator.Machine.State.Ppu.PPUCTRL & PpuControlRegisterFlags.BackgroundPatternTableAddress) == 0 ?
                        _patternTable1Texture.Texture :
                        _patternTable2Texture.Texture;
                drawList.AddImage(
                    patternTableTexture,
                    cellMin,
                    cellMax,
                    uv0,
                    uv1
                );

                //drawList.AddRectFilled(cellMin, cellMax, 0xFF_00_00_00 | value | ((uint) value << 8) | ((uint) value << 16));
            }
        }

        if (_drawAttributeGrid) {
            DrawAttributesGrid(nametableData, tileSize, nametableRectMin);
        }

        ImGui.SetCursorPos(cursorPos);
        if (ImGui.BeginChild($"##Nametable_{nametableIndex}", nametablePreviewSize, true)) {
            ImGui.EndChild();
        }
    }

    private void DrawAttributesGrid(
        in PpuNametable nametableData,
        float tileSize,
        Vector2 nametableRectMin
    ) {
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        
        float attributeTileSize = tileSize * 4;
        float attributeSubTileSize = tileSize * 2;

        for (int y = 0; y < 8; y++) {
            for (int x = 0; x < 8; x++) {
                int attributeIndex = y * 8 + x;
                (byte bottomRight, byte bottomLeft, byte topRight, byte topLeft) =
                    nametableData.AttributeTable.Decode(nametableData.AttributeTable[attributeIndex]);

                void DrawAttributeTile2x2(byte value, Vector2 tileMin) {
                    Span<int> bitPaletteArgb = stackalloc int[4] {
                        Color.FromArgb(100, 0, 0, 0).ToArgb(),
                        Color.FromArgb(100, 85, 85, 85).ToArgb(),
                        Color.FromArgb(100, 170, 170, 170).ToArgb(),
                        Color.FromArgb(100, 255, 255, 255).ToArgb(),
                    };

                    drawList.AddRectFilled(
                        tileMin,
                        tileMin + Vector2.One * attributeSubTileSize,
                        (uint) bitPaletteArgb[value]
                    );
                    drawList.AddText(
                        Context.SharedResources.FontMonospacedNormal.NativePtr,
                        12,
                        tileMin + new Vector2(0.5f, 0.25f) * attributeSubTileSize,
                        0xFF0000FF,
                        value.ToString()
                    );
                }

                Vector2 topLeftTileMin = nametableRectMin + new Vector2(x, y) * attributeTileSize;
                DrawAttributeTile2x2(topLeft, topLeftTileMin);
                DrawAttributeTile2x2(topRight, topLeftTileMin + Vector2.UnitX * attributeSubTileSize);

                if (y != 7) {
                    DrawAttributeTile2x2(bottomLeft, topLeftTileMin + Vector2.UnitY * attributeSubTileSize);
                    DrawAttributeTile2x2(bottomRight, topLeftTileMin + Vector2.One * attributeSubTileSize);
                }

                //drawList.AddRectFilled(cellMin, cellMax, 0xFF_00_00_00 | value | ((uint) value << 8) | ((uint) value << 16));
            }
        }
    }

    private void DrawPatternTable(DynamicSdlTexture patternTableTexture, string title) {
        const float previewScale = 2;
        const float previewTileSize = PpuConstants.TileSize * previewScale;
        Vector2 previewSize = new Vector2(patternTableTexture.Width, patternTableTexture.Height) * previewScale;

        ImGui.BeginGroup();
        ImGui.Text(title);

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 cursorPos = ImGui.GetCursorPos();
        ImGui.Dummy(Vector2.One * previewSize);
        Vector2 imageMin = ImGui.GetItemRectMin();
        Vector2 imageMax = ImGui.GetItemRectMax();

        // background
        drawList.AddRectFilled(imageMin, imageMax, 0xFFFF00FF);

        ImGui.SetCursorPos(cursorPos);
        ImGui.Image(patternTableTexture.Texture, previewSize);

        TileTooltip(imageMin, Vector2.One * previewTileSize, 0xFFFF0000, null);

        ImGui.EndGroup();
    }

    private void TileTooltip(Vector2 rectMin, Vector2 tileSize, uint frameColor, Action<int, int>? drawContents) {
        if (!ImGui.IsItemHovered())
            return;

        Vector2 mousePos = ImGui.GetMousePos();

        Vector2 activeTileMin = mousePos - rectMin;
        activeTileMin.X = (int) (activeTileMin.X / tileSize.X);
        activeTileMin.Y = (int) (activeTileMin.Y / tileSize.Y);

        int tileX = (int) activeTileMin.X;
        int tileY = (int) activeTileMin.Y;

        activeTileMin *= tileSize;
        activeTileMin += rectMin;
        Vector2 activeTileMax = activeTileMin + tileSize;

        ImDrawListPtr drawListPtr = ImGui.GetWindowDrawList();
        drawListPtr.AddRect(activeTileMin, activeTileMax, frameColor, 0, ImDrawFlags.None, 2);

        ImGui.SetNextWindowPos(activeTileMin + tileSize with { X = tileSize.X * 2 });
        ImGui.BeginTooltip();

        drawContents ??= (x, y) => ImGui.Text($"{x}, {y}");
        drawContents(tileX, tileY);

        GuiVisualUtility.DrawCurrentWindowDropShadow(Context.SharedResources.ShadowTexture, 25, 150, 2.75f);
        ImGui.EndTooltip();
    }

    private void FillTileData(int vramOffset, int tileIndex, Span<byte> tile64Bytes) {
        PpuBusDevice ppuBusDevice = Emulator.Machine.PpuBusDevice;

        const int tilePixelCount = PpuConstants.TileSize * PpuConstants.TileSize;

        for (int tilePixelIndex = 0; tilePixelIndex < PpuConstants.BytesPerDecodedTile; tilePixelIndex++) {
            int tileRow = tileIndex / PpuConstants.BytesPerTile;
            int tileColumn = tileIndex % PpuConstants.BytesPerTile;

            //int tilePixelIndex = pixelIndex - tileIndex * tilePixelCount;

            int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;
            int tilePixelColumn = tilePixelIndex % PpuConstants.TileSize;
            
            int surfacePixelRow = tileRow * PpuConstants.TileSize + tilePixelRow;
            int surfacePixelColumn = tileColumn * PpuConstants.TileSize + tilePixelColumn;

            int bit = 7 - tilePixelIndex % 8;
            ushort address = (ushort) (vramOffset + tileIndex * PpuConstants.BytesPerTile + tilePixelRow);

            byte pixelPaletteValue = (byte) (
                ((ppuBusDevice.ReadObserve(address) >> bit) & 1) |
                (((ppuBusDevice.ReadObserve((ushort) (address + 8)) >> bit) & 1) << 1)
            );

            tile64Bytes[tilePixelIndex] = pixelPaletteValue;
        }
    }

    private void FillPatternTableTextureFromVram(int vramOffset, SDL_Surface* surface) {
        Span<int> bitPaletteArgb = stackalloc int[4] {
            Color.FromArgb(255, 0, 0, 0).ToArgb(),
            Color.FromArgb(255, 85, 85, 85).ToArgb(),
            Color.FromArgb(255, 170, 170, 170).ToArgb(),
            Color.FromArgb(255, 255, 255, 255).ToArgb(),
        };

        PpuBusDevice ppuBusDevice = Emulator.Machine.PpuBusDevice;
        const int totalTiles = PpuConstants.PatternTableWidthTiles * PpuConstants.PatternTableHeightTiles;

        Span<byte> tileData = stackalloc byte[PpuConstants.BytesPerDecodedTile];
        
        for (int tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
            int tileRow = tileIndex / PpuConstants.BytesPerTile;
            int tileColumn = tileIndex % PpuConstants.BytesPerTile;
            
            FillTileData(vramOffset, tileIndex, tileData);

            for (int tilePixelIndex = 0; tilePixelIndex < PpuConstants.BytesPerDecodedTile; tilePixelIndex++) {
                int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;
                int tilePixelColumn = tilePixelIndex % PpuConstants.TileSize;

                int surfacePixelRow = tileRow * PpuConstants.TileSize + tilePixelRow;
                int surfacePixelColumn = tileColumn * PpuConstants.TileSize + tilePixelColumn;

                int* surfacePixels = (int*) surface->pixels;
                int surfacePixelIndex = surfacePixelRow * PpuConstants.PatternTableWidth + surfacePixelColumn;
                surfacePixels[surfacePixelIndex] = bitPaletteArgb[tileData[tilePixelIndex]];
            }
        }
    }
}
