using System;
using System.Runtime.InteropServices;

namespace Sencae; 

public unsafe class PinnedArray<T> : IDisposable where T : unmanaged {
    public readonly T[] Array;
    public readonly int Length;
    public readonly GCHandle GCHandle;
    public readonly T* Pointer;

    public PinnedArray(T[] array) {
        Array = array;
        Length = Array.Length;
        GCHandle = GCHandle.Alloc(Array, GCHandleType.Pinned);
        Pointer = (T*) GCHandle.AddrOfPinnedObject();
    }

    public void Dispose() {
       GCHandle.Free();
    }
}
