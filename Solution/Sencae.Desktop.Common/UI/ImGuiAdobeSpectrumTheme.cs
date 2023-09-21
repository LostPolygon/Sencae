using System.Numerics;
using ImGuiNET;
using static ImGuiNET.ImGuiCol;

namespace Sencae.Desktop;

// https://github.com/adobe/imgui/blob/master/imgui_spectrum.cpp
public static unsafe class ImGuiAdobeSpectrumTheme {
    public static void StyleColorsSpectrumLight() {
        ImGuiStyle* style = ImGui.GetStyle();
        SetStyleMethod(style);

        Vector4* colors = &style->Colors_0;
        colors[(int) Text] = RgbColor(LightColors.GRAY800); // text on hovered controls is gray900
        colors[(int) TextDisabled] = RgbColor(LightColors.GRAY500);
        colors[(int) WindowBg] = RgbColor(LightColors.GRAY100);
        colors[(int) ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) PopupBg] = RgbColor(LightColors.GRAY50); // not sure about this. Note: applies to tooltips too.
        colors[(int) Border] = RgbColor(LightColors.GRAY300);
        colors[(int) BorderShadow] = RgbColor(StaticColors.NONE); // We don't want shadows. Ever.
        colors[(int) FrameBg] = RgbColor(LightColors.GRAY75); // this isnt right, spectrum does not do this, but it's a good fallback
        colors[(int) FrameBgHovered] = RgbColor(LightColors.GRAY50);
        colors[(int) FrameBgActive] = RgbColor(LightColors.GRAY200);
        colors[(int) TitleBg] = RgbColor(LightColors.GRAY300); // those titlebar values are totally made up, spectrum does not have this.
        colors[(int) TitleBgActive] = RgbColor(LightColors.GRAY200);
        colors[(int) TitleBgCollapsed] = RgbColor(LightColors.GRAY400);
        colors[(int) MenuBarBg] = RgbColor(LightColors.GRAY100);
        colors[(int) ScrollbarBg] = RgbColor(LightColors.GRAY100); // same as regular background
        colors[(int) ScrollbarGrab] = RgbColor(LightColors.GRAY400);
        colors[(int) ScrollbarGrabHovered] = RgbColor(LightColors.GRAY600);
        colors[(int) ScrollbarGrabActive] = RgbColor(LightColors.GRAY700);
        colors[(int) CheckMark] = RgbColor(LightColors.BLUE500);
        colors[(int) SliderGrab] = RgbColor(LightColors.GRAY700);
        colors[(int) SliderGrabActive] = RgbColor(LightColors.GRAY800);
        colors[(int) Button] = RgbColor(LightColors.GRAY75); // match default button to Spectrum's 'Action Button'.
        colors[(int) ButtonHovered] = RgbColor(LightColors.GRAY50);
        colors[(int) ButtonActive] = RgbColor(LightColors.GRAY200);
        colors[(int) Header] = RgbColor(LightColors.BLUE400);
        colors[(int) HeaderHovered] = RgbColor(LightColors.BLUE500);
        colors[(int) HeaderActive] = RgbColor(LightColors.BLUE600);
        colors[(int) Separator] = RgbColor(LightColors.GRAY400);
        colors[(int) SeparatorHovered] = RgbColor(LightColors.GRAY600);
        colors[(int) SeparatorActive] = RgbColor(LightColors.GRAY700);
        colors[(int) ResizeGrip] = RgbColor(LightColors.GRAY400);
        colors[(int) ResizeGripHovered] = RgbColor(LightColors.GRAY600);
        colors[(int) ResizeGripActive] = RgbColor(LightColors.GRAY700);
        colors[(int) PlotLines] = RgbColor(LightColors.BLUE400);
        colors[(int) PlotLinesHovered] = RgbColor(LightColors.BLUE600);
        colors[(int) PlotHistogram] = RgbColor(LightColors.BLUE400);
        colors[(int) PlotHistogramHovered] = RgbColor(LightColors.BLUE600);
        colors[(int) TextSelectedBg] = RgbColor((LightColors.BLUE400 & 0x00FFFFFF) | 0x33000000);
        colors[(int) DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        colors[(int) NavHighlight] = RgbColor((LightColors.GRAY900 & 0x00FFFFFF) | 0x0A000000);
        colors[(int) NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int) NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int) ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
    }

    public static void StyleColorsSpectrumDark() {
        ImGuiStyle* style = ImGui.GetStyle();
        SetStyleMethod(style);

        Vector4* colors = &style->Colors_0;
        colors[(int) Text] = RgbColor(DarkColors.GRAY800); // text on hovered controls is gray900
        colors[(int) TextDisabled] = RgbColor(DarkColors.GRAY500);
        colors[(int) WindowBg] = RgbColor(DarkColors.GRAY100);
        colors[(int) ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) PopupBg] = RgbColor(DarkColors.GRAY50); // not sure about this. Note: applies to tooltips too.
        colors[(int) Border] = RgbColor(DarkColors.GRAY300);
        colors[(int) BorderShadow] = RgbColor(StaticColors.NONE); // We don't want shadows. Ever.
        colors[(int) FrameBg] = RgbColor(DarkColors.GRAY75); // this isnt right, spectrum does not do this, but it's a good fallback
        colors[(int) FrameBgHovered] = RgbColor(DarkColors.GRAY50);
        colors[(int) FrameBgActive] = RgbColor(DarkColors.GRAY200);
        colors[(int) TitleBg] = RgbColor(DarkColors.GRAY300); // those titlebar values are totally made up, spectrum does not have this.
        colors[(int) TitleBgActive] = RgbColor(DarkColors.GRAY200);
        colors[(int) TitleBgCollapsed] = RgbColor(DarkColors.GRAY400);
        colors[(int) MenuBarBg] = RgbColor(DarkColors.GRAY100);
        colors[(int) ScrollbarBg] = RgbColor(DarkColors.GRAY100); // same as regular background
        colors[(int) ScrollbarGrab] = RgbColor(DarkColors.GRAY400);
        colors[(int) ScrollbarGrabHovered] = RgbColor(DarkColors.GRAY600);
        colors[(int) ScrollbarGrabActive] = RgbColor(DarkColors.GRAY700);
        colors[(int) CheckMark] = RgbColor(DarkColors.BLUE500);
        colors[(int) SliderGrab] = RgbColor(DarkColors.GRAY700);
        colors[(int) SliderGrabActive] = RgbColor(DarkColors.GRAY800);
        colors[(int) Button] = RgbColor(DarkColors.GRAY75); // match default button to Spectrum's 'Action Button'.
        colors[(int) ButtonHovered] = RgbColor(DarkColors.GRAY50);
        colors[(int) ButtonActive] = RgbColor(DarkColors.GRAY200);
        colors[(int) Header] = RgbColor(DarkColors.BLUE400);
        colors[(int) HeaderHovered] = RgbColor(DarkColors.BLUE500);
        colors[(int) HeaderActive] = RgbColor(DarkColors.BLUE600);
        colors[(int) Separator] = RgbColor(DarkColors.GRAY400);
        colors[(int) SeparatorHovered] = RgbColor(DarkColors.GRAY600);
        colors[(int) SeparatorActive] = RgbColor(DarkColors.GRAY700);
        colors[(int) ResizeGrip] = RgbColor(DarkColors.GRAY400);
        colors[(int) ResizeGripHovered] = RgbColor(DarkColors.GRAY600);
        colors[(int) ResizeGripActive] = RgbColor(DarkColors.GRAY700);
        colors[(int) PlotLines] = RgbColor(DarkColors.BLUE400);
        colors[(int) PlotLinesHovered] = RgbColor(DarkColors.BLUE600);
        colors[(int) PlotHistogram] = RgbColor(DarkColors.BLUE400);
        colors[(int) PlotHistogramHovered] = RgbColor(DarkColors.BLUE600);
        colors[(int) TextSelectedBg] = RgbColor((DarkColors.BLUE400 & 0x00FFFFFF) | 0x33000000);
        colors[(int) DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        colors[(int) NavHighlight] = RgbColor((DarkColors.GRAY900 & 0x00FFFFFF) | 0x0A000000);
        colors[(int) NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int) NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int) ModalWindowDimBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.35f);
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

    public static class LightColors {
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

    public static class DarkColors {
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
        const byte a = 0xFF;
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
