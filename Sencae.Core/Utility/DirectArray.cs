#if !DEBUG
#define USE_DATA_POINTER
#endif

using System;
using System.Runtime.InteropServices;

namespace Sencae;

public readonly struct DirectArray<T> : IDisposable where T : unmanaged {
    public readonly T[] DataRaw;
    public readonly int Length;
    public readonly GCHandle GCHandle;
#if USE_DATA_POINTER
    public readonly unsafe T* Data;
#else
    public readonly T[] Data;
#endif

    public DirectArray(T[] data) {
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

    public void Dispose() {
        GCHandle.Free();
    }
}
