namespace Sencae.Ppu;

[Flags]
public enum PpuStatusRegisterFlags : byte {
    // PPU open bus. Returns stale PPU bus contents.
    StatusMask = 0b_0001_1111,

    /*
    Sprite overflow. The intent was for this flag to be set
    whenever more than eight sprites appear on a scanline, but a
    hardware bug causes the actual behavior to be more complicated
    and generate false positives as well as false negatives; see
    PPU sprite evaluation. This flag is set during sprite
    evaluation and cleared at dot 1 (the second dot) of the
    pre-render line.
    */
    SpriteOverflow = 1 << 5,

    /*
    Sprite 0 Hit.  Set when a nonzero pixel of sprite 0 overlaps a nonzero background pixel;
    cleared at dot 1 of the pre-render line. Used for raster timing.
    */
    Sprite0Hit = 1 << 6,

    /*
    Vertical blank has started (0: not in vblank; 1: in vblank).
    Set at dot 1 of line 241 (the line *after* the post-render line);
    cleared after reading $2002 and at dot 1 of the pre-render line.
    */
    VerticalBlankStarted = 1 << 7,
}
