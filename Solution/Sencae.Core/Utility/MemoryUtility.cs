using System;
using System.Runtime.InteropServices;

namespace Sencae;

public static class MemoryUtility {
    public static Span<byte> AsBytes<T>(scoped ref T data, int length = 1) where T : unmanaged {
        return MemoryMarshal.CreateSpan(ref data, length).AsBytes();
    }

    public static ReadOnlySpan<byte> AsReadOnlyBytes<T>(ref T data, int length = 1) where T : unmanaged {
        return MemoryMarshal.CreateReadOnlySpan(ref data, length).AsBytes();
    }
}
