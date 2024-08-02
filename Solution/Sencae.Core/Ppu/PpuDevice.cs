using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

public sealed partial class PpuDevice {
    private readonly PpuState _ppuState;
    private readonly CpuState _cpuState;
    private readonly PpuCycleOperation[] _operations;

    public PpuDevice(PpuState ppuState, CpuState cpuState) {
        _ppuState = ppuState;
        _cpuState = cpuState;
        _operations = CreatePpuCycleOperationTable();
    }

    public void SoftReset() {
        // https://www.nesdev.org/wiki/PPU_power_up_state
        _ppuState.Registers.PPUCTRL = 0;
        _ppuState.Registers.PPUMASK = 0;
        _ppuState.Registers.PPUSTATUS = 0;
        _ppuState.Registers.OAMADDR = 0;

        _ppuState.Internal = default;
        _ppuState.Internal.Reset = true;

        _ppuState.Memory.AsSpan().Clear();
        _ppuState.OamMemory.AsSpan().Clear();
        _ppuState.PaletteMemory.AsSpan().Clear();
    }

    [MethodImpl(HotPathBlock)]
    public void Clock() {
        ref PpuState.PpuRegisterState registers = ref _ppuState.Registers;
        ref PpuState.PpuInternalState internalState = ref _ppuState.Internal;

        int frameCycle = internalState.FrameCycle;
        PpuCycleOperation operation = _operations[frameCycle];
        switch (operation.Flag) {
            case PpuCycleFlagOperation.None:
                break;
            case PpuCycleFlagOperation.Scanline0Cycle0:
                break;
            case PpuCycleFlagOperation.ScanlineNon0Cycle0:
                break;
            case PpuCycleFlagOperation.SetVerticalBlankFlag:
                registers.PPUSTATUS |= PpuStatusRegisterFlags.VerticalBlankStarted;
                internalState.Reset = false;
                break;
            case PpuCycleFlagOperation.ClearFlags:
                registers.PPUSTATUS &= ~(PpuStatusRegisterFlags.VerticalBlankStarted | PpuStatusRegisterFlags.SpriteOverflow | PpuStatusRegisterFlags.Sprite0Hit);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (operation.Register) {
            case PpuCycleRegisterOperation.None:
                break;
            case PpuCycleRegisterOperation.SpriteAttributeFetch:
                break;
            case PpuCycleRegisterOperation.SpriteXFetch:
                break;
            case PpuCycleRegisterOperation.IncrementHorizontalV:
                break;
            case PpuCycleRegisterOperation.IncrementHorizontalAndVerticalV:
                break;
            case PpuCycleRegisterOperation.SetHorizontalVToHorizontalT:
                break;
            case PpuCycleRegisterOperation.SetVerticalVToVerticalT:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        frameCycle++;
        if (frameCycle == _operations.Length - 1) {
            bool mustSkipDot = !registers.IsRenderingDisabled && internalState.Frames % 2 == 0;

            frameCycle = mustSkipDot ? 1 : 0;
            internalState.Frames++;
        }

        // If the control register tells us to emit a NMI when-
        // entering vertical blanking period, do it! The CPU
        // will be informed that rendering is complete so it can
        // perform operations with the PPU knowing it wont
        // produce visible artefacts
        ref CpuInternalState cpuInternalState = ref _cpuState.Internal;

        bool nmiPulledDownOld = cpuInternalState.NmiPulledDown;
        cpuInternalState.NmiPulledDown =

            //!suppressNmi &&
            (registers.PPUCTRL & PpuControlRegisterFlags.GenerateNmi) != 0 &&
            (registers.PPUSTATUS & PpuStatusRegisterFlags.VerticalBlankStarted) != 0;

        if (cpuInternalState.NmiPulledDown && !nmiPulledDownOld) {
            cpuInternalState.NmiFlipFlop = true;
        }

        // HACK
        if ((registers.PPUMASK & PpuMaskRegisterFlags.ShowBackground) != 0 &&
            (registers.PPUMASK & PpuMaskRegisterFlags.ShowSprites) != 0) {
            if (_cpuState.Internal.Cycles % 5000 == 0) {
                registers.PPUSTATUS |= PpuStatusRegisterFlags.Sprite0Hit;
            }
        }

        internalState.FrameCycle = frameCycle;
        internalState.Cycles++;
    }

    public record struct PpuCycleOperation(
        PpuCycleVRamFetchOperation VRamFetch,
        PpuCycleRegisterOperation Register = PpuCycleRegisterOperation.None,
        PpuCycleFlagOperation Flag = PpuCycleFlagOperation.None
    );

    public enum PpuCycleFlagOperation : byte {
        None,

        Scanline0Cycle0,
        ScanlineNon0Cycle0,
        SetVerticalBlankFlag,
        ClearFlags,
    }

    public enum PpuCycleRegisterOperation : byte {
        None,

        SpriteAttributeFetch,
        SpriteXFetch,

        IncrementHorizontalV,
        IncrementHorizontalAndVerticalV,
        SetHorizontalVToHorizontalT,
        SetVerticalVToVerticalT
    }

    public enum PpuCycleVRamFetchOperation : byte {
        Idle,

        NametableFetchCycle0,
        NametableFetchCycle1,
        AttributeTableFetchCycle0,
        AttributeTableFetchCycle1,
        PatternTableLsbFetchCycle0,
        PatternTableLsbFetchCycle1,
        PatternTableMsbFetchCycle0,
        PatternTableMsbFetchCycle1
    }
}
