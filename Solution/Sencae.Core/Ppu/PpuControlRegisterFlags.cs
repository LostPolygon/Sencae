namespace Sencae.Ppu;

[Flags]
public enum PpuControlRegisterFlags : byte {
    BaseNametableAddressMask = 1 << 0 | 1 << 1, // Base nametable address (0 = $2000; 1 = $2400; 2 = $2800; 3 = $2C00)
    VramAddressIncrement = 1 << 2, // VRAM address increment per CPU read/write of PPUDATA (0: add 1, going across; 1: add 32, going down)
    SpritePatternTableAddressFor8x8Sprites = 1 << 3, // Sprite pattern table address for 8x8 sprites (0: $0000; 1: $1000)
    BackgroundPatternTableAddress = 1 << 4, // Background pattern table address (0: $0000; 1: $1000)
    SpriteSize = 1 << 5, // Sprite size (0: 8x8 pixels; 1: 8x16 pixels)
    MasterSlaveSelect = 1 << 6, // PPU master/slave select (0: read backdrop from EXT pins; 1: output color on EXT pins)
    GenerateNmi = 1 << 7, // Generate an NMI at the start of the vertical blanking interval (0: off; 1: on)
}