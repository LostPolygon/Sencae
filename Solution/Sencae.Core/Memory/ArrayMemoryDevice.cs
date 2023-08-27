using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae;

public sealed unsafe class ArrayMemoryDevice : IMemoryDevice, IDisposable {
    public readonly int AddressOffset;

    public readonly DirectArray<byte> DirectArray;

    public ArrayMemoryDevice(byte[] memory, int addressOffset) {
        AddressOffset = addressOffset;
        DirectArray = new DirectArray<byte>(memory);
    }

    [MethodImpl(MethodOptions.Options)]
    public ref byte GetU8(ushort address) {
        int index = CalculateIndex(address);
        Debug.Assert(
            index is >= 0 and <= UInt16.MaxValue && index < DirectArray.Length,
            "index is >= 0 and <= UInt16.MaxValue", index.ToString()
        );

        return ref DirectArray.Data[index];
    }

    [MethodImpl(MethodOptions.Options)]
    private int CalculateIndex(ushort address) => (address - AddressOffset) % DirectArray.Length;

    private void ReleaseUnmanagedResources() {
        DirectArray.Dispose();
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ArrayMemoryDevice() {
        ReleaseUnmanagedResources();
    }
}
