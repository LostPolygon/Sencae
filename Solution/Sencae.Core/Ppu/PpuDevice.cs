using System;
using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

public sealed class PpuDevice {
    private readonly PpuState _ppuState;
    private readonly CpuState _cpuState;

    public PpuDevice(PpuState ppuState, CpuState cpuState) {
        _ppuState = ppuState;
        _cpuState = cpuState;
    }

    public void SoftReset() {
        // https://www.nesdev.org/wiki/PPU_power_up_state
        _ppuState.PPUCTRL = 0;
        _ppuState.PPUMASK = 0;
        _ppuState.PPUSTATUS = 0;
        _ppuState.OAMADDR = 0;

        _ppuState.Internal = default;
        _ppuState.Internal.Reset = true;

        _ppuState.Memory.AsSpan().Clear();
        _ppuState.OamMemory.AsSpan().Clear();
        _ppuState.PaletteMemory.AsSpan().Clear();

        StartNewFrame(ref _ppuState.Internal);
    }

    public void Clock() {
        ref PpuState.PpuInternalState ppuStateInternal = ref _ppuState.Internal;
        int scanline = ppuStateInternal.Scanline;
        int scanlineCycle = ppuStateInternal.CyclesInScanline;

        bool suppressNmi = false;
        switch (scanline) {
            case 241 when scanlineCycle == 1: {
                // Effectively end of frame, so set vertical blank flag
                _ppuState.PPUSTATUS |= PpuStatusRegisterFlags.VerticalBlankStarted;

                long cyclesSinceLastPpuStatusRead = (long) (ppuStateInternal.Cycles - ppuStateInternal.LastPPUSTATUS_ReadCycle);
                suppressNmi = false;
                if (cyclesSinceLastPpuStatusRead is 0) {
                } else {
                }

                goto default;
            }
            case 261: {
                if (scanlineCycle == 1) {
                    _ppuState.PPUSTATUS &= ~PpuStatusRegisterFlags.VerticalBlankStarted;
                    ppuStateInternal.Reset = false;
                }

                if (scanlineCycle == ppuStateInternal.NextFrameLastScanlineLastCycle) {
                    scanlineCycle = 0;
                    scanline = 0;

                    //_lastPpuFrameStart = State.MachineCycles;
                    ppuStateInternal.Frames++;

                    StartNewFrame(ref ppuStateInternal);
                } else {
                    scanlineCycle++;
                }

                break;
            }
            default:
                scanlineCycle++;
                if (scanlineCycle == 341) {
                    scanlineCycle = 0;
                    scanline++;
                }

                break;
        }

        ppuStateInternal.Scanline = scanline;
        ppuStateInternal.CyclesInScanline = scanlineCycle;

        // If the control register tells us to emit a NMI when-
        // entering vertical blanking period, do it! The CPU
        // will be informed that rendering is complete so it can
        // perform operations with the PPU knowing it wont
        // produce visible artefacts
        ref CpuInternalState cpuInternalState = ref _cpuState.Internal;

        bool nmiPulledDownOld = cpuInternalState.NmiPulledDown;
        cpuInternalState.NmiPulledDown =
            !suppressNmi &&
            (_ppuState.PPUCTRL & PpuControlRegisterFlags.GenerateNmi) != 0 &&
            (_ppuState.PPUSTATUS & PpuStatusRegisterFlags.VerticalBlankStarted) != 0;

        if (cpuInternalState.NmiPulledDown && !nmiPulledDownOld) {
            cpuInternalState.NmiFlipFlop = true;
        }

        // HACK
        if (_cpuState.Internal.Cycles % 28394 == 0) {
            _ppuState.PPUSTATUS |= PpuStatusRegisterFlags.Sprite0Hit;
        } else if (_cpuState.Internal.Cycles % 30000 == 0) {
            _ppuState.PPUSTATUS &= ~PpuStatusRegisterFlags.Sprite0Hit;
        }

        ppuStateInternal.Cycles++;
    }

    [MethodImpl(HotPath)]
    private void StartNewFrame(ref PpuState.PpuInternalState ppuStateInternal) {
        bool canSkipOddDot = !_ppuState.IsRenderingDisabled;
        int targetColumn = canSkipOddDot ? 339 + (int) (ppuStateInternal.Frames % 2) : 340;
        ppuStateInternal.NextFrameLastScanlineLastCycle = targetColumn;
    }
}
