using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Sencae.Ppu;

namespace Sencae.Desktop;

public class PpuCycleOperationDebugWindow : AppWindow {
    public Emulator Emulator { get; }

    private readonly PpuCycleOperation[] _operations;

    public PpuCycleOperationDebugWindow(AppWindowContext context, Emulator emulator) : base(
        nameof(PpuCycleOperationDebugWindow),
        "PPU Cycle Operation Debugger",
        context
    ) {
        Emulator = emulator;
        Flags = ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar;
        _operations = PpuOperationTableGenerator.Generate();
    }

    protected override void OnOpening() {
    }

    protected override void DrawContent(float deltaTime) {
        const int scanlines = 262;
        const int cyclesInScanline = 341;

        const int tileSize = 8;

        Vector2 previewSize = new Vector2(cyclesInScanline, scanlines) * tileSize;

        //ImGui.BeginGroup();
        int ppuFrameCycle = Emulator.Machine.State.Ppu.Internal.FrameCycle;
        int ppuScanline = ppuFrameCycle / cyclesInScanline;
        int ppuScanlineCycle = ppuFrameCycle % cyclesInScanline;

        ImGui.TextUnformatted($"PPU position: {ppuScanlineCycle,3}, {ppuScanline,3} (cycle {ppuFrameCycle})");
        ImGui.TextUnformatted($"PPUSTATUS: {Emulator.Machine.State.Ppu.Registers.PPUSTATUS}");
        ImGui.TextUnformatted($"PPUCTRL: {Emulator.Machine.State.Ppu.Registers.PPUCTRL}");
        ImGui.TextUnformatted($"PPUMASK: {Emulator.Machine.State.Ppu.Registers.PPUMASK}");

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        Vector2 cursorPos = ImGui.GetCursorPos();
        ImGui.Dummy(Vector2.One * previewSize);
        Vector2 imageMin = ImGui.GetItemRectMin();
        Vector2 imageMax = ImGui.GetItemRectMax();

        // background
        //drawList.AddRectFilled(imageMin, imageMax, 0xFFFF0000);
        drawList.AddRectFilled(imageMin, imageMax, 0xFF000000);
        for (int scanline = 0; scanline < scanlines; scanline++) {
            for (int scanlineCycle = 0; scanlineCycle < cyclesInScanline; scanlineCycle++) {
                Vector2 spriteRectMin = imageMin + new Vector2(scanlineCycle, scanline) * tileSize;
                Vector2 spriteRectMax = spriteRectMin + Vector2.One * tileSize;
                if (!ImGui.IsRectVisible(spriteRectMin, spriteRectMin))
                    continue;

                int cycle = scanline * cyclesInScanline + scanlineCycle;
                PpuCycleOperation operation = _operations[cycle];

                uint color = 0xFF_ffe375;
                if (scanline is >= 240) {
                    color = 0xFF_666666;
                }

                if (operation.Flag != PpuCycleOperation.PpuCycleFlagOperation.None) {
                    color = 0xFF_89cb7b;
                } else if (operation.Register != PpuCycleOperation.PpuCycleRegisterOperation.None) {
                    color = 0xFF_1822fc;
                } else if (operation.VRamFetch != PpuCycleOperation.PpuCycleVRamFetchOperation.Idle) {
                    color = 0xFF_96bdf1;
                }

                bool isActiveCycle = ppuFrameCycle == cycle;
                if (isActiveCycle) {
                    color = 0xFF_FF00FF;
                }

                drawList.AddRectFilled(spriteRectMin, spriteRectMax, color, 0, ImDrawFlags.None);
                drawList.AddRect(spriteRectMin, spriteRectMax, 0xFF000000, 0, ImDrawFlags.None, isActiveCycle ? 3 : 1);
            }
        }

        Tooltips.TileTooltip(Context, imageMin, Vector2.One * tileSize, 0xFFFF0000, (x, y) => {
            int cycle = y * cyclesInScanline + x;
            PpuCycleOperation operation = _operations[cycle];
            ImGui.TextUnformatted(
                $"{x,3}, {y,3}\n\n" + 
                $"Flag: {operation.Flag}\n" +
                $"VRamFetch: {operation.VRamFetch}\n" +
                $"Register: {operation.Register}"
            );
        });

        //ImGui.SetCursorPos(cursorPos);
        /*ImGui.Image(nametableTexture.Texture, previewSize);

        if (_drawAttributeGrid) {
            DrawAttributesGrid(ref attributes, previewTileSize, imageMin);
        }

        Tooltips.TileTooltip(Context, imageMin, Vector2.One * previewTileSize, 0xFFFF0000, null);

        ImDrawListPtr drawListPtr = ImGui.GetWindowDrawList();
        ReadOnlySpan<byte> oamMemoryBytes = Emulator.Machine.State.Ppu.OamMemory.AsSpan();
        ReadOnlySpan<PpuObjectAttributes> oamObjects = MemoryMarshal.Cast<byte, PpuObjectAttributes>(oamMemoryBytes);

        bool is8X16Sprite = (Emulator.Machine.State.Ppu.Registers.PPUCTRL & PpuControlRegisterFlags.SpriteSize) != 0;
        Vector2 previewSpriteSize = (is8X16Sprite ? new Vector2(8, 16) : new Vector2(8, 8)) * previewScale;
        for (int i = 0; i < oamObjects.Length; i++) {
            PpuObjectAttributes oamObject = oamObjects[i];

            Vector2 spriteRectMin = imageMin + new Vector2(oamObject.SpriteX, oamObject.SpriteY) * previewScale;
            Vector2 spriteRectMax = spriteRectMin + Vector2.One * previewSpriteSize;
            drawListPtr.AddRect(spriteRectMin, spriteRectMax, 0xFF0000FF, 0, ImDrawFlags.None, 1);
        }*/

        //ImGui.EndGroup();
    }
}
