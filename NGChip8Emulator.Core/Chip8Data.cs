using System.Linq;

namespace NGChip8Emulator {
    public static class Chip8Data {
        public const int ScreenWidth = 64;
        public const int ScreenHeight = 32;

        public const int Framerate = 60;
        public const int InstructionPerFrame = 8;
        public const int MemorySize = 4096;
        public const int ProgramStartAddress = 0x200;

        // 0x050-0x0A0 - Used for the built in 4x5 pixel font set (0-F)
        public const int HexDigitFontStartAddress = 0x50;

        public static byte[] HexDigitFontSprite0 = {
            0b_11110000,
            0b_10010000,
            0b_10010000,
            0b_10010000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSprite1 = {
            0b_00100000,
            0b_01100000,
            0b_00100000,
            0b_00100000,
            0b_01110000,
        };

        public static byte[] HexDigitFontSprite2 = {
            0b_11110000,
            0b_00010000,
            0b_11110000,
            0b_10000000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSprite3 = {
            0b_11110000,
            0b_00010000,
            0b_11110000,
            0b_00010000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSprite4 = {
            0b_10010000,
            0b_10010000,
            0b_11110000,
            0b_00010000,
            0b_00010000,
        };

        public static byte[] HexDigitFontSprite5 = {
            0b_11110000,
            0b_10000000,
            0b_11110000,
            0b_00010000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSprite6 = {
            0b_11110000,
            0b_10000000,
            0b_11110000,
            0b_10010000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSprite7 = {
            0b_11110000,
            0b_00010000,
            0b_00100000,
            0b_01000000,
            0b_01000000,
        };

        public static byte[] HexDigitFontSprite8 = {
            0b_11110000,
            0b_10010000,
            0b_11110000,
            0b_10010000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSprite9 = {
            0b_11110000,
            0b_10010000,
            0b_11110000,
            0b_00010000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSpriteA = {
            0b_11110000,
            0b_10010000,
            0b_11110000,
            0b_10010000,
            0b_10010000,
        };

        public static byte[] HexDigitFontSpriteB = {
            0b_11100000,
            0b_10010000,
            0b_11100000,
            0b_10010000,
            0b_11100000,
        };

        public static byte[] HexDigitFontSpriteC = {
            0b_11110000,
            0b_10000000,
            0b_10000000,
            0b_10000000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSpriteD = {
            0b_11100000,
            0b_10010000,
            0b_10010000,
            0b_10010000,
            0b_11100000,
        };

        public static byte[] HexDigitFontSpriteE = {
            0b_11110000,
            0b_10000000,
            0b_11110000,
            0b_10000000,
            0b_11110000,
        };

        public static byte[] HexDigitFontSpriteF = {
            0b_11110000,
            0b_10000000,
            0b_11110000,
            0b_10000000,
            0b_10000000,
        };

        public static byte[] HexDigitFontData =
            new byte[0]
                .Concat(HexDigitFontSprite0)
                .Concat(HexDigitFontSprite1)
                .Concat(HexDigitFontSprite2)
                .Concat(HexDigitFontSprite3)
                .Concat(HexDigitFontSprite4)
                .Concat(HexDigitFontSprite5)
                .Concat(HexDigitFontSprite6)
                .Concat(HexDigitFontSprite7)
                .Concat(HexDigitFontSprite8)
                .Concat(HexDigitFontSprite9)
                .Concat(HexDigitFontSpriteA)
                .Concat(HexDigitFontSpriteB)
                .Concat(HexDigitFontSpriteC)
                .Concat(HexDigitFontSpriteD)
                .Concat(HexDigitFontSpriteE)
                .Concat(HexDigitFontSpriteF)
                .ToArray();

    }
}
