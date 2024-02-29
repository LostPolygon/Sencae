using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Sencae.Ppu;

namespace Sencae.Desktop;

public unsafe class PpuNametableViewerWindow : AppWindow, IDisposable {
    private readonly NametableContainer[] _nametablesContainers = new NametableContainer[4];

    private readonly PpuPreviewState.PalettesData _palettes = new();
    private readonly PpuPreviewState.PatternTablesData _patternTables = new();
    private readonly PpuPreviewState.NametablesData _nametables = new();

    private bool _drawAttributeGrid;

    public Emulator Emulator { get; }

    public PpuNametableViewerWindow(
        AppWindowContext context,
        Emulator emulator
    ) : base(
        nameof(PpuNametableViewerWindow),
        "PPU Nametables",
        context
    ) {
        Emulator = emulator;

        for (int index = 0; index < _nametablesContainers.Length; index++) {
            _nametablesContainers[index] = new NametableContainer(context);
        }
    }

    public void Dispose() {
        foreach (NametableContainer nametableContainer in _nametablesContainers) {
            nametableContainer.Dispose();
        }
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
        previewStateUpdater.FillPatternTables(_patternTables);
        previewStateUpdater.FillNametables(_nametables);

        Span<PpuPreviewState.NametableData> nametablesSpan = _nametables.AsSpan();
        for (int i = 0; i < nametablesSpan.Length; i++) {
            using DynamicSdlTexture.LockedSurface nametableSurface = _nametablesContainers[i].Texture.Lock();
            ref PpuPreviewState.NametableData nametable = ref nametablesSpan[i];
            UpdateNametableTexture(ref nametable.Nametable, nametable.Attributes, nametableSurface.Surface);
        }

        ImGui.Checkbox("Draw Attribute Grid", ref _drawAttributeGrid);

        Vector2 oldItemSpacing = ImGui.GetStyle().ItemSpacing;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, oldItemSpacing with { X = 0 });
        
        DrawNametable(_nametablesContainers[0].Texture, _nametables.Nametable0.Nametable, ref _nametables.Nametable0.Attributes, 0);
        ImGui.SameLine();
        DrawNametable(_nametablesContainers[1].Texture, _nametables.Nametable1.Nametable, ref _nametables.Nametable1.Attributes, 1);
        
        DrawNametable(_nametablesContainers[2].Texture, _nametables.Nametable2.Nametable, ref _nametables.Nametable2.Attributes, 2);
        ImGui.SameLine();
        DrawNametable(_nametablesContainers[3].Texture, _nametables.Nametable3.Nametable, ref _nametables.Nametable3.Attributes, 3);
        
        ImGui.PopStyleVar();
        
