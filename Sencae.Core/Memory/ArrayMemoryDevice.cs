using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae;

public sealed unsafe class ArrayMemoryDevice : IMemoryDevice, IDisposable {
    public readonly int AddressOffset;

    public readonly PinnedArray<byte> PinnedArray;

    public ArrayMemoryDevice(byte[] memory, int addressOffset) {
        AddressOffset = addressOffset;
        PinnedArray = new PinnedArray<byte>(memory);
    }

    [MethodImpl(MethodOptions.Options)]
    public ref byte GetU8(ushort address) {
        int index = CalculateIndex(address);
        Debug.Assert(
            index is >= 0 and <= UInt16.MaxValue && index < PinnedArray.Length,
            "index is >= 0 and <= UInt16.MaxValue", index.ToString()
        );

        return ref PinnedArray.Pointer[index];
    }

    [MethodImpl(MethodOptions.Options)]
    private int CalculateIndex(ushort address) => (address - AddressOffset) % PinnedArray.Length;

    private void ReleaseUnmanagedResources() {
        PinnedArray.Dispose();
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ArrayMemoryDevice() {
        ReleaseUnmanagedResources();
    }
}
