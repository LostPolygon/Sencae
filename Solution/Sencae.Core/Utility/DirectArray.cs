#if !DEBUG
#define USE_DATA_POINTER
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae;

public readonly struct DirectArray<T> : IDisposable where T : unmanaged {
    public readonly T[] DataRaw;
    public readonly int Length;
    public readonly int Offset;
    private readonly GCHandle _gcHandler;
#if USE_DATA_POINTER
    private readonly unsafe T* _data;
#else
    private readonly T[] _data;
#endif

    public unsafe ref T this[int i] {
        [MethodImpl(AggressiveInlining)]
        get => ref _data[i];
    }

    public DirectArray(T[] data, int offset = 0) {
        Debug.Assert(offset is > -UInt16.MaxValue and < UInt16.MaxValue);

        Offset = offset;
        DataRaw = data;
        Length = DataRaw.Length;
        _gcHandler = GCHandle.Alloc(DataRaw, GCHandleType.Pinned);
#if USE_DATA_POINTER
        unsafe {
            _data = (T*) GCHandle.AddrOfPinnedObject();
        }
#else
        _data = DataRaw;
#endif
    }

    [MethodImpl(HotPath)]
    public int CalculateIndex(int address) {
#if USE_DATA_POINTER
        Debug.Assert(GCHandle.IsAllocated, "GCHandle.IsAllocated");
#endif

        int index = (address - Offset) % Length;
        Debug.Assert(
            index is >= 0 and <= UInt16.MaxValue && index < Length,
            "index is >= 0 and <= UInt16.MaxValue",
            index.ToString()
        );

        return index;
    }

    public void Dispose() {
        _gcHandler.Free();
    }

    public override string ToString() =>
        $"{nameof(Length)}: {Length} (0x{Length:X4}), " +
        $"{nameof(Offset)}: {Offset} (0x{Offset:X4}), ";
}
