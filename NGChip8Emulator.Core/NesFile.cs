namespace NGChip8Emulator;

public record NesFile(
    byte[] PrgRom,
    byte[] ChrRom
);
