namespace NGChip8Emulator;

public record CartridgeData(
    byte[] PrgRom,
    byte[] ChrRom
);
