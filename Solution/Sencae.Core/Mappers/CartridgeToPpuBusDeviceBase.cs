using System;
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

    public void Connect(Machine machine) {
        Machine = machine;
        PpuBusDevice = machine.PpuBusDevice;
        PpuMemory = new(machine.State.Ppu.Memory);
    }
    
    public void Reset() {
        ChrRamMemory?.AsSpan().Clear();
    }
    
    void IDisposable.Dispose() {
        PpuMemory.Dispose();
        PpuMemory = default;
    }
        
    [MethodImpl(HotPath)]
    public abstract bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus);

    [MethodImpl(HotPath)]
    protected bool HandlePpuBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        return PpuBusDevice.HandleBus(address, accessMode, ref bus);
    } 
    
    [MethodImpl(HotPath)]
    protected bool HandleChrRomOrRamIndexed(int arrayIndex, MemoryAccessMode accessMode, ref byte bus) {
        if (ChrRomMemory != null)
            return BusDeviceHandlers.ReadOnlyIndexed(ChrRomMemory.Value, arrayIndex, accessMode, ref bus);

        if (ChrRamMemory != null)
            return BusDeviceHandlers.ReadWriteIndexed(ChrRamMemory.Value, arrayIndex, accessMode, ref bus);

        Debug.Assert(ChrRomMemory != null || ChrRamMemory != null, "ChrRomMemory != null || ChrRamMemory != null");
        return false;
    } 
}
