namespace Sencae.Ppu; 

public unsafe struct PpuPalette {
    public const int ColorCount = 64;
    
    public fixed uint Colors[ColorCount];
}
