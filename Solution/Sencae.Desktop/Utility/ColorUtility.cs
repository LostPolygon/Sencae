using System.Drawing;

namespace Sencae.Desktop;

public static class ColorUtility {
    public static float CalculateWcagContrast(Color text, Color background) {
        float textLuminance = CalculateLuminance(text);
        float backgroundLuminance = CalculateLuminance(background);

        float L1 = textLuminance > backgroundLuminance ? textLuminance : backgroundLuminance;
        float L2 = textLuminance > backgroundLuminance ? backgroundLuminance : textLuminance;

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
            0.2126f * ComponentLuminance(color.R) +
            0.7152f * ComponentLuminance(color.G) +
            0.0722f * ComponentLuminance(color.B);

        return L;
    }
}
