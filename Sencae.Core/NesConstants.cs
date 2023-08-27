namespace Sencae;

public static class NesConstants {
    public static readonly NesVariantConstants Pal = new NesVariantConstants {
        Framerate = 60,
        ScreenWidth = 256,
        ScreenHeight = 224,
        CyclesPerFrame = 29780
    };

    public static readonly NesVariantConstants Ntsc = new NesVariantConstants {
        Framerate = 50,
        ScreenWidth = 256,
        ScreenHeight = 240,
        CyclesPerFrame = 33248
    };

    public class NesVariantConstants {
        public required int Framerate { get; init; }
        public required int ScreenWidth { get; init; }
        public required int ScreenHeight { get; init; }
        public required int CyclesPerFrame { get; init; }
    }
}
