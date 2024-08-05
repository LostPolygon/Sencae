using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae.Mappers;

public abstract class CartridgeToPpuBusDeviceBase(byte[]? chrRom, byte[]? chrRam) : IMapperDevice {
    protected readonly DirectArray<byte>? ChrRomMemory = DirectArray.FromNullable(chrRom);
    protected readonly DirectArray<byte>? ChrRamMemory = DirectArray.FromNullable(chrRam);
    protected DirectArray<byte> PpuMemory { get; private set; }
    protected Machine Machine { get; private set; }
    protected PpuBusDevice PpuBusDevice { get; private set; }

    [MethodImpl(HotPath)]
    public abstract bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus);
    
    public virtual void Connect(Machine machine) {
        Machine = machine;
        PpuBusDevice = machine.PpuBusDevice;
        PpuMemory = new(machine.State.Ppu.Memory);
    }

    public virtual void Reset() {
        ChrRamMemory?.AsSpan().Clear();
    }

    public virtual void Dispose() {
        PpuMemory.Dispose();
        PpuMemory = default;
    }

    [MethodImpl(HotPath)]
    protected bool DefaultPpuHandler(int bankedAddress, ushort machineAddress, MemoryAccessMode accessMode, ref byte bus, PpuNametableMirroringType mirroringType) {
        return machineAddress switch {
            >= 0x0000 and <= 0x1FFF => HandleChrRomOrChrRam(bankedAddress, accessMode, ref bus),
            >= 0x2000 and <= 0x3EFF => HandlePpuNametables(machineAddress, accessMode, ref bus, mirroringType),
            _ => HandlePpuBus(machineAddress, accessMode, ref bus)
        };
    }
    
    [MethodImpl(HotPath)]
    protected bool HandlePpuBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        return PpuBusDevice.HandleBus(address, accessMode, ref bus);
    }

    [MethodImpl(HotPath)]
    protected bool HandlePpuNametables(ushort address, MemoryAccessMode accessMode, ref byte bus, PpuNametableMirroringType mirroringType) {
        const int bit10Mask = 0b0000_0100_0000_0000;
        const int bit11Mask = 0b0000_1000_0000_0000;

        switch (mirroringType) {
            case PpuNametableMirroringType.OneScreenBlk0:
            case PpuNametableMirroringType.OneScreenBlk1:
                // TODO
                break;
            case PpuNametableMirroringType.TwoScreenVerticalMirroring:
                break;
            case PpuNametableMirroringType.TwoScreenHorizontalMirroring:
                address = (ushort) ((address & ~(bit11Mask | bit10Mask)) | ((address & bit11Mask) >> 1));
                break;
        }

        return HandlePpuBus(address, accessMode, ref bus);
    }

    [MethodImpl(HotPath)]
    protected bool HandleChrRomOrChrRam(int arrayIndex, MemoryAccessMode accessMode, ref byte bus) {
        if (ChrRomMemory != null)
            return BusDeviceHandlers.ReadOnlyIndexed(ChrRomMemory.Value, arrayIndex, accessMode, ref bus);

        if (ChrRamMemory != null)
            return BusDeviceHandlers.ReadWriteIndexed(ChrRamMemory.Value, arrayIndex, accessMode, ref bus);

        Debug.Assert(ChrRomMemory != null || ChrRamMemory != null, "ChrRomMemory != null || ChrRamMemory != null");
        return false;
    }
}
