using System.Drawing;
using System.Numerics;
using ImGuiNET;
using Perfolizer.Horology;
using Sencae.Ppu;

namespace Sencae.Desktop;

public unsafe class PpuTileViewerWindow : AppWindow, IDisposable {
    private readonly DynamicSdlTexture _patternTable1Texture;
    private readonly DynamicSdlTexture _patternTable2Texture;

    public Emulator Emulator { get; set; }

    public PpuTileViewerWindow(
        AppWindowContext context
    ) : base(
        nameof(PpuTileViewerWindow),
        "PPU Tile Viewer",
        context
    ) {
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

        if (Emulator == null!)
            return;

        {
            using var _ = AppProfiler.Begin(nameof(PpuTileViewerWindow) + "_Preview", TimeUnit.Millisecond);

            using DynamicSdlTexture.LockedSurface pattern1TableSurface = _patternTable1Texture.Lock();
            FillSurfaceFromVram(0, pattern1TableSurface.Surface);

            using DynamicSdlTexture.LockedSurface pattern2TableSurface = _patternTable2Texture.Lock();
            FillSurfaceFromVram(PpuConstants.PatternTable1.Start.Value, pattern2TableSurface.Surface);
        }

        DrawPatternTable(_patternTable1Texture, "Pattern Table 1");
        ImGui.SameLine();
        DrawPatternTable(_patternTable2Texture, "Pattern Table 2");

        ImGui.Separator();

        DrawNametable0();
    }

    private void DrawNametable0() {
        ImGui.Text("Nametable 0");

        const float tileSize = 12;

        PpuBusDevice ppuBusDevice = Emulator.Machine.PpuBusDevice;

        if (ImGui.BeginChild("##Nametable", new Vector2(32 * tileSize, 30 * tileSize), true)) {
            ImGui.EndChild();
        }

        Vector2 nametableRectMin = ImGui.GetItemRectMin();
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        for (int y = 0; y < 30; y++) {
            for (int x = 0; x < 32; x++) {
                Vector2 cellMin = nametableRectMin + new Vector2(x, y) * tileSize;
                Vector2 cellMax = cellMin + Vector2.One * tileSize;

                int index = y * 32 + x + 0x2000;
                byte value = ppuBusDevice.Read((ushort) index);

                drawList.AddRectFilled(cellMin, cellMax, 0xFF_00_00_00 | value | ((uint) value << 8) | ((uint) value << 16));
            }
        }

        TileTooltip(nametableRectMin, tileSize, 0xFFFF0000, (x, y) => {
            int index = y * 32 + x + 0x2000;
            byte patternTableTileIndex = ppuBusDevice.Read((ushort) index);

            int tileRow = patternTableTileIndex / 16;
            int tileColumn = patternTableTileIndex % 16;

            const float uvInset = 0.001f;
            Vector2 uv0 = new Vector2(tileColumn, tileRow) * (1f / 16f) + Vector2.One * uvInset;
            Vector2 uv1 = uv0 + Vector2.One * (1f / 16f - uvInset);

            ImGui.Text($"{x}, {y}");

            ImGui.Image(
                _patternTable1Texture.Texture,
                Vector2.One * 3 * tileSize,
                uv0,
                uv1
            );
        });
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

        TileTooltip(imageMin, previewTileSize, 0xFFFF0000, null);

        ImGui.EndGroup();
    }

    private void TileTooltip(Vector2 rectMin, float tileSize, uint frameColor, Action<int, int>? drawContents) {
        if (!ImGui.IsItemHovered())
            return;

        Vector2 mousePos = ImGui.GetMousePos();

        Vector2 activeTileMin = mousePos - rectMin;
        activeTileMin.X = (int) (activeTileMin.X / tileSize);
        activeTileMin.Y = (int) (activeTileMin.Y / tileSize);

        int tileX = (int) activeTileMin.X;
        int tileY = (int) activeTileMin.Y;

        activeTileMin *= tileSize;
        activeTileMin += rectMin;
        Vector2 activeTileMax = activeTileMin + Vector2.One * tileSize;

        ImDrawListPtr drawListPtr = ImGui.GetWindowDrawList();
        drawListPtr.AddRect(activeTileMin, activeTileMax, frameColor, 0, ImDrawFlags.None, 2);

        ImGui.SetNextWindowPos(activeTileMin + new Vector2(tileSize * 2, tileSize));
        ImGui.BeginTooltip();

        drawContents ??= (x, y) => ImGui.Text($"{x}, {y}");
        drawContents(tileX, tileY);

        GuiVisualUtility.DrawCurrentWindowDropShadow(Context.SharedResources.ShadowTexture, 25, 150, 2.75f);
        ImGui.EndTooltip();
    }

    private void FillSurfaceFromVram(int vramOffset, SDL_Surface* surface) {
        Span<int> bitPaletteArgb = stackalloc int[4] {
            Color.FromArgb(0, 0, 0, 0).ToArgb(),
            Color.FromArgb(255, 64, 64, 64).ToArgb(),
            Color.FromArgb(255, 128, 128, 128).ToArgb(),
            Color.FromArgb(255, 192, 192, 192).ToArgb(),
        };

        PpuBusDevice ppuBusDevice = Emulator.Machine.PpuBusDevice;

        for (int pixelIndex = 0; pixelIndex < PpuConstants.PatternTablePixelCount; pixelIndex++) {
            const int tilePixelCount = PpuConstants.TileSize * PpuConstants.TileSize;
            const int bytesPerTile = 16;

            int tileIndex = pixelIndex / tilePixelCount;
            int tileRow = tileIndex / bytesPerTile;
            int tileColumn = tileIndex % bytesPerTile;

            int tilePixelIndex = (pixelIndex - tileIndex * tilePixelCount);

            int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;
            int tilePixelColumn = tilePixelIndex % PpuConstants.TileSize;

            int surfacePixelRow = tileRow * PpuConstants.TileSize + tilePixelRow;
            int surfacePixelColumn = tileColumn * PpuConstants.TileSize + tilePixelColumn;

            int bit = 7 - pixelIndex % 8;
            ushort address = (ushort) (tileIndex * bytesPerTile + tilePixelRow + vramOffset);

            byte pixelPaletteValue = (byte) (
                ((ppuBusDevice.Read(address) >> bit) & 1) |
                (((ppuBusDevice.Read((ushort) (address + 8)) >> bit) & 1) << 1)
            );

            int* surfacePixels = (int*) surface->pixels;
            int surfacePixelIndex = surfacePixelRow * PpuConstants.PatternTableWidth + surfacePixelColumn;
            surfacePixels[surfacePixelIndex] = bitPaletteArgb[pixelPaletteValue];
        }
    }
}
