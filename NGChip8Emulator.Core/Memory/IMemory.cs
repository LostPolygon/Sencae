namespace NGChip8Emulator; 


public interface IMemory {
    byte ReadU8(ushort address);
    void WriteU8(ushort address, byte value);

    ref byte GetU8(ushort address);
    ref ushort GetU16(ushort address);
}