namespace Sencae;

public readonly record struct CpuResetOptions {
    //public bool SoftReset { get; init; }
    public ushort? OverrideResetVector { get; init; }
    public CpuMemoryResetState CpuMemoryResetState { get; init; }

    public static CpuResetOptions Default => new() {
        CpuMemoryResetState = CpuMemoryResetState.Pattern
    };
}
