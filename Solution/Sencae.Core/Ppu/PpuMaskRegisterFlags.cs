using System;

namespace Sencae.Ppu;

[Flags]
public enum PpuMaskRegisterFlags : byte {
    Greyscale = 1 << 0, // Greyscale (0: normal color, 1: produce a greyscale display)
    Show8LeftmostBackgroundPixels = 1 << 1, // 
    Show8LeftmostSpritePixels = 1 << 2, // 
    ShowBackground = 1 << 3, // 
    ShowSprites = 1 << 4, // 
    EmphasizeRed = 1 << 5, // Emphasize red (green on PAL/Dendy)
    EmphasizeGreen = 1 << 6, // Emphasize green (red on PAL/Dendy)
    EmphasizeBlue = 1 << 7, // Emphasize blue
}
