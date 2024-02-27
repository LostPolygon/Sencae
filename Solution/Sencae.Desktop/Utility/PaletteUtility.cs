namespace Sencae.Desktop;

public static class PaletteUtility {
    public static void GetGreyscalePalette(Span<uint> destination, byte alpha) {
        float step = (float) Byte.MaxValue / (destination.Length - 1);
        
        for (int i = 0; i < destination.Length; i++) {
            byte value = (byte) (i * step);
            uint color = ColorUtility.ColorAsAbgrUint(value, value, value, alpha);
            destination[i] = color;
        }
    }
    
    /*
    private static Span<uint> GetGreyscalePalette(int steps, byte alpha) {
        Span<uint> palette = stackalloc uint[steps];
        GetGreyscalePalette(palette, alpha);

        return palette;
    }*/
}
