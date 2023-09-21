using System;

namespace Sencae;

public record CartridgeDevice(
    CartridgeData CartridgeData,
    CartridgeToCpuBusDevice CartridgeToCpuBusDevice,
    CartridgeToPpuBusDevice CartridgeToPpuBusDevice
) : IDisposable {
    public void Dispose() {
        CartridgeToCpuBusDevice.Dispose();
        CartridgeToPpuBusDevice.Dispose();
    }
}
