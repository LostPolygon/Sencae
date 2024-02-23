using System;
using Sencae.Mappers;

namespace Sencae;

public sealed record CartridgeDevice(
    CartridgeData CartridgeData,
    Mapper000.CartridgeToCpuBusDevice CartridgeToCpuBusDevice,
    Mapper000.CartridgeToPpuBusDevice CartridgeToPpuBusDevice
) : IDisposable {
    public void Dispose() {
        CartridgeToCpuBusDevice.Dispose();
        CartridgeToPpuBusDevice.Dispose();
    }
}
