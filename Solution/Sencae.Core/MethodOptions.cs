using System.Runtime.CompilerServices;

namespace Sencae;

public static class MethodOptions {
    public const MethodImplOptions AggressiveInlining = MethodImplOptions.AggressiveInlining;
    public const MethodImplOptions HotPath = MethodImplOptions.AggressiveInlining;
    public const MethodImplOptions NoInlining = MethodImplOptions.NoInlining;

    //public const MethodImplOptions Options = 0;
}
