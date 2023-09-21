using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public static class GuiVisualUtility {
    public static void DrawDropShadow(IntPtr texture, Vector2 rectMin, Vector2 rectMax, float size, byte alpha, float inset) {
        if (alpha == 0)
            return;

        Vector2 insetVector = Vector2.One * inset;
        rectMin += insetVector;
        rectMax -= insetVector;
        
        const float uv0 = 0.0f; // left/top region
        const float uv1 = 0.333333f; // leftward/upper region
        const float uv2 = 0.666666f; // rightward/lower region
        const float uv3 = 1.0f; // right/bottom region
        uint shadowColor = (uint) ((alpha << 24) | 0xFFFFFF);

        ImDrawListPtr dl = ImGui.GetWindowDrawList();

        void DrawTile(Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax) {
            dl.AddImage(texture, posMin, posMax, uvMin, uvMax, shadowColor);
        }

        // @formatter:off
        dl.PushClipRectFullScreen();
        DrawTile(new(rectMin.X - size, rectMin.Y - size), new(rectMin.X       , rectMin.Y       ), new(uv0, uv0), new(uv1, uv1));
        DrawTile(new(rectMin.X       , rectMin.Y - size), new(rectMax.X       , rectMin.Y       ), new(uv1, uv0), new(uv2, uv1));
        DrawTile(new(rectMax.X       , rectMin.Y - size), new(rectMax.X + size, rectMin.Y       ), new(uv2, uv0), new(uv3, uv1));
        
        DrawTile(new(rectMin.X - size, rectMin.Y       ), new(rectMin.X       , rectMax.Y       ), new(uv0, uv1), new(uv1, uv2));
        DrawTile(new(rectMax.X       , rectMin.Y       ), new(rectMax.X + size, rectMax.Y       ), new(uv2, uv1), new(uv3, uv2));
        
        DrawTile(new(rectMin.X - size, rectMax.Y       ), new(rectMin.X       , rectMax.Y + size), new(uv0, uv2), new(uv1, uv3));
        DrawTile(new(rectMin.X       , rectMax.Y       ), new(rectMax.X       , rectMax.Y + size), new(uv1, uv2), new(uv2, uv3));
        DrawTile(new(rectMax.X       , rectMax.Y       ), new(rectMax.X + size, rectMax.Y + size), new(uv2, uv2), new(uv3, uv3));
        dl.PopClipRect();
        // @formatter:on
    }

    public static void DrawCurrentWindowDropShadow(IntPtr texture, float size, byte alpha, float inset) {
        if (alpha == 0)
            return;

        DrawDropShadow(
            texture,
            ImGui.GetWindowPos(),
            ImGui.GetWindowPos() + ImGui.GetWindowSize(),
            size,
            alpha,
            inset
        );
    }
}
