using Sencae.Mappers;

namespace Sencae;

public static class MapperDeviceFactory {
    public static MapperDevice Create(CartridgeData cartridgeData, byte[]? prgRam = null) {
        prgRam ??= new byte[8 * 1024];

        switch (cartridgeData.Mapper) {
            case 0:
                return new MapperDevice(
                    new Mapper000.CartridgeToCpuBusDevice(cartridgeData.PrgRom, prgRam),
                    new Mapper000.CartridgeToPpuBusDevice(cartridgeData.ChrRom, cartridgeData.ChrRam, cartridgeData.PpuNametableMirroringType)
                );
            case 1: {
                Mapper001.State state = new();
                return new MapperDevice(
                    new Mapper001.CartridgeToCpuBusDevice(state, cartridgeData.PrgRom, prgRam),
                    new Mapper001.CartridgeToPpuBusDevice(state, cartridgeData.ChrRom, cartridgeData.ChrRam)
                );
            }
            case 2: {
                Mapper002.State state = new();
                return new MapperDevice(
                    new Mapper002.CartridgeToCpuBusDevice(state, cartridgeData.PrgRom, prgRam),
                    new Mapper000.CartridgeToPpuBusDevice(cartridgeData.ChrRom, cartridgeData.ChrRam, cartridgeData.PpuNametableMirroringType)
                );
            }
            case 3: {
                Mapper003.State state = new();
                return new MapperDevice(
                    new Mapper003.CartridgeToCpuBusDevice(state, cartridgeData.PrgRom, prgRam),
                    new Mapper003.CartridgeToPpuBusDevice(state, cartridgeData.ChrRom, cartridgeData.ChrRam, cartridgeData.PpuNametableMirroringType)
                );
            }
            default:
                throw new NotImplementedException($"Mapper {cartridgeData.Mapper} is not supported");
        }
    }
}
