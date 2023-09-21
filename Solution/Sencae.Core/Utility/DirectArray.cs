#if !DEBUG && false
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
    public readonly GCHandle GCHandle;
#if USE_DATA_POINTER
    public readonly unsafe T* Data;
#else
    public readonly T[] Data;
#endif

    public DirectArray(T[] data, int offset = 0) {
        Debug.Assert(offset is > -UInt16.MaxValue and < UInt16.MaxValue);
        
        Offset = offset;
        DataRaw = data;
        Length = DataRaw.Length;
        GCHandle = GCHandle.Alloc(DataRaw, GCHandleType.Pinned);
#if USE_DATA_POINTER
        unsafe {
            Data = (T*) GCHandle.AddrOfPinnedObject();
        }
#else
        Data = DataRaw;
#endif
    }

    [MethodImpl(MethodOptions.Options)]
    public int CalculateIndex(ushort address) {
        int index = (address - Offset) % Length;
        Debug.Assert(
            index is >= 0 and <= UInt16.MaxValue && index < Length,
            "index is >= 0 and <= UInt16.MaxValue", index.ToString()
        );
        
        return index;
    }

    public void Dispose() {
        GCHandle.Free();
    }

    public override unsafe string ToString() =>
#if USE_DATA_POINTER
        $"{nameof(Data)}: {(IntPtr) Data}" + 
#else
        $"{nameof(Data)}: {Data}, " + 
#endif
        $"{nameof(Length)}: {Length} (0x{Length:X4}), " +
        $"{nameof(Offset)}: {Offset} (0x{Offset:X4}), ";
}
