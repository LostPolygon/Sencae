using System;

namespace Sencae.Ppu;

public unsafe class PpuDevice {
    public readonly Machine Machine;

    private readonly PpuState _state;

    public PpuDevice(Machine machine) {
        Machine = machine;
        _state = Machine.State.Ppu;
    }

    public void SoftReset() {
        // https://www.nesdev.org/wiki/PPU_power_up_state
        _state.PPUCTRL = 0;
        _state.PPUMASK = 0;
        _state.OAMADDR = 0;
        _state.PPUCTRL = 0;

        _state.Internal.Address.Value = 0;
        _state.Internal.PpuAddressRegisterLatch = false;
        _state.Internal.PpuScrollRegisterLatch = false;
        _state.Internal.FineScrollX = 0;
        _state.Internal.FineScrollY = 0;

        _state.Internal.DataBus = 0;
        _state.Internal.DataBusLatch = 0;

        _state.Memory.AsSpan().Clear();
        _state.OamMemory.AsSpan().Clear();
        _state.PalleteMemory.AsSpan().Clear();

        _state.Internal.NmiPending = false;
        _state.Internal.CpuCyclesAtNmi = 0;
    }
}
