namespace Sencae;

public static class NesConstants {
    public static readonly NesVariantConstants Ntsc = new NesVariantConstants {
        Framerate = 60,
        ScreenWidth = 256,
        ScreenHeight = 240,
        MachineCyclesPerFrame = 29780 * 3
    };

    public static readonly NesVariantConstants Pal = new NesVariantConstants {
        Framerate = 50,
        ScreenWidth = 256,
        ScreenHeight = 240,
        MachineCyclesPerFrame = 33248 * 3
    };

    public class NesVariantConstants {
        public required int Framerate { get; init; }
        public required int ScreenWidth { get; init; }
        public required int ScreenHeight { get; init; }
        public required int MachineCyclesPerFrame { get; init; }
    }
}
