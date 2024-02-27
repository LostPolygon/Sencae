using System.Drawing;

namespace Sencae.Desktop;

public static class ColorUtility {
    public static float CalculateWcagContrast(Color text, Color background) {
        float textLuminance = CalculateLuminance(text);
        float backgroundLuminance = CalculateLuminance(background);

        float L1 = MathF.Max(textLuminance, backgroundLuminance);
        float L2 = MathF.Min(textLuminance, backgroundLuminance);

        return (L1 + 0.05f) / (L2 + 0.05f);
    }

    private static float CalculateLuminance(Color color) {
        static float ComponentLuminanceByte(byte value) =>
            ComponentLuminance(value / (float) Byte.MaxValue);

        static float ComponentLuminance(float value) {
            if (value <= 0.04045f) {
                return value / 12.92f;
            }

            return MathF.Pow((value + 0.055f) / 1.055f, 2.4f);
        }

        float L =
            0.2126f * ComponentLuminanceByte(color.R) +
            0.7152f * ComponentLuminanceByte(color.G) +
            0.0722f * ComponentLuminanceByte(color.B);

        return L;
    }
    
    public static uint ColorAsAbgrUint(byte r, byte g, byte b, byte a) {
        return (uint) ((a << 24) | (b << 16) | (g << 8) | r);
    }

    public static uint ArgbToAbgr(uint color) {
        const uint RMask = 0x00FF0000;
        const uint BMask = 0x000000FF;
        const uint AGMask = ~RMask & ~BMask;
        
        uint r = (color & RMask) >> 16;
        uint b = color & BMask;
        return (color & AGMask) | (b << 16) | r;
    }
}
