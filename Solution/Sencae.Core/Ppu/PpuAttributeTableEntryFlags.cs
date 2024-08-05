namespace Sencae.Ppu;

[Flags]
public enum PpuAttributeTableEntryFlags : byte {
    // Color bits 3-2
    TopLeftQuadrantColorBits = 0b_0000_0011,
    TopRightQuadrantColorBits = 0b_0000_1100,
    BottomLeftQuadrantColorBits = 0b_0011_0000,
    BottomRightQuadrantColorBits = 0b_1100_0000,
}
