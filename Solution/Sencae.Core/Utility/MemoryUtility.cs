using System;
using System.Runtime.InteropServices;

namespace Sencae;

public static class MemoryUtility {
    public static Span<byte> AsBytes<T>(ref T data) where T : unmanaged {
        Span<T> dataSpan = MemoryMarshal.CreateSpan(ref data, 1);
        return MemoryMarshal.AsBytes(dataSpan);
    }
}