        ImGui.BeginTable("Stats", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        {
            ImGui.TableSetupColumn("Id");
            ImGui.TableSetupColumn("Position", ImGuiTableColumnFlags.WidthFixed, 90);
            ImGui.TableSetupColumn("Tile", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Attributes", ImGuiTableColumnFlags.WidthFixed, 70);
            
            ImGui.TableHeadersRow();

            ReadOnlySpan<byte> oamMemoryBytes = Emulator.Machine.State.Ppu.OamMemory.AsSpan();
            ReadOnlySpan<PpuObjectAttributes> oamObjects = MemoryMarshal.Cast<byte, PpuObjectAttributes>(oamMemoryBytes);
            for (int i = 0; i < oamObjects.Length; i++) {
                PpuObjectAttributes oamObject = oamObjects[i];
                
                ImGui.TableNextRow();
                
                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"{i:00}");
                    
                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"{oamObject.SpriteX}, {oamObject.SpriteY}");
                
                ImGui.TableSetColumnIndex(2);
                ImGui.Text($"{oamObject.SpriteTileIndex:X2}");
                
                ImGui.TableSetColumnIndex(3);
                ImGui.Text($"{oamObject.SpriteAttributes:X2}");
            }
        }
        ImGui.EndTable();
    }

    private void UpdateNametableTexture(
        ref PpuNametable nametable,
        in PpuPreviewState.PpuNametableAttributesPaletteIndices attributes,
        SDL_Surface* surface
    ) {
        ReadOnlySpan<byte> attributesSpan = attributes.AsReadOnlySpan();
        Span<PpuNametable.PpuNametableRow> nametableRowsSpan = nametable.Rows.AsSpan();

        ref PpuPreviewState.PpuPatternTableTiles patternTable =
            ref (Emulator.Machine.State.Ppu.PPUCTRL & PpuControlRegisterFlags.BackgroundPatternTableAddress) == 0 ?
                ref _patternTables.PatternTable0 :
                ref _patternTables.PatternTable1;

        ReadOnlySpan<PpuPreviewState.PpuTilePaletteColorIndices> patternTableSpan = patternTable.AsReadOnlySpan();

        //Span<uint> palette = stackalloc uint[4];
        //PaletteUtility.GetGreyscalePalette(palette, 255);

        int* surfacePixels = (int*) surface->pixels;
        for (int tileRow = 0; tileRow < PpuConstants.NametableHeightTiles; tileRow++) {
            ReadOnlySpan<byte> nametableRowSpan = PpuNametable.PpuNametableRow.AsReadOnlySpan(ref nametableRowsSpan[tileRow]);
            
            for (int tileColumn = 0; tileColumn < PpuConstants.NametableWidthTiles; tileColumn++) {
                byte tilePaletteIndex = attributesSpan[tileRow * PpuConstants.NametableWidthTiles + tileColumn];
                byte patternTableTileIndex = nametableRowSpan[tileColumn];
                ReadOnlySpan<byte> patternTableTilePaletteColorIndices = patternTableSpan[patternTableTileIndex].AsReadOnlySpan();

                for (int tilePixelIndex = 0; tilePixelIndex < PpuConstants.BytesPerDecodedTile; tilePixelIndex++) {
                    int tilePixelRow = tilePixelIndex / PpuConstants.TileSize;
                    int tilePixelColumn = tilePixelIndex % PpuConstants.TileSize;

                    int surfacePixelRow = tileRow * PpuConstants.TileSize + tilePixelRow;
                    int surfacePixelColumn = tileColumn * PpuConstants.TileSize + tilePixelColumn;

                    int surfacePixelIndex = surfacePixelRow * PpuConstants.NametableWidthPixels + surfacePixelColumn;

                    byte pixelPaletteColorIndex = patternTableTilePaletteColorIndices[tilePixelIndex];
                    ref PpuObjectPalette palette = ref _palettes.Background[tilePaletteIndex];

                    surfacePixels[surfacePixelIndex] = (int) Context.SharedResources.PpuPalette.Colors[palette.ColorIndices[pixelPaletteColorIndex]];

                    //surfacePixels[surfacePixelIndex] = (int) paletteColors[pixelPaletteColorIndex];
                }
            }
        }
    }

    private void DrawNametable(
        DynamicSdlTexture nametableTexture,
        in PpuNametable nametable,
        ref PpuPreviewState.PpuNametableAttributesPaletteIndices attributes,
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
            DrawAttributesGrid(ref attributes, previewTileSize, imageMin);
        }

        Tooltips.TileTooltip(Context, imageMin, Vector2.One * previewTileSize, 0xFFFF0000, null);
        
        ImDrawListPtr drawListPtr = ImGui.GetWindowDrawList();
        ReadOnlySpan<byte> oamMemoryBytes = Emulator.Machine.State.Ppu.OamMemory.AsSpan();
        ReadOnlySpan<PpuObjectAttributes> oamObjects = MemoryMarshal.Cast<byte, PpuObjectAttributes>(oamMemoryBytes);

        bool is8X16Sprite = (Emulator.Machine.State.Ppu.PPUCTRL & PpuControlRegisterFlags.SpriteSize) != 0;
        Vector2 previewSpriteSize = (is8X16Sprite ? new Vector2(8, 16) : new Vector2(8, 8)) * previewScale;
        for (int i = 0; i < oamObjects.Length; i++) {
            PpuObjectAttributes oamObject = oamObjects[i];

            Vector2 spriteRectMin = imageMin + new Vector2(oamObject.SpriteX, oamObject.SpriteY) * previewScale;
            Vector2 spriteRectMax = spriteRectMin + Vector2.One * previewSpriteSize;
            drawListPtr.AddRect(spriteRectMin, spriteRectMax, 0xFF0000FF, 0, ImDrawFlags.None, 1);
        }

        ImGui.EndGroup();
    }

    private void DrawAttributesGrid(
        ref PpuPreviewState.PpuNametableAttributesPaletteIndices attributes,
        float tileSize,
        Vector2 nametableRectMin
    ) {
        ReadOnlySpan<byte> attributesSpan = attributes.AsReadOnlySpan();
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        Span<uint> palette = stackalloc uint[4];
        PaletteUtility.GetGreyscalePalette(palette, 100);

        for (int tileRow = 0; tileRow < PpuConstants.NametableHeightTiles + 1; tileRow++) {
            for (int tileColumn = 0; tileColumn < PpuConstants.NametableWidthTiles; tileColumn++) {
                Vector2 tileMin = nametableRectMin + new Vector2(tileColumn, tileRow) * tileSize;

                int tileIndex = tileRow * PpuConstants.NametableWidthTiles + tileColumn;
                byte value = attributesSpan[tileIndex];

                drawList.AddRectFilled(
                    tileMin,
                    tileMin + Vector2.One * tileSize,
                    palette[value]
                );
            }
        }
    }

    private class NametableContainer : IDisposable {
        public DynamicSdlTexture Texture;

        public NametableContainer(AppWindowContext context) {
            Texture = new DynamicSdlTexture(
                context.Renderer,
                PpuConstants.NametableWidthPixels,
                PpuConstants.NametableHeightPixels,
                SDL_PIXELFORMAT_ARGB8888
            );
        }

        public void Dispose() {
            Texture.Dispose();
        }
    }
}
