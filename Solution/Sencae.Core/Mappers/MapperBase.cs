namespace Sencae.Mappers;

public abstract class MapperBase : IMapperDevice {
    public abstract bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus);
}