using System;
using Sencae.Mappers;

namespace Sencae;

public static class CartridgeDeviceFactory {
    public static CartridgeDevice Create(CartridgeData cartridgeData, byte[]? prgRam = null) {
        prgRam ??= new byte[8 * 1024];

        return cartridgeData.Mapper switch {
            0 => new CartridgeDevice(
                cartridgeData,
                new Mapper000.CartridgeToCpuBusDevice(cartridgeData.PrgRom, prgRam),
                new Mapper000.CartridgeToPpuBusDevice(cartridgeData.ChrRom)
            ),
            _ => throw new NotImplementedException($"Mapper {cartridgeData.Mapper} is not supported")
        };
    }
}
