using System;
using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

public unsafe class PpuBusDevice : IBusDevice, IDisposable {
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
    public bool HandleBus(ushort address, bool isWrite, ref byte bus) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: { // Pattern tables
                ref byte mappedData = ref _chrRomMemory.Data[address % 0x2000];
                BusDeviceHandlers.ReadOnly(ref mappedData, isWrite, ref bus);
                return true;
            }
            case >= 0x2000 and <= 0x3EFF: { // Nametables
                //ref byte mappedData = ref _memory.Data[address % 0x1000];
                ref byte mappedData = ref _memory.Data[address % 0x800];
                BusDeviceHandlers.ReadWrite(ref mappedData, isWrite, ref bus);
                return true;
            }
            case >= 0x3F00 and <= 0x3FFF: { // Palette RAM
                ref byte mappedData = ref _palleteMemory.Data[address % 0x1F];
                BusDeviceHandlers.ReadWrite(ref mappedData, isWrite, ref bus);
                return true;
            }
        }

        return false;
    }

    private void ReleaseUnmanagedResources() {
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
        HandleBus(address, true, ref data);
    }

    [MethodImpl(MethodOptions.Options)]
    public byte Read(ushort address) {
        byte data = 0;
        HandleBus(address, false, ref data);

        return data;
    }

    [MethodImpl(MethodOptions.Options)]
    public ushort GetU16(ushort address) {
        return (ushort) (Read((ushort) (address + 1)) << 8 | Read(address));
    }
}
