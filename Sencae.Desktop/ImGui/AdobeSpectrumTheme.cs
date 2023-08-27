using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public static unsafe class AdobeSpectrumTheme {
    public static void StyleColorsSpectrumLight() {
        ImGuiStyle* style = ImGui.GetStyle();
        SetStyleMethod(style);

        Vector4* colors = &style->Colors_0;
        colors[(int) ImGuiCol.Text] = RgbColor(LightThemeColors.GRAY800); // text on hovered controls is gray900
        colors[(int) ImGuiCol.TextDisabled] = RgbColor(LightThemeColors.GRAY500);
        colors[(int) ImGuiCol.WindowBg] = RgbColor(LightThemeColors.GRAY100);
        colors[(int) ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.PopupBg] = RgbColor(LightThemeColors.GRAY50); // not sure about this. Note: applies to tooltips too.
        colors[(int) ImGuiCol.Border] = RgbColor(LightThemeColors.GRAY300);
        colors[(int) ImGuiCol.BorderShadow] = RgbColor(StaticColors.NONE); // We don't want shadows. Ever.
        colors[(int) ImGuiCol.FrameBg] = RgbColor(LightThemeColors.GRAY75); // this isnt right, spectrum does not do this, but it's a good fallback
        colors[(int) ImGuiCol.FrameBgHovered] = RgbColor(LightThemeColors.GRAY50);
        colors[(int) ImGuiCol.FrameBgActive] = RgbColor(LightThemeColors.GRAY200);
        colors[(int) ImGuiCol.TitleBg] = RgbColor(LightThemeColors.GRAY300); // those titlebar values are totally made up, spectrum does not have this.
        colors[(int) ImGuiCol.TitleBgActive] = RgbColor(LightThemeColors.GRAY200);
        colors[(int) ImGuiCol.TitleBgCollapsed] = RgbColor(LightThemeColors.GRAY400);
        colors[(int) ImGuiCol.MenuBarBg] = RgbColor(LightThemeColors.GRAY100);
        colors[(int) ImGuiCol.ScrollbarBg] = RgbColor(LightThemeColors.GRAY100); // same as regular background
        colors[(int) ImGuiCol.ScrollbarGrab] = RgbColor(LightThemeColors.GRAY400);
        colors[(int) ImGuiCol.ScrollbarGrabHovered] = RgbColor(LightThemeColors.GRAY600);
        colors[(int) ImGuiCol.ScrollbarGrabActive] = RgbColor(LightThemeColors.GRAY700);
        colors[(int) ImGuiCol.CheckMark] = RgbColor(LightThemeColors.BLUE500);
        colors[(int) ImGuiCol.SliderGrab] = RgbColor(LightThemeColors.GRAY700);
        colors[(int) ImGuiCol.SliderGrabActive] = RgbColor(LightThemeColors.GRAY800);
        colors[(int) ImGuiCol.Button] = RgbColor(LightThemeColors.GRAY75); // match default button to Spectrum's 'Action Button'.
        colors[(int) ImGuiCol.ButtonHovered] = RgbColor(LightThemeColors.GRAY50);
        colors[(int) ImGuiCol.ButtonActive] = RgbColor(LightThemeColors.GRAY200);
        colors[(int) ImGuiCol.Header] = RgbColor(LightThemeColors.BLUE400);
        colors[(int) ImGuiCol.HeaderHovered] = RgbColor(LightThemeColors.BLUE500);
        colors[(int) ImGuiCol.HeaderActive] = RgbColor(LightThemeColors.BLUE600);
        colors[(int) ImGuiCol.Separator] = RgbColor(LightThemeColors.GRAY400);
        colors[(int) ImGuiCol.SeparatorHovered] = RgbColor(LightThemeColors.GRAY600);
        colors[(int) ImGuiCol.SeparatorActive] = RgbColor(LightThemeColors.GRAY700);
        colors[(int) ImGuiCol.ResizeGrip] = RgbColor(LightThemeColors.GRAY400);
        colors[(int) ImGuiCol.ResizeGripHovered] = RgbColor(LightThemeColors.GRAY600);
        colors[(int) ImGuiCol.ResizeGripActive] = RgbColor(LightThemeColors.GRAY700);
        colors[(int) ImGuiCol.PlotLines] = RgbColor(LightThemeColors.BLUE400);
        colors[(int) ImGuiCol.PlotLinesHovered] = RgbColor(LightThemeColors.BLUE600);
        colors[(int) ImGuiCol.PlotHistogram] = RgbColor(LightThemeColors.BLUE400);
        colors[(int) ImGuiCol.PlotHistogramHovered] = RgbColor(LightThemeColors.BLUE600);
        colors[(int) ImGuiCol.TextSelectedBg] = RgbColor((LightThemeColors.BLUE400 & 0x00FFFFFF) | 0x33000000);
        colors[(int) ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        colors[(int) ImGuiCol.NavHighlight] = RgbColor((LightThemeColors.GRAY900 & 0x00FFFFFF) | 0x0A000000);
        colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
    }

    public static void StyleColorsSpectrumDark() {
        ImGuiStyle* style = ImGui.GetStyle();
        SetStyleMethod(style);

        Vector4* colors = &style->Colors_0;
        colors[(int) ImGuiCol.Text] = RgbColor(DarkThemeColors.GRAY800); // text on hovered controls is gray900
        colors[(int) ImGuiCol.TextDisabled] = RgbColor(DarkThemeColors.GRAY500);
        colors[(int) ImGuiCol.WindowBg] = RgbColor(DarkThemeColors.GRAY100);
        colors[(int) ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.PopupBg] = RgbColor(DarkThemeColors.GRAY50); // not sure about this. Note: applies to tooltips too.
        colors[(int) ImGuiCol.Border] = RgbColor(DarkThemeColors.GRAY300);
        colors[(int) ImGuiCol.BorderShadow] = RgbColor(StaticColors.NONE); // We don't want shadows. Ever.
        colors[(int) ImGuiCol.FrameBg] = RgbColor(DarkThemeColors.GRAY75); // this isnt right, spectrum does not do this, but it's a good fallback
        colors[(int) ImGuiCol.FrameBgHovered] = RgbColor(DarkThemeColors.GRAY50);
        colors[(int) ImGuiCol.FrameBgActive] = RgbColor(DarkThemeColors.GRAY200);
        colors[(int) ImGuiCol.TitleBg] = RgbColor(DarkThemeColors.GRAY300); // those titlebar values are totally made up, spectrum does not have this.
        colors[(int) ImGuiCol.TitleBgActive] = RgbColor(DarkThemeColors.GRAY200);
        colors[(int) ImGuiCol.TitleBgCollapsed] = RgbColor(DarkThemeColors.GRAY400);
        colors[(int) ImGuiCol.MenuBarBg] = RgbColor(DarkThemeColors.GRAY100);
        colors[(int) ImGuiCol.ScrollbarBg] = RgbColor(DarkThemeColors.GRAY100); // same as regular background
        colors[(int) ImGuiCol.ScrollbarGrab] = RgbColor(DarkThemeColors.GRAY400);
        colors[(int) ImGuiCol.ScrollbarGrabHovered] = RgbColor(DarkThemeColors.GRAY600);
        colors[(int) ImGuiCol.ScrollbarGrabActive] = RgbColor(DarkThemeColors.GRAY700);
        colors[(int) ImGuiCol.CheckMark] = RgbColor(DarkThemeColors.BLUE500);
        colors[(int) ImGuiCol.SliderGrab] = RgbColor(DarkThemeColors.GRAY700);
        colors[(int) ImGuiCol.SliderGrabActive] = RgbColor(DarkThemeColors.GRAY800);
        colors[(int) ImGuiCol.Button] = RgbColor(DarkThemeColors.GRAY75); // match default button to Spectrum's 'Action Button'.
        colors[(int) ImGuiCol.ButtonHovered] = RgbColor(DarkThemeColors.GRAY50);
        colors[(int) ImGuiCol.ButtonActive] = RgbColor(DarkThemeColors.GRAY200);
        colors[(int) ImGuiCol.Header] = RgbColor(DarkThemeColors.BLUE400);
        colors[(int) ImGuiCol.HeaderHovered] = RgbColor(DarkThemeColors.BLUE500);
        colors[(int) ImGuiCol.HeaderActive] = RgbColor(DarkThemeColors.BLUE600);
        colors[(int) ImGuiCol.Separator] = RgbColor(DarkThemeColors.GRAY400);
        colors[(int) ImGuiCol.SeparatorHovered] = RgbColor(DarkThemeColors.GRAY600);
        colors[(int) ImGuiCol.SeparatorActive] = RgbColor(DarkThemeColors.GRAY700);
        colors[(int) ImGuiCol.ResizeGrip] = RgbColor(DarkThemeColors.GRAY400);
        colors[(int) ImGuiCol.ResizeGripHovered] = RgbColor(DarkThemeColors.GRAY600);
        colors[(int) ImGuiCol.ResizeGripActive] = RgbColor(DarkThemeColors.GRAY700);
        colors[(int) ImGuiCol.PlotLines] = RgbColor(DarkThemeColors.BLUE400);
        colors[(int) ImGuiCol.PlotLinesHovered] = RgbColor(DarkThemeColors.BLUE600);
        colors[(int) ImGuiCol.PlotHistogram] = RgbColor(DarkThemeColors.BLUE400);
        colors[(int) ImGuiCol.PlotHistogramHovered] = RgbColor(DarkThemeColors.BLUE600);
        colors[(int) ImGuiCol.TextSelectedBg] = RgbColor((DarkThemeColors.BLUE400 & 0x00FFFFFF) | 0x33000000);
        colors[(int) ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        colors[(int) ImGuiCol.NavHighlight] = RgbColor((DarkThemeColors.GRAY900 & 0x00FFFFFF) | 0x0A000000);
        colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
    }
    
    private static void SetStyleMethod(ImGuiStyle* style) {
        style->GrabRounding = 4f;
        style->WindowRounding = 6f;
        
        style->FrameBorderSize = 1.5f;
        style->FrameRounding = 4f;
    }

    public static class StaticColors {
        public static readonly uint NONE = 0x00000000; // transparent
        public static readonly uint WHITE = AddAlpha255(0xFFFFFF);
        public static readonly uint BLACK = AddAlpha255(0x000000);
        public static readonly uint GRAY200 = AddAlpha255(0xF4F4F4);
        public static readonly uint GRAY300 = AddAlpha255(0xEAEAEA);
        public static readonly uint GRAY400 = AddAlpha255(0xD3D3D3);
        public static readonly uint GRAY500 = AddAlpha255(0xBCBCBC);
        public static readonly uint GRAY600 = AddAlpha255(0x959595);
        public static readonly uint GRAY700 = AddAlpha255(0x767676);
        public static readonly uint GRAY800 = AddAlpha255(0x505050);
        public static readonly uint GRAY900 = AddAlpha255(0x323232);
        public static readonly uint BLUE400 = AddAlpha255(0x378EF0);
        public static readonly uint BLUE500 = AddAlpha255(0x2680EB);
        public static readonly uint BLUE600 = AddAlpha255(0x1473E6);
        public static readonly uint BLUE700 = AddAlpha255(0x0D66D0);
        public static readonly uint RED400 = AddAlpha255(0xEC5B62);
        public static readonly uint RED500 = AddAlpha255(0xE34850);
        public static readonly uint RED600 = AddAlpha255(0xD7373F);
        public static readonly uint RED700 = AddAlpha255(0xC9252D);
        public static readonly uint ORANGE400 = AddAlpha255(0xF29423);
        public static readonly uint ORANGE500 = AddAlpha255(0xE68619);
        public static readonly uint ORANGE600 = AddAlpha255(0xDA7B11);
        public static readonly uint ORANGE700 = AddAlpha255(0xCB6F10);
        public static readonly uint GREEN400 = AddAlpha255(0x33AB84);
        public static readonly uint GREEN500 = AddAlpha255(0x2D9D78);
        public static readonly uint GREEN600 = AddAlpha255(0x268E6C);
        public static readonly uint GREEN700 = AddAlpha255(0x12805C);
    }

    public static class LightThemeColors {
        public static readonly uint GRAY50 = AddAlpha255(0xFFFFFF);
        public static readonly uint GRAY75 = AddAlpha255(0xFAFAFA);
        public static readonly uint GRAY100 = AddAlpha255(0xF5F5F5);
        public static readonly uint GRAY200 = AddAlpha255(0xEAEAEA);
        public static readonly uint GRAY300 = AddAlpha255(0xE1E1E1);
        public static readonly uint GRAY400 = AddAlpha255(0xCACACA);
        public static readonly uint GRAY500 = AddAlpha255(0xB3B3B3);
        public static readonly uint GRAY600 = AddAlpha255(0x8E8E8E);
        public static readonly uint GRAY700 = AddAlpha255(0x707070);
        public static readonly uint GRAY800 = AddAlpha255(0x4B4B4B);
        public static readonly uint GRAY900 = AddAlpha255(0x2C2C2C);
        public static readonly uint BLUE400 = AddAlpha255(0x2680EB);
        public static readonly uint BLUE500 = AddAlpha255(0x1473E6);
        public static readonly uint BLUE600 = AddAlpha255(0x0D66D0);
        public static readonly uint BLUE700 = AddAlpha255(0x095ABA);
        public static readonly uint RED400 = AddAlpha255(0xE34850);
        public static readonly uint RED500 = AddAlpha255(0xD7373F);
        public static readonly uint RED600 = AddAlpha255(0xC9252D);
        public static readonly uint RED700 = AddAlpha255(0xBB121A);
        public static readonly uint ORANGE400 = AddAlpha255(0xE68619);
        public static readonly uint ORANGE500 = AddAlpha255(0xDA7B11);
        public static readonly uint ORANGE600 = AddAlpha255(0xCB6F10);
        public static readonly uint ORANGE700 = AddAlpha255(0xBD640D);
        public static readonly uint GREEN400 = AddAlpha255(0x2D9D78);
        public static readonly uint GREEN500 = AddAlpha255(0x268E6C);
        public static readonly uint GREEN600 = AddAlpha255(0x12805C);
        public static readonly uint GREEN700 = AddAlpha255(0x107154);
        public static readonly uint INDIGO400 = AddAlpha255(0x6767EC);
        public static readonly uint INDIGO500 = AddAlpha255(0x5C5CE0);
        public static readonly uint INDIGO600 = AddAlpha255(0x5151D3);
        public static readonly uint INDIGO700 = AddAlpha255(0x4646C6);
        public static readonly uint CELERY400 = AddAlpha255(0x44B556);
        public static readonly uint CELERY500 = AddAlpha255(0x3DA74E);
        public static readonly uint CELERY600 = AddAlpha255(0x379947);
        public static readonly uint CELERY700 = AddAlpha255(0x318B40);
        public static readonly uint MAGENTA400 = AddAlpha255(0xD83790);
        public static readonly uint MAGENTA500 = AddAlpha255(0xCE2783);
        public static readonly uint MAGENTA600 = AddAlpha255(0xBC1C74);
        public static readonly uint MAGENTA700 = AddAlpha255(0xAE0E66);
        public static readonly uint YELLOW400 = AddAlpha255(0xDFBF00);
        public static readonly uint YELLOW500 = AddAlpha255(0xD2B200);
        public static readonly uint YELLOW600 = AddAlpha255(0xC4A600);
        public static readonly uint YELLOW700 = AddAlpha255(0xB79900);
        public static readonly uint FUCHSIA400 = AddAlpha255(0xC038CC);
        public static readonly uint FUCHSIA500 = AddAlpha255(0xB130BD);
        public static readonly uint FUCHSIA600 = AddAlpha255(0xA228AD);
        public static readonly uint FUCHSIA700 = AddAlpha255(0x93219E);
        public static readonly uint SEAFOAM400 = AddAlpha255(0x1B959A);
        public static readonly uint SEAFOAM500 = AddAlpha255(0x16878C);
        public static readonly uint SEAFOAM600 = AddAlpha255(0x0F797D);
        public static readonly uint SEAFOAM700 = AddAlpha255(0x096C6F);
        public static readonly uint CHARTREUSE400 = AddAlpha255(0x85D044);
        public static readonly uint CHARTREUSE500 = AddAlpha255(0x7CC33F);
        public static readonly uint CHARTREUSE600 = AddAlpha255(0x73B53A);
        public static readonly uint CHARTREUSE700 = AddAlpha255(0x6AA834);
        public static readonly uint PURPLE400 = AddAlpha255(0x9256D9);
        public static readonly uint PURPLE500 = AddAlpha255(0x864CCC);
        public static readonly uint PURPLE600 = AddAlpha255(0x7A42BF);
        public static readonly uint PURPLE700 = AddAlpha255(0x6F38B1);
    }

    public static class DarkThemeColors {
        public static readonly uint GRAY50 = AddAlpha255(0x252525);
        public static readonly uint GRAY75 = AddAlpha255(0x2F2F2F);
        public static readonly uint GRAY100 = AddAlpha255(0x323232);
        public static readonly uint GRAY200 = AddAlpha255(0x393939);
        public static readonly uint GRAY300 = AddAlpha255(0x3E3E3E);
        public static readonly uint GRAY400 = AddAlpha255(0x4D4D4D);
        public static readonly uint GRAY500 = AddAlpha255(0x5C5C5C);
        public static readonly uint GRAY600 = AddAlpha255(0x7B7B7B);
        public static readonly uint GRAY700 = AddAlpha255(0x999999);
        public static readonly uint GRAY800 = AddAlpha255(0xCDCDCD);
        public static readonly uint GRAY900 = AddAlpha255(0xFFFFFF);
        public static readonly uint BLUE400 = AddAlpha255(0x2680EB);
        public static readonly uint BLUE500 = AddAlpha255(0x378EF0);
        public static readonly uint BLUE600 = AddAlpha255(0x4B9CF5);
        public static readonly uint BLUE700 = AddAlpha255(0x5AA9FA);
        public static readonly uint RED400 = AddAlpha255(0xE34850);
        public static readonly uint RED500 = AddAlpha255(0xEC5B62);
        public static readonly uint RED600 = AddAlpha255(0xF76D74);
        public static readonly uint RED700 = AddAlpha255(0xFF7B82);
        public static readonly uint ORANGE400 = AddAlpha255(0xE68619);
        public static readonly uint ORANGE500 = AddAlpha255(0xF29423);
        public static readonly uint ORANGE600 = AddAlpha255(0xF9A43F);
        public static readonly uint ORANGE700 = AddAlpha255(0xFFB55B);
        public static readonly uint GREEN400 = AddAlpha255(0x2D9D78);
        public static readonly uint GREEN500 = AddAlpha255(0x33AB84);
        public static readonly uint GREEN600 = AddAlpha255(0x39B990);
        public static readonly uint GREEN700 = AddAlpha255(0x3FC89C);
        public static readonly uint INDIGO400 = AddAlpha255(0x6767EC);
        public static readonly uint INDIGO500 = AddAlpha255(0x7575F1);
        public static readonly uint INDIGO600 = AddAlpha255(0x8282F6);
        public static readonly uint INDIGO700 = AddAlpha255(0x9090FA);
        public static readonly uint CELERY400 = AddAlpha255(0x44B556);
        public static readonly uint CELERY500 = AddAlpha255(0x4BC35F);
        public static readonly uint CELERY600 = AddAlpha255(0x51D267);
        public static readonly uint CELERY700 = AddAlpha255(0x58E06F);
        public static readonly uint MAGENTA400 = AddAlpha255(0xD83790);
        public static readonly uint MAGENTA500 = AddAlpha255(0xE2499D);
        public static readonly uint MAGENTA600 = AddAlpha255(0xEC5AAA);
        public static readonly uint MAGENTA700 = AddAlpha255(0xF56BB7);
        public static readonly uint YELLOW400 = AddAlpha255(0xDFBF00);
        public static readonly uint YELLOW500 = AddAlpha255(0xEDCC00);
        public static readonly uint YELLOW600 = AddAlpha255(0xFAD900);
        public static readonly uint YELLOW700 = AddAlpha255(0xFFE22E);
        public static readonly uint FUCHSIA400 = AddAlpha255(0xC038CC);
        public static readonly uint FUCHSIA500 = AddAlpha255(0xCF3EDC);
        public static readonly uint FUCHSIA600 = AddAlpha255(0xD951E5);
        public static readonly uint FUCHSIA700 = AddAlpha255(0xE366EF);
        public static readonly uint SEAFOAM400 = AddAlpha255(0x1B959A);
        public static readonly uint SEAFOAM500 = AddAlpha255(0x20A3A8);
        public static readonly uint SEAFOAM600 = AddAlpha255(0x23B2B8);
        public static readonly uint SEAFOAM700 = AddAlpha255(0x26C0C7);
        public static readonly uint CHARTREUSE400 = AddAlpha255(0x85D044);
        public static readonly uint CHARTREUSE500 = AddAlpha255(0x8EDE49);
        public static readonly uint CHARTREUSE600 = AddAlpha255(0x9BEC54);
        public static readonly uint CHARTREUSE700 = AddAlpha255(0xA3F858);
        public static readonly uint PURPLE400 = AddAlpha255(0x9256D9);
        public static readonly uint PURPLE500 = AddAlpha255(0x9D64E1);
        public static readonly uint PURPLE600 = AddAlpha255(0xA873E9);
        public static readonly uint PURPLE700 = AddAlpha255(0xB483F0);
    }

    private static uint AddAlpha255(uint color) {
        // add alpha.
        // also swap red and blue channel for some reason.
        // todo: figure out why, and fix it.
        byte a = 0xFF;
        byte r = (byte) (color >> 16);
        byte g = (byte) (color >> 8);
        byte b = (byte) (color >> 0);

        return (uint) ((a << 24) | (r << 16) | (g << 8) | (b << 0));
    }

    private static Vector4 RgbColor(uint color) {
        byte a = (byte) (color >> 24);
        byte r = (byte) (color >> 16);
        byte g = (byte) (color >> 8);
        byte b = (byte) (color >> 0);

        return new Vector4(r, g, b, a) * (1f / Byte.MaxValue);
    }
}
