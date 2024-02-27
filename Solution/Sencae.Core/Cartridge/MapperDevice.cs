using System;
using Sencae.Mappers;

namespace Sencae;

public sealed record MapperDevice(
    IMapperDevice CartridgeToCpuBusDevice,
    IMapperDevice CartridgeToPpuBusDevice
) : IDisposable {
    public void Dispose() {
        CartridgeToCpuBusDevice.Dispose();
        CartridgeToPpuBusDevice.Dispose();
    }
}
