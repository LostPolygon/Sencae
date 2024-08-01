using System;
using System.Diagnostics;
using System.Linq;

namespace Sencae.Ppu;

using static PpuDevice.PpuCycleFlagOperation;
using static PpuDevice.PpuCycleRegisterOperation;
using static PpuDevice.PpuCycleVRamFetchOperation;

public sealed partial class PpuDevice {
   
    private PpuCycleOperation[] CreatePpuCycleOperationTable() {
        const int scanlines = 262;
        const int cyclesInScanline = 341;

        const int visibleScanlines = 240;
        const int postRenderScanlines = 1;
        const int verticalBlankingScanlines = 19;

        const int visibleScanlinesFirst = 0;
        const int visibleScanlinesLast = visibleScanlines - 1;

        const int postRenderScanlinesFirst = visibleScanlines;
        const int postRenderScanlinesLast = postRenderScanlinesFirst + postRenderScanlines - 1;

        const int verticalBlankingScanlinesFirst = postRenderScanlinesLast + 1;
        const int verticalBlankingScanlinesLast = verticalBlankingScanlinesFirst + verticalBlankingScanlines;
        const int preRenderScanline = verticalBlankingScanlinesLast + 1;

        PpuCycleOperation[] operations = new PpuCycleOperation[scanlines * cyclesInScanline];
        operations.AsSpan().Fill(
            new PpuCycleOperation(
                (PpuCycleVRamFetchOperation) 0xFF,
                (PpuCycleRegisterOperation) 0xFF,
                (PpuCycleFlagOperation) 0xFF
            )
        );

        for (int scanline = 0; scanline < scanlines; scanline++) {
            for (int cycle = 0; cycle < cyclesInScanline; cycle++) {
                int index = scanline * cyclesInScanline + cycle;

                switch (scanline) {
                    case >= 0 when scanline <= visibleScanlinesLast || scanline == preRenderScanline:
                        switch (cycle) {
                            case 0:
                                operations[index] = new(
                                    Idle,
                                    PpuCycleRegisterOperation.None,
                                    scanline == 0 ?
                                        Scanline0Cycle0 :
                                        ScanlineNon0Cycle0
                                );
                                break;
                            case >= 1 and <= 256: {
                                int step = (cycle - 1) % 8;
                                operations[index] = step switch {
                                    0 => new(NametableFetchCycle0),
                                    1 => new(NametableFetchCycle1),
                                    2 => new(AttributeTableFetchCycle0),
                                    3 => new(AttributeTableFetchCycle1),
                                    4 => new(PatternTableLsbFetchCycle0),
                                    5 => new(PatternTableLsbFetchCycle1),
                                    6 => new(PatternTableMsbFetchCycle0),
                                    7 => new(PatternTableMsbFetchCycle1, Register: IncrementHorizontalV),
                                    _ => throw new ArgumentOutOfRangeException(nameof(step), step, "")
                                };

                                if (scanline == preRenderScanline && cycle == 1) {
                                    operations[index].Flag = ClearFlags;
                                }
                                
                                break;
                            }
                            case >= 257 and <= 320: {
                                int step = (cycle - 257) % 8;
                                operations[index] = step switch {
                                    0 => new(NametableFetchCycle0),
                                    1 => new(NametableFetchCycle1),
                                    2 => new(NametableFetchCycle0, Register: SpriteAttributeFetch),
                                    3 => new(NametableFetchCycle1, Register: SpriteXFetch),
                                    4 => new(PatternTableLsbFetchCycle0),
                                    5 => new(PatternTableLsbFetchCycle1),
                                    6 => new(PatternTableMsbFetchCycle0),
                                    7 => new(PatternTableMsbFetchCycle1),
                                    _ => throw new ArgumentOutOfRangeException(nameof(step), step, "")
                                };
                                
                                if (scanline == preRenderScanline && cycle is >= 280 and <= 304) {
                                    // FIXME: overrides SpriteAttributeFetch and SpriteXFetch
                                    operations[index].Register = SetVerticalVToVerticalT;
                                }
                                
                                
                                break;
                            }
                            case >= 321 and <= 336: {
                                int step = (cycle - 321) % 8;
                                operations[index] = step switch {
                                    0 => new(NametableFetchCycle0),
                                    1 => new(NametableFetchCycle1),
                                    2 => new(AttributeTableFetchCycle0),
                                    3 => new(AttributeTableFetchCycle1),
                                    4 => new(PatternTableLsbFetchCycle0),
                                    5 => new(PatternTableLsbFetchCycle1),
                                    6 => new(PatternTableMsbFetchCycle0),
                                    7 => new(PatternTableMsbFetchCycle1, Register: IncrementHorizontalV),
                                    _ => throw new ArgumentOutOfRangeException(nameof(step), step, "")
                                };
                                break;
                            }
                            case >= 337 and <= 340: {
                                int step = (cycle - 337) % 2;
                                operations[index] = step switch {
                                    0 => new(NametableFetchCycle0),
                                    1 => new(NametableFetchCycle1),
                                    _ => throw new ArgumentOutOfRangeException(nameof(step), step, "")
                                };
                                break;
                            }
                        }

                        break;

                    case >= 0 when scanline >= postRenderScanlinesFirst && scanline <= postRenderScanlinesLast:
                        operations[index] = new(Idle);
                        break;

                    case >= 0 when scanline >= verticalBlankingScanlinesFirst && scanline <= verticalBlankingScanlinesLast:
                        operations[index] = new(Idle);
                        if (scanline == verticalBlankingScanlinesFirst && cycle == 1) {
                            operations[index].Flag = SetVerticalBlankFlag;
                        }

                        break;
                }
            }
        }
        
        Debug.Assert(operations.All(x => (byte) x.VRamFetch != 0xFF), "operations.All(x => (byte) x.VRamFetch != 0xFF)");
        Debug.Assert(operations.All(x => (byte) x.Register != 0xFF), "operations.All(x => (byte) x.Register != 0xFF)");
        Debug.Assert(operations.All(x => (byte) x.Flag != 0xFF), "operations.All(x => (byte) x.Flag != 0xFF)");

        return operations;
    }
}
