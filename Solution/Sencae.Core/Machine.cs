using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae;

public sealed class Machine : IDisposable {
    public MachineCpuMemoryBus CpuMemoryBus;
    public CpuBusDevice CpuBusDevice;
    public CpuDevice<MachineCpuMemoryBus> CpuDevice;
    public PpuCpuBusDevice PpuCpuBusDevice;

    public PpuDevice PpuDevice;
    public PpuMemoryBus PpuMemoryBus;
    public PpuBusDevice PpuBusDevice;

    public MapperDevice MapperDevice;

    public MachineState State;

    public event Action? PpuClocked;
    public event Action? CpuClocked;

    private bool _disposed;

    public Machine(MapperDevice mapperDevice) {
        MapperDevice = mapperDevice;

        State = new MachineState();

        PpuDevice = new PpuDevice(State.Ppu, State.Cpu);
        PpuBusDevice = new PpuBusDevice(this);
        PpuMemoryBus = new PpuMemoryBus(PpuBusDevice, MapperDevice.CartridgeToPpuBusDevice);
        PpuCpuBusDevice = new PpuCpuBusDevice(State, State.Ppu, PpuMemoryBus);

        CpuBusDevice = new CpuBusDevice(State.Cpu);
        CpuMemoryBus = new MachineCpuMemoryBus(CpuBusDevice, PpuCpuBusDevice, MapperDevice.CartridgeToCpuBusDevice);
        CpuDevice = new CpuDevice<MachineCpuMemoryBus>(State.Cpu, CpuMemoryBus);
    }

    public void Reset(CpuResetOptions? options = null, bool hardReset = true) {
        AssertNotDisposed();

        State.ResetCounters();

        options ??= CpuResetOptions.Default;
        if (hardReset) {
            CpuDevice.HardReset(options.Value);
        } else {
            CpuDevice.SoftReset(options.Value);
        }

        MapperDevice.CartridgeToCpuBusDevice.Reset();
        MapperDevice.CartridgeToPpuBusDevice.Reset();
        PpuDevice.SoftReset();
    }

    [MethodImpl(HotPath)]
    public void Clock() {
        AssertNotDisposed();

        if (State.MachineCycles % 3 == 0) {
            CpuClock();
        }

        PpuClock();
        State.MachineCycles++;
    }

    [MethodImpl(HotPath)]
    public void CpuClock() {
        CpuDevice.Clock();
        CpuClocked?.Invoke();
    }

    [MethodImpl(HotPath)]
    public void PpuClock() {
        PpuDevice.Clock();
        PpuClocked?.Invoke();
    }

    [Conditional("DEBUG")]
    private void AssertNotDisposed() {
        Debug.Assert(!_disposed, "!_disposed");
    }

    private void ReleaseUnmanagedResources() {
        //CpuMemoryBus.Dispose();
        CpuBusDevice.Dispose();
        PpuCpuBusDevice.Dispose();

        //PpuMemoryBus.Dispose();
        PpuBusDevice.Dispose();

        MapperDevice.Dispose();
    }

    public void Dispose() {
        _disposed = true;

        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Machine() {
        ReleaseUnmanagedResources();
    }
}
