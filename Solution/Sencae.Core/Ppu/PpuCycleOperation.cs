namespace Sencae.Ppu;

public record struct PpuCycleOperation(
    PpuCycleOperation.PpuCycleVRamFetchOperation VRamFetch,
    PpuCycleOperation.PpuCycleRegisterOperation Register = PpuCycleOperation.PpuCycleRegisterOperation.None,
    PpuCycleOperation.PpuCycleFlagOperation Flag = PpuCycleOperation.PpuCycleFlagOperation.None
) {
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
