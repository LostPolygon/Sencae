using ImGuiNET;

namespace Sencae.Desktop;

public static unsafe class ImGuiGlobals {
    public static IntPtr Context { get; private set; }
    public static ImGuiIOPtr IO { get; private set; }

    public static ImFontPtr FontMonospacedSmall { get; private set; }

    public static ImFontPtr FontMonospacedNormal { get; private set; }

    public static ImFontPtr FontUINormal { get; private set; }

    public static void Initialize() {
        Context = ImGui.CreateContext();
        IO = ImGui.GetIO();
        IO.ConfigFlags |=
            ImGuiConfigFlags.NavEnableGamepad |
            ImGuiConfigFlags.NavEnableKeyboard |
            ImGuiConfigFlags.DpiEnableScaleFonts |
            ImGuiConfigFlags.DpiEnableScaleViewports |
            ImGuiConfigFlags.ViewportsEnable;

        AdobeSpectrumTheme.StyleColorsSpectrumDark();

        FontUINormal = AddFont("AdobeClean/AdobeClean-Regular.otf", 16);
        FontMonospacedNormal = AddFont("AdobeSourceCodePro/SourceCodePro-Regular.ttf", 16);
        FontMonospacedSmall = AddFont("AdobeSourceCodePro/SourceCodePro-Regular.ttf", 14);
    }

    private static ImFontPtr AddFont(string path, float sizePixels) {
        ImFontConfig fontConfig;
        fontConfig.FontDataOwnedByAtlas = 1;
        fontConfig.OversampleH = 3;
        fontConfig.OversampleV = 3;
        fontConfig.GlyphMaxAdvanceX = float.MaxValue;
        fontConfig.RasterizerMultiply = 1.0f;
        fontConfig.EllipsisChar = unchecked((ushort) -1);

        return IO.Fonts.AddFontFromFileTTF($"Resources/Fonts/{path}", sizePixels, new ImFontConfigPtr(&fontConfig));
    }
}
