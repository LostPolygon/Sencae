using System;

namespace Sencae; 

public static class CartridgeDeviceFactory {
    public static CartridgeDevice Create(CartridgeData cartridgeData, byte[]? prgRam = null) {
        prgRam ??= new byte[8 * 1024];
        
        switch (cartridgeData.Mapper) {
            case 0:
                return new CartridgeDevice(
                    cartridgeData,
                    new CartridgeToCpuBusDevice(cartridgeData.PrgRom, prgRam),
                    new CartridgeToPpuBusDevice(cartridgeData.ChrRom)
                );
            default:
                throw new NotImplementedException($"Mapper {cartridgeData.Mapper} is not supported");
        }
    }
}
