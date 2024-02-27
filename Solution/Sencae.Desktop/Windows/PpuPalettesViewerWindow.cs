using System.Drawing;
using System.Numerics;
using ImGuiNET;
using Sencae.Ppu;

namespace Sencae.Desktop;

public unsafe class PpuPalettesViewerWindow : AppWindow, IDisposable {
    private readonly PpuPreviewState.PalettesData _palettes = new();

    public Emulator Emulator { get; }

    public PpuPalettesViewerWindow(
        AppWindowContext context,
        Emulator emulator
    ) : base(
        nameof(PpuPalettesViewerWindow),
        "PPU Palettes",
        context
    ) {
        Emulator = emulator;
    }

    public void Dispose() {
    }

    protected override void DrawContent(float deltaTime) {
        ImGui.SetWindowSize(new Vector2(270, 280), ImGuiCond.FirstUseEver);

        if (!Emulator.IsLoaded)
            return;

        PpuPreviewStateUpdater previewStateUpdater = new(
            Emulator.Machine.PpuBusDevice,
            Emulator.Machine.PpuMemoryBus
        );

        previewStateUpdater.FillPalettes(_palettes);

        const float paletteSpacing = 130;

        // @formatter:off
        DrawPalette(_palettes.Background[0], "BG Palette 0"); ImGui.SameLine(paletteSpacing * 1);
        DrawPalette(_palettes.Background[1], "BG Palette 1"); ImGui.SameLine(paletteSpacing * 2);
        DrawPalette(_palettes.Background[2], "BG Palette 2"); ImGui.SameLine(paletteSpacing * 3);
        DrawPalette(_palettes.Background[3], "BG Palette 3");
        ImGui.Separator();
    
        DrawPalette(_palettes.Sprite[0], "SPR Palette 0"); ImGui.SameLine(paletteSpacing * 1);
        DrawPalette(_palettes.Sprite[1], "SPR Palette 1"); ImGui.SameLine(paletteSpacing * 2);
        DrawPalette(_palettes.Sprite[2], "SPR Palette 2"); ImGui.SameLine(paletteSpacing * 3);
        DrawPalette(_palettes.Sprite[3], "SPR Palette 3");
        ImGui.Separator();
        // @formatter:on
    }

    private void DrawPalette(in PpuObjectPalette palette, string title) {
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

            //drawList.AddRectFilled(
            //    textPos,
            //    textPos + textSize,
            //    0xaa0000FF
            //);
        }

        ImGui.PopFont();

        object paletteCopyBoxed = palette;

        Tooltips.TileTooltip(Context, imageMin, colorTileSize, 0xFFFF0000, (x, _) => {
            PpuObjectPalette paletteCopy = (PpuObjectPalette) paletteCopyBoxed;
            byte colorIndex = paletteCopy.ColorIndices[x];
            string colorIndexHex = colorIndex.ToString("X2");
            ImGui.Text(colorIndexHex);
        });

        ImGui.EndGroup();
    }
}
