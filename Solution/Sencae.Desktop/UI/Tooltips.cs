using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public class Tooltips {
    public static void TileTooltip(
        AppWindowContext context,
        Vector2 rectMin,
        Vector2 tileSize,
        uint frameColor,
        Action<int, int>? drawContents
    ) {
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

        GuiVisualUtility.DrawCurrentWindowDropShadow(context.SharedResources.ShadowTexture, 25, 150, 2.75f);
        ImGui.EndTooltip();
    }
}
