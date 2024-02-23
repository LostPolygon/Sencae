using System;

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
        _ppuState.PalleteMemory.AsSpan().Clear();
    }

    public void Clock() {
        ref PpuState.PpuInternalState ppuStateInternal = ref _ppuState.Internal;
        ref int scanline = ref ppuStateInternal.Scanline;
        ref int scanlineCycle = ref ppuStateInternal.CyclesInScanline;

        bool shouldSkipOddDot = !_ppuState.IsRenderingDisabled && ppuStateInternal.Frames % 2 != 0;
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
            case 261 when scanlineCycle == 1:
                _ppuState.PPUSTATUS &= ~PpuStatusRegisterFlags.VerticalBlankStarted;
                ppuStateInternal.Reset = false;
                goto case 261;
            case 261: {
                int targetColumn = shouldSkipOddDot ? 339 : 340;
                if (scanlineCycle == targetColumn) {
                    scanlineCycle = 0;
                    scanline = 0;

                    //_lastPpuFrameStart = State.MachineCycles;
                    ppuStateInternal.Frames++;
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

        // If the control register tells us to emit a NMI when-
        // entering vertical blanking period, do it! The CPU
        // will be informed that rendering is complete so it can
        // perform operations with the PPU knowing it wont
        // produce visible artefacts
        ref CpuInternalState cpuInternalState = ref _cpuState.Internal;
        cpuInternalState.NmiPulledDown =
            !suppressNmi &&
            (_ppuState.PPUCTRL & PpuControlRegisterFlags.GenerateNmi) != 0 &&
            (_ppuState.PPUSTATUS & PpuStatusRegisterFlags.VerticalBlankStarted) != 0;

        if (cpuInternalState.NmiPulledDown && !cpuInternalState.NmiPulledDownOld) {
            cpuInternalState.NmiFlipFlop = true;
        }

        cpuInternalState.NmiPulledDownOld = cpuInternalState.NmiPulledDown;
        
        // HACK
        if (_cpuState.Internal.Cycles % 28394 == 0) {
            _ppuState.PPUSTATUS |= PpuStatusRegisterFlags.Sprite0Hit;
        } else if (_cpuState.Internal.Cycles % 30000 == 0) {
            _ppuState.PPUSTATUS &= ~PpuStatusRegisterFlags.Sprite0Hit;
        }

        ppuStateInternal.Cycles++;
    }
}
