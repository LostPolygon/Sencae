namespace Sencae.Ppu; 

public unsafe struct PpuObjectPalette {
    public const int ColorCount = 4;
    
    public fixed byte ColorIndices[ColorCount];
}
