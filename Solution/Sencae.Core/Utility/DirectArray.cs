#if !DEBUG
#define USE_DATA_POINTER
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#if USE_DATA_POINTER
using System.Runtime.InteropServices;
#endif

namespace Sencae;

public readonly struct DirectArray<T> : IDisposable where T : unmanaged {
    public readonly T[] DataRaw;
    public readonly int Length;
    public readonly int Offset;
#if USE_DATA_POINTER
    private readonly GCHandle _gcHandle;
    private readonly unsafe T* _data;
#else
    private readonly T[] _data;
#endif

    public unsafe ref T this[int i] {
        [MethodImpl(AggressiveInlining)]
        get => ref _data[i];
    }

    public DirectArray(T[]? data, int offset = 0) {
        ArgumentNullException.ThrowIfNull(data);
        Debug.Assert(offset is > -UInt16.MaxValue and < UInt16.MaxValue);

        Offset = offset;
        DataRaw = data;
        Length = DataRaw.Length;
#if USE_DATA_POINTER
        _gcHandle = GCHandle.Alloc(DataRaw, GCHandleType.Pinned);
        unsafe {
            _data = (T*) _gcHandle.AddrOfPinnedObject();
        }
#else
        _data = DataRaw;
#endif
    }

    [MethodImpl(HotPath)]
    public int CalculateIndex(int address) {
#if USE_DATA_POINTER
        Debug.Assert(_gcHandle.IsAllocated, "GCHandle.IsAllocated");
#endif

        int index = (address - Offset) % Length;
        Debug.Assert(
            index is >= 0 and <= UInt16.MaxValue && index < Length,
            "index is >= 0 and <= UInt16.MaxValue",
            index.ToString()
        );

        return index;
    }

    public Span<T> AsSpan() => DataRaw.AsSpan();

    public void Dispose() {
#if USE_DATA_POINTER
        _gcHandle.Free();
#endif
    }

    public override string ToString() =>
        $"{nameof(Length)}: {Length} (0x{Length:X4}), " +
        $"{nameof(Offset)}: {Offset} (0x{Offset:X4})";

    public static DirectArray<T>? FromNullable(T[]? array, int offset = 0) =>
        array != null ? new DirectArray<T>(array, offset) : null;
}

public static class DirectArray {
    public static DirectArray<T>? FromNullable<T>(T[]? array, int offset = 0) where T : unmanaged =>
        DirectArray<T>.FromNullable(array, offset);
}
