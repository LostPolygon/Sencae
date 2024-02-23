using System.Drawing;
using System.Numerics;
using ImGuiNET;
using Perfolizer.Horology;
using Sencae.Ppu;

namespace Sencae.Desktop;

public unsafe class PpuTileViewerWindow : AppWindow, IDisposable {
    private readonly DynamicSdlTexture _patternTable0Texture;
    private readonly DynamicSdlTexture _patternTable1Texture;
    private readonly DynamicSdlTexture _nametable0Texture;
    private readonly DynamicSdlTexture _nametable1Texture;

    private bool _drawAttributeGrid;
    private PpuPreviewState _previewState = new PpuPreviewState();

    public Emulator Emulator { get; }

    public PpuTileViewerWindow(
        AppWindowContext context,
        Emulator emulator
    ) : base(
        nameof(PpuTileViewerWindow),
        "PPU Tile Viewer",
        context
    ) {
        Emulator = emulator;

        _patternTable0Texture = new DynamicSdlTexture(
            Context.Renderer,
            PpuConstants.PatternTableWidth,
            PpuConstants.PatternTableHeight,
            SDL_PIXELFORMAT_ARGB8888
        );
        _patternTable1Texture = new DynamicSdlTexture(
            Context.Renderer,
            PpuConstants.PatternTableWidth,
            PpuConstants.PatternTableHeight,
            SDL_PIXELFORMAT_ARGB8888
        );
        _nametable0Texture = new DynamicSdlTexture(
            Context.Renderer,
            PpuConstants.NametableWidthPixels,
            PpuConstants.NametableHeightPixels,
            SDL_PIXELFORMAT_ARGB8888
        );
        _nametable1Texture = new DynamicSdlTexture(
            Context.Renderer,
            PpuConstants.NametableWidthPixels,
            PpuConstants.NametableHeightPixels,
            SDL_PIXELFORMAT_ARGB8888
        );
    }

    public void Dispose() {
        _patternTable0Texture.Dispose();
        _patternTable1Texture.Dispose();
        _nametable0Texture.Dispose();
        _nametable1Texture.Dispose();
    }

