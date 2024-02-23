using ImGuiNET;

namespace Sencae.Desktop;

public static unsafe class ImGuiGlobals {
    public static IntPtr Context { get; private set; }
    public static ImGuiIOPtr IO { get; private set; }

    public static ImFontPtr FontMonospacedSmall { get; private set; }

    public static ImFontPtr FontMonospacedNormal { get; private set; }

    public static ImFontPtr FontUINormal { get; private set; }
    public static ImFontPtr FontFontAwesomeIconsSolid { get; private set; }
    public static ImFontPtr FontFontAwesomeIconsRegular { get; private set; }

    public static void Initialize() {
        Context = ImGui.CreateContext();
        IO = ImGui.GetIO();
        IO.ConfigFlags |=
            ImGuiConfigFlags.NavEnableGamepad |
            ImGuiConfigFlags.NavEnableKeyboard |
            ImGuiConfigFlags.DpiEnableScaleFonts |
            ImGuiConfigFlags.DpiEnableScaleViewports |
            ImGuiConfigFlags.ViewportsEnable |
            ImGuiConfigFlags.DockingEnable;
        
        ImGuiAdobeSpectrumTheme.StyleColorsSpectrumDark();

        ImFontGlyphRangesBuilder fontAwesomeSolidGlyphsBuilder = default;
        BuildFontGlyphRanges(
            new[] {
                IconsFontAwesome5.FolderOpen[0],
            },
            &fontAwesomeSolidGlyphsBuilder,
            out IntPtr fontAwesomeSolidRangesData
        );

        ImFontGlyphRangesBuilder fontAwesomeRegularGlyphsBuilder = default;
        BuildFontGlyphRanges(
            new[] {
                IconsFontAwesome5.Bug[0],
            },
            &fontAwesomeRegularGlyphsBuilder,
            out IntPtr fontAwesomeRegularRangesData
        );

        FontUINormal = AddFont("AdobeClean/AdobeClean-Regular.otf", 16);

        const float fontAwesomeSize = 16 * 4.0f / 5.0f;
        FontFontAwesomeIconsRegular = AddFont(
            "FontAwesome6/Font Awesome 6 Free-Regular-400.otf",
            fontAwesomeSize,
            fontAwesomeSolidRangesData,
            config => {
                config.MergeMode = 1;
                config.PixelSnapH = 1;
                config.GlyphMinAdvanceX = fontAwesomeSize;
                return config;
            }
        );

        FontFontAwesomeIconsSolid = AddFont(
            "FontAwesome6/Font Awesome 6 Free-Solid-900.otf",
            fontAwesomeSize,
            fontAwesomeRegularRangesData,
            config => {
                config.MergeMode = 1;
                config.PixelSnapH = 1;
                config.GlyphMinAdvanceX = fontAwesomeSize;
                return config;
            }
        );

        FontMonospacedNormal = AddFont("AdobeSourceCodePro/SourceCodePro-Regular.ttf", 16);
        FontMonospacedSmall = AddFont("AdobeSourceCodePro/SourceCodePro-Regular.ttf", 14);

        IO.Fonts.Build();
    }

    private static void BuildFontGlyphRanges(char[] glyphs, ImFontGlyphRangesBuilder* rangesBuilderPtrRaw, out IntPtr rangesData) {
        ImFontGlyphRangesBuilderPtr rangesBuilderPtr = new(rangesBuilderPtrRaw);
        rangesBuilderPtr.Clear();

        foreach (ushort glyph in glyphs) {
            rangesBuilderPtr.AddChar(glyph);
        }

        // ReSharper disable once RedundantAssignment
        // ReSharper disable once InlineOutVariableDeclaration
        // serhiiy: explicit declaration fixes crash with SkipLocalsInit
        ImVector glyphRanges = default;
        rangesBuilderPtr.BuildRanges(out glyphRanges);
        rangesData = glyphRanges.Data;
    }

    private static ImFontPtr AddFont(
        string path,
        float sizePixels,
        IntPtr? fontAwesomeRangesData = null,
        Func<ImFontConfig, ImFontConfig>? modifyFontConfigFunc = null
    ) {
        ImFontConfig fontConfig = CreateFontConfig();

        if (modifyFontConfigFunc != null) {
            fontConfig = modifyFontConfigFunc(fontConfig);
        }

        ImFontPtr fontPtr = IO.Fonts.AddFontFromFileTTF(
            $"Resources/Fonts/{path}",
            sizePixels,
            new ImFontConfigPtr(&fontConfig),
            fontAwesomeRangesData ?? IntPtr.Zero
        );

        if (fontPtr.NativePtr == null)
            throw new InvalidOperationException($"Failed to load font {path}");

        return fontPtr;
    }

    private static ImFontConfig CreateFontConfig() {
        ImFontConfig fontConfig = default;
        fontConfig.FontDataOwnedByAtlas = 1;
        fontConfig.OversampleH = 3;
        fontConfig.OversampleV = 3;
        fontConfig.GlyphMaxAdvanceX = float.MaxValue;
        fontConfig.RasterizerMultiply = 1.0f;
        fontConfig.EllipsisChar = unchecked((ushort) -1);
        return fontConfig;
    }
}
