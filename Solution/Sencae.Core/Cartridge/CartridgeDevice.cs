namespace Sencae;

public record CartridgeDevice(
    CartridgeData CartridgeData,
    CartridgeToCpuBusDevice CartridgeToCpuBusDevice,
    CartridgeToPpuBusDevice CartridgeToPpuBusDevice
);
