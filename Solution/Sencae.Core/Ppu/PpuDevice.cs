using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

public sealed partial class PpuDevice {
    private static readonly byte[] PowerUpPalette = [
        0x09, 0x01, 0x00, 0x01, 0x00, 0x02, 0x02, 0x0D, 0x08, 0x10, 0x08, 0x24, 0x00, 0x00, 0x04, 0x2C,
        0x09, 0x01, 0x34, 0x03, 0x00, 0x04, 0x00, 0x14, 0x08, 0x3A, 0x00, 0x02, 0x00, 0x20, 0x2C, 0x08
    ];

    private readonly PpuState _ppuState;
    private readonly CpuState _cpuState;
    private readonly PpuCycleOperation[] _operations;

    public PpuDevice(PpuState ppuState, CpuState cpuState) {
        _ppuState = ppuState;
        _cpuState = cpuState;
        _operations = PpuOperationTableGenerator.Generate();
    }

    public void SoftReset() {
        // https://www.nesdev.org/wiki/PPU_power_up_state
        _ppuState.Registers.PPUCTRL = 0;
        _ppuState.Registers.PPUMASK = 0;
        _ppuState.Registers.PPUSTATUS = 0;
        _ppuState.Registers.OAMADDR = 0;

        _ppuState.Internal = default;

        _ppuState.Memory.AsSpan().Clear();
        _ppuState.OamMemory.AsSpan().Clear();
        _ppuState.PaletteMemory.AsSpan().Clear();
        PowerUpPalette.CopyTo(_ppuState.PaletteMemory.AsSpan());
    }

    [MethodImpl(HotPathBlock)]
    public void Clock() {
        ref PpuState.PpuRegisterState registers = ref _ppuState.Registers;
        ref PpuState.PpuInternalState internalState = ref _ppuState.Internal;

        int frameCycle = internalState.FrameCycle;
        PpuCycleOperation operation = _operations[frameCycle];
        switch (operation.Flag) {
            case PpuCycleOperation.PpuCycleFlagOperation.None:
                break;
            case PpuCycleOperation.PpuCycleFlagOperation.Scanline0Cycle0:
                break;
            case PpuCycleOperation.PpuCycleFlagOperation.ScanlineNon0Cycle0:
                break;
            case PpuCycleOperation.PpuCycleFlagOperation.SetVerticalBlankFlag:
                registers.PPUSTATUS |= PpuStatusRegisterFlags.VerticalBlankStarted;
                //internalState.LastNmiStartCycle = _ppuState.Internal.Cycles;
                break;
            case PpuCycleOperation.PpuCycleFlagOperation.ClearFlags:
                //ulong nmiDuration = _ppuState.Internal.Cycles - internalState.LastNmiStartCycle;
                //Console.WriteLine($"nmiDuration = {nmiDuration} PPU cycles");
                
                registers.PPUSTATUS &= ~(PpuStatusRegisterFlags.VerticalBlankStarted | PpuStatusRegisterFlags.SpriteOverflow | PpuStatusRegisterFlags.Sprite0Hit);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (operation.Register) {
            case PpuCycleOperation.PpuCycleRegisterOperation.None:
                break;
            case PpuCycleOperation.PpuCycleRegisterOperation.SpriteAttributeFetch:
                break;
            case PpuCycleOperation.PpuCycleRegisterOperation.SpriteXFetch:
                break;
            case PpuCycleOperation.PpuCycleRegisterOperation.IncrementHorizontalV:
                break;
            case PpuCycleOperation.PpuCycleRegisterOperation.IncrementHorizontalAndVerticalV:
                break;
            case PpuCycleOperation.PpuCycleRegisterOperation.SetHorizontalVToHorizontalT:
                break;
            case PpuCycleOperation.PpuCycleRegisterOperation.SetVerticalVToVerticalT:
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
            
            ulong nmiDuration = internalState.Cycles - internalState.LastNmiStartCycle;
            //Console.WriteLine($"nmiDuration = {nmiDuration} PPU cycles");
            internalState.LastNmiStartCycle = internalState.Cycles;
        }

        // HACK
        if ((registers.PPUMASK & PpuMaskRegisterFlags.ShowBackground) != 0 &&
            (registers.PPUMASK & PpuMaskRegisterFlags.ShowSprites) != 0) {
            if (_cpuState.Internal.Cycles % 5000 == 0) {
                registers.PPUSTATUS |= PpuStatusRegisterFlags.Sprite0Hit;
            }
        }

        frameCycle++;
        if (frameCycle == _operations.Length - 1) {
            bool mustSkipDot = !registers.IsRenderingDisabled && internalState.Frames % 2 != 0;

            frameCycle = mustSkipDot ? 1 : 0;
            internalState.Frames++;
        }

        internalState.FrameCycle = frameCycle;
        internalState.Cycles++;
    }
}