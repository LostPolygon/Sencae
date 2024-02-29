using System.Runtime.CompilerServices;
using Sencae.Ppu;

namespace Sencae;

public sealed class OamDmaDevice(
    DmaDeviceState state,
    CpuState cpuState,
    MachineCpuMemoryBus cpuMemoryBus,
    PpuState ppuState
) : DmaDevice(state) {
    public readonly CpuState CpuState = cpuState;
    public readonly MachineCpuMemoryBus CpuMemoryBus = cpuMemoryBus;
    public readonly PpuState PpuState = ppuState;
    private readonly DirectArray<byte> _oamMemory = new DirectArray<byte>(ppuState.OamMemory);

    [MethodImpl(HotPath)]
    protected override void ExecuteCycle() {
        if (State.IsReadCycle) {
            UInt16Bytes dmaCpuAddress = State.DmaStartAddress;
            dmaCpuAddress.Low = (byte) (dmaCpuAddress.Low + State.BytesCopied);
            CpuMemoryBus.HandleBus(dmaCpuAddress.Value, MemoryAccessMode.Read, ref State.Data);
        } else {
            _oamMemory[State.BytesCopied] = State.Data;
            if (State.BytesCopied == 255) {
                State.IsActive = false;
            } else {
                State.BytesCopied++;
            }
        }

        State.IsReadCycle = !State.IsReadCycle;
    }

    [MethodImpl(HotPath)]
    public override bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        switch (address) {
            case CpuInternalState.OamDmaRegister: {
                // OAMDMA
                ref byte mappedData = ref CpuState.IORegisters[CpuInternalState.OamDmaRegisterIndex];
                switch (accessMode) {
                    case MemoryAccessMode.Read:
                    case MemoryAccessMode.ReadObserve:
                        BusDeviceHandlers.ReadOnlyRaw(ref mappedData, accessMode, ref bus);
                        return true;
                    case MemoryAccessMode.Write:
                        BusDeviceHandlers.WriteOnlyRaw(ref mappedData, accessMode, ref bus);
                        State.IsActive = true;
                        State.BytesCopied = 0;
                        State.IsReadCycle = true; // FIXME: there should be a delay cycle instead
                        State.DmaStartAddress.Value = (ushort) (bus * 0x100);
                        return true;
                }

                break;
            }
        }

        return false;
    }
}
