using System.Runtime.CompilerServices;

namespace Sencae.Mappers;

public abstract class CartridgeToCpuBusDeviceBase(byte[] prgRom, byte[]? prgRam) : IMapperDevice {
    protected readonly DirectArray<byte> PrgRomMemory = new(prgRom, 0x8000);
    protected readonly DirectArray<byte>? PrgRamMemory = DirectArray.FromNullable(prgRam, 0x6000);

    [MethodImpl(HotPath)]
    public abstract bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus);

    public virtual void Reset() {
        PrgRamMemory?.AsSpan().Clear();
    }

    public virtual void Dispose() {
        PrgRomMemory.Dispose();
        PrgRamMemory?.Dispose();
    }
}