    protected override void DrawContent(float deltaTime) {
        ImGui.SetWindowSize(new Vector2(270, 280), ImGuiCond.FirstUseEver);

        if (!Emulator.IsLoaded)
            return;

        {
            using (Profiler.Begin(nameof(PpuTileViewerWindow) + "_UpdateState", TimeUnit.Millisecond)) {
                new PpuPreviewStateUpdater(Emulator.Machine.PpuBusDevice, _previewState).Update();
            }

            using (Profiler.Begin(nameof(PpuTileViewerWindow) + "_UpdateTextures", TimeUnit.Millisecond)) {
                Span<uint> patternTablePalette = stackalloc uint[4];
                GetGreyscalePalette(255, patternTablePalette);

                using DynamicSdlTexture.LockedSurface pattern0TableSurface = _patternTable0Texture.Lock();
                UpdatePatternTableTexture(_previewState.PatternTable0, patternTablePalette, pattern0TableSurface.Surface);

                using DynamicSdlTexture.LockedSurface pattern1TableSurface = _patternTable1Texture.Lock();
                UpdatePatternTableTexture(_previewState.PatternTable1, patternTablePalette, pattern1TableSurface.Surface);

                using DynamicSdlTexture.LockedSurface nametable0Surface = _nametable0Texture.Lock();
                UpdateNametableTexture(_previewState, _previewState.Nametable0, _previewState.Nametable0Attributes, nametable0Surface.Surface);
                
                using DynamicSdlTexture.LockedSurface nametable1Surface = _nametable1Texture.Lock();
                UpdateNametableTexture(_previewState, _previewState.Nametable1, _previewState.Nametable1Attributes, nametable1Surface.Surface);
            }
        }

        using (Profiler.Begin(nameof(PpuTileViewerWindow) + "_Draw", TimeUnit.Millisecond)) {
            ImGui.TextUnformatted($"Scanline: {Emulator.Machine.State.Ppu.Internal.Scanline}");
            ImGui.TextUnformatted($"Scanline cycle: {Emulator.Machine.State.Ppu.Internal.CyclesInScanline}");
            ImGui.TextUnformatted($"Frames: {Emulator.Machine.State.Ppu.Internal.Frames}");
            
            ImGui.Separator();
            
            DrawPatternTable(_patternTable0Texture, "Pattern Table 1");
            ImGui.SameLine();
            DrawPatternTable(_patternTable1Texture, "Pattern Table 2");

            ImGui.Separator();

            ImGui.Checkbox("Draw Attribute Grid", ref _drawAttributeGrid);

            Vector2 oldItemSpacing = ImGui.GetStyle().ItemSpacing;
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, oldItemSpacing/* with { X = 0 }*/);
            DrawNametable(_nametable0Texture, _previewState.Nametable0, _previewState.Nametable0Attributes, 0);
            ImGui.SameLine();
            DrawNametable(_nametable1Texture, _previewState.Nametable1, _previewState.Nametable1Attributes, 1);
            ImGui.PopStyleVar();

            ImGui.Separator();

            const float paletteSpacing = 130;

            // @formatter:off
            DrawPalette(_previewState.BackgroundPalettes[0], "BG Palette 0"); ImGui.SameLine(paletteSpacing * 1);
            DrawPalette(_previewState.BackgroundPalettes[1], "BG Palette 1"); ImGui.SameLine(paletteSpacing * 2);
            DrawPalette(_previewState.BackgroundPalettes[2], "BG Palette 2"); ImGui.SameLine(paletteSpacing * 3);
            DrawPalette(_previewState.BackgroundPalettes[3], "BG Palette 3");
            ImGui.Separator();
    
            DrawPalette(_previewState.SpritePalettes[0], "SPR Palette 0"); ImGui.SameLine(paletteSpacing * 1);
            DrawPalette(_previewState.SpritePalettes[1], "SPR Palette 1"); ImGui.SameLine(paletteSpacing * 2);
            DrawPalette(_previewState.SpritePalettes[2], "SPR Palette 2"); ImGui.SameLine(paletteSpacing * 3);
            DrawPalette(_previewState.SpritePalettes[3], "SPR Palette 3");
            ImGui.Separator();
            // @formatter:on
        }
    }

    private void DrawPalette(PpuObjectPalette palette, string title) {
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
        ImFontPtr font = Context.SharedResources.FontMonospacedNormal;
        ImGui.PushFont(font);
        for (int i = 0; i < 4; i++) {
            Vector2 tileMin = imageMin + Vector2.UnitX * colorTileSize * i;
            byte colorIndex = palette.ColorIndices[i];

            Color color = Color.FromArgb((int) ColorUtility.ArgbToAbgr(Context.SharedResources.PpuPalette.Colors[colorIndex]));

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

        object paletteCopyBoxed = palette;

        TileTooltip(imageMin, colorTileSize, 0xFFFF0000, (x, _) => {
            PpuObjectPalette paletteCopy = (PpuObjectPalette) paletteCopyBoxed;
            byte colorIndex = paletteCopy.ColorIndices[x];
            string colorIndexHex = colorIndex.ToString("X2");
            ImGui.Text(colorIndexHex);
        });

        ImGui.EndGroup();
    }

    private void DrawNametable(
        DynamicSdlTexture nametableTexture,
        in PpuNametable nametable,
        in PpuPreviewState.PpuNametableAttributesDecoded attributes,
        int nametableIndex
    ) {
        const float previewScale = 1.5f;
        const float previewTileSize = PpuConstants.TileSize * previewScale;
        Vector2 previewSize = new Vector2(nametableTexture.Width, nametableTexture.Height) * previewScale;

        ImGui.BeginGroup();
        ImGui.Text($"Nametable {nametableIndex}");

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 cursorPos = ImGui.GetCursorPos();
        ImGui.Dummy(Vector2.One * previewSize);
        Vector2 imageMin = ImGui.GetItemRectMin();
        Vector2 imageMax = ImGui.GetItemRectMax();

        // background
        drawList.AddRectFilled(imageMin, imageMax, 0xFFFF0000);

        ImGui.SetCursorPos(cursorPos);
        ImGui.Image(nametableTexture.Texture, previewSize);

        if (_drawAttributeGrid) {
            DrawAttributesGrid(nametable, attributes, previewTileSize, imageMin);
        }

        TileTooltip(imageMin, Vector2.One * previewTileSize, 0xFFFF0000, null);

        ImGui.EndGroup();
    }

    private void DrawAttributesGrid(
        in PpuNametable nametable,
        in PpuPreviewState.PpuNametableAttributesDecoded attributes,
        float tileSize,
        Vector2 nametableRectMin
    ) {
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        Span<uint> palette = stackalloc uint[4];
        GetGreyscalePalette(100, palette);

        for (int tileRow = 0; tileRow < PpuConstants.NametableHeightTiles + 1; tileRow++) {
            for (int tileColumn = 0; tileColumn < PpuConstants.NametableWidthTiles; tileColumn++) {
                Vector2 tileMin = nametableRectMin + new Vector2(tileColumn, tileRow) * tileSize;

                int tileIndex = tileRow * PpuConstants.NametableWidthTiles + tileColumn;
                byte value = attributes.PaletteIndices[tileIndex];

                drawList.AddRectFilled(
                    tileMin,
                    tileMin + Vector2.One * tileSize,
                    palette[value]
                );
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

    private static void GetGreyscalePalette(byte alpha, Span<uint> patternTablePalette) {
        stackalloc uint[4] {
            (uint) Color.FromArgb(alpha, 0, 0, 0).ToArgb(),
            (uint) Color.FromArgb(alpha, 85, 85, 85).ToArgb(),
            (uint) Color.FromArgb(alpha, 170, 170, 170).ToArgb(),
            (uint) Color.FromArgb(alpha, 255, 255, 255).ToArgb(),
        }.CopyTo(patternTablePalette);
    }

    private void UpdateNametableTexture(
        PpuPreviewState previewState,
        PpuNametable nametable,
        PpuPreviewState.PpuNametableAttributesDecoded attributes,
        SDL_Surface* surface
    ) {
        PpuPreviewState.PpuPatternTableDecoded patternTable =
            (Emulator.Machine.State.Ppu.PPUCTRL & PpuControlRegisterFlags.BackgroundPatternTableAddress) == 0 ?
                previewState.PatternTable0 :
                previewState.PatternTable1;

        Span<uint> paletteColors = stackalloc uint[4];
        GetGreyscalePalette(255, paletteColors);

        int* surfacePixels = (int*) surface->pixels;
        for (int tileRow = 0; tileRow < PpuConstants.NametableHeightTiles; tileRow++) {
            for (int tileColumn = 0; tileColumn < PpuConstants.NametableWidthTiles; tileColumn++) {
                byte tilePaletteIndex = attributes.PaletteIndices[tileRow * PpuConstants.NametableWidthTiles + tileColumn];

                for (int tilePixelIndex = 0; tilePixelIndex < PpuConstants.BytesPerDecodedTile; tilePixelIndex++) {
                    int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;
                    int tilePixelColumn = tilePixelIndex % PpuConstants.TileSize;

                    int surfacePixelRow = tileRow * PpuConstants.TileSize + tilePixelRow;
                    int surfacePixelColumn = tileColumn * PpuConstants.TileSize + tilePixelColumn;

                    int surfacePixelIndex = surfacePixelRow * PpuConstants.NametableWidthPixels + surfacePixelColumn;

                    byte patternTableTileIndex = nametable.Rows[tileRow][tileColumn];

                    byte pixelPaletteColorIndex = patternTable.Tiles[patternTableTileIndex].PaletteColorIndices[tilePixelIndex];
                    PpuObjectPalette palette = _previewState.BackgroundPalettes[tilePaletteIndex];
                    //pixelPaletteColorIndex = (byte) (3 - pixelPaletteColorIndex);
                    //pixelPaletteColorIndex = (byte) ((pixelPaletteColorIndex + 3) % 4);
                    
                    surfacePixels[surfacePixelIndex] = (int) Context.SharedResources.PpuPalette.Colors[palette.ColorIndices[pixelPaletteColorIndex]];
                    
                    //surfacePixels[surfacePixelIndex] = (int) paletteColors[pixelPaletteColorIndex];
                }
            }
        }
    }

    private void UpdatePatternTableTexture(
        PpuPreviewState.PpuPatternTableDecoded patternTable,
        Span<uint> palette,
        SDL_Surface* surface
    ) {
        const int totalTiles = PpuConstants.PatternTableWidthTiles * PpuConstants.PatternTableHeightTiles;

        int* surfacePixels = (int*) surface->pixels;
        for (int tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
            int tileRow = tileIndex / PpuConstants.BytesPerTile;
            int tileColumn = tileIndex % PpuConstants.BytesPerTile;

            for (int tilePixelIndex = 0; tilePixelIndex < PpuConstants.BytesPerDecodedTile; tilePixelIndex++) {
                int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;
                int tilePixelColumn = tilePixelIndex % PpuConstants.TileSize;

                int surfacePixelRow = tileRow * PpuConstants.TileSize + tilePixelRow;
                int surfacePixelColumn = tileColumn * PpuConstants.TileSize + tilePixelColumn;

                int surfacePixelIndex = surfacePixelRow * PpuConstants.PatternTableWidth + surfacePixelColumn;

                byte pixelColorIndex = patternTable.Tiles[tileIndex].PaletteColorIndices[tilePixelIndex];
                surfacePixels[surfacePixelIndex] = (int) palette[pixelColorIndex];
            }
        }
    }
}
