using System.Numerics;
using ImGuiNET;
using Sencae.Ppu;

namespace Sencae.Desktop;

public unsafe class PpuPatternTableViewerWindow : AppWindow, IDisposable {
    private readonly PatternTableContainer[] _patternTablesContainers = new PatternTableContainer[2];
    private readonly PpuPreviewState.PatternTablesData _patternTables = new();

    public Emulator Emulator { get; }

    public PpuPatternTableViewerWindow(
        AppWindowContext context,
        Emulator emulator
    ) : base(
        nameof(PpuPatternTableViewerWindow),
        "PPU Pattern Tables",
        context
    ) {
        Emulator = emulator;

        for (int index = 0; index < _patternTablesContainers.Length; index++) {
            _patternTablesContainers[index] = new PatternTableContainer(context);
        }
    }

    public void Dispose() {
        foreach (PatternTableContainer nametableContainer in _patternTablesContainers) {
            nametableContainer.Dispose();
        }
    }

    protected override void DrawContent(float deltaTime) {
        ImGui.SetWindowSize(new Vector2(270, 280), ImGuiCond.FirstUseEver);

        if (!Emulator.IsLoaded)
            return;

        PpuPreviewStateUpdater previewStateUpdater = new(Emulator.Machine.PpuMemoryBus);
        previewStateUpdater.FillPatternTables(_patternTables);

        Span<uint> patternTablePalette = stackalloc uint[4];
        PaletteUtility.GetGreyscalePalette(patternTablePalette, 255);

        Span<PpuPreviewState.PpuPatternTableTiles> patternTablesSpan = _patternTables.AsSpan();
        for (int i = 0; i < patternTablesSpan.Length; i++) {
            using DynamicSdlTexture.LockedSurface patternTableSurface = _patternTablesContainers[i].Texture.Lock();
            UpdatePatternTableTexture(ref patternTablesSpan[i], patternTablePalette, patternTableSurface.Surface);
        }

        DrawPatternTable(_patternTablesContainers[0].Texture, "Pattern Table 1");
        ImGui.SameLine();
        DrawPatternTable(_patternTablesContainers[1].Texture, "Pattern Table 2");
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

        Tooltips.TileTooltip(Context, imageMin, Vector2.One * previewTileSize, 0xFFFF0000, null);

        ImGui.EndGroup();
    }

    private static void UpdatePatternTableTexture(
        ref PpuPreviewState.PpuPatternTableTiles patternTable,
        Span<uint> palette,
        SDL_Surface* surface
    ) {
        ReadOnlySpan<PpuPreviewState.PpuTilePaletteColorIndices> patternTableTilesSpan = patternTable.AsReadOnlySpan() ;
        
        const int totalTiles = PpuConstants.PatternTableWidthTiles * PpuConstants.PatternTableHeightTiles;
        int* surfacePixels = (int*) surface->pixels;
        for (int tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
            ReadOnlySpan<byte> tilePaletteIndices = patternTableTilesSpan[tileIndex].AsReadOnlySpan();

            int tileRow = tileIndex / PpuConstants.BytesPerTile;
            int tileColumn = tileIndex % PpuConstants.BytesPerTile;

            for (int tilePixelIndex = 0; tilePixelIndex < PpuConstants.BytesPerDecodedTile; tilePixelIndex++) {
                int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;
                int tilePixelColumn = tilePixelIndex % PpuConstants.TileSize;

                int surfacePixelRow = tileRow * PpuConstants.TileSize + tilePixelRow;
                int surfacePixelColumn = tileColumn * PpuConstants.TileSize + tilePixelColumn;

                int surfacePixelIndex = surfacePixelRow * PpuConstants.PatternTableWidth + surfacePixelColumn;

                byte pixelColorIndex = tilePaletteIndices[tilePixelIndex];
                surfacePixels[surfacePixelIndex] = (int) palette[pixelColorIndex];
            }
        }
    }

    private class PatternTableContainer(AppWindowContext context) : IDisposable {
        public DynamicSdlTexture Texture = new(
            context.Renderer,
            PpuConstants.PatternTableWidth,
            PpuConstants.PatternTableHeight,
            SDL_PIXELFORMAT_ARGB8888
        );

        public void Dispose() {
            Texture.Dispose();
        }
    }
}
