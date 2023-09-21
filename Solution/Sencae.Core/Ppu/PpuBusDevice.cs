using System;
using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

public class PpuBusDevice : IBusDevice, IDisposable {
    public readonly Machine Machine;

    private readonly DirectArray<byte> _chrRomMemory;
    private readonly DirectArray<byte> _memory;
    private readonly DirectArray<byte> _oamMemory;
    private readonly DirectArray<byte> _palleteMemory;
    private PpuState _state;

    public PpuBusDevice(Machine machine) {
        Machine = machine;

        _state = machine.State.Ppu;
        _chrRomMemory = new DirectArray<byte>(Machine.CartridgeDevice.CartridgeData.ChrRom);
        _memory = new DirectArray<byte>(_state.Memory);
        _oamMemory = new DirectArray<byte>(_state.OamMemory);
        _palleteMemory = new DirectArray<byte>(_state.PalleteMemory);
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: // Pattern tables
                return
                    Machine.CartridgeDevice.CartridgeData.IsChrRomWriteable ?
                        BusDeviceHandlers.ReadWrite(_chrRomMemory, address % 0x2000, accessMode, ref bus) :
                        BusDeviceHandlers.ReadOnly(_chrRomMemory, address % 0x2000, accessMode, ref bus);
            case >= 0x2000 and <= 0x3EFF: // Nametables
                return BusDeviceHandlers.ReadWrite(_memory, address % 0x800, accessMode, ref bus);
            case >= 0x3F00 and <= 0x3FFF: // Palette RAM
                // $3F10/$3F14/$3F18/$3F1C are mirrors of $3F00/$3F04/$3F08/$3F0C
                address = address switch {
                    0x3F10 => 0x3F00,
                    0x3F14 => 0x3F04,
                    0x3F18 => 0x3F08,
                    0x3F1C => 0x3F0C,
                    _ => address
                };
                
                return BusDeviceHandlers.ReadWrite(_palleteMemory, address % 0x20, accessMode, ref bus);
            default:
                return false;
        }
    }

    private void ReleaseUnmanagedResources() {
        _chrRomMemory.Dispose();
        _memory.Dispose();
        _oamMemory.Dispose();
        _palleteMemory.Dispose();
    }

    private void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if (disposing) {
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~PpuBusDevice() {
        Dispose(false);
    }

    [MethodImpl(MethodOptions.Options)]
    public void Write(ushort address, byte data) {
        HandleBus(address, MemoryAccessMode.Write, ref data);
    }

    [MethodImpl(MethodOptions.Options)]
    public byte Read(ushort address) {
        byte data = 0;
        HandleBus(address, MemoryAccessMode.Read, ref data);

        return data;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public byte ReadObserve(ushort address) {
        byte data = 0;
        HandleBus(address, MemoryAccessMode.ReadObserve, ref data);

        return data;
    }
}
