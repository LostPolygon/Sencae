using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;

namespace Sencae;

public unsafe class ImGuiProto2 {
    public readonly ref struct StringSpan {
        public readonly ReadOnlySpan<byte> Utf8Span;
        public readonly ReadOnlySpan<char> CharSpan;

        public readonly bool IsUtf8;
        public readonly bool IsEmpty;
        public readonly void* Pointer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringSpan(ReadOnlySpan<byte> utf8Span) {
            Utf8Span = utf8Span;
            CharSpan = default;
            IsUtf8 = true;
            IsEmpty = !utf8Span.IsEmpty;
            Pointer = SpanToPointer(utf8Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringSpan(ReadOnlySpan<char> charSpan) {
            Utf8Span = default;
            CharSpan = charSpan;
            IsUtf8 = false;
            IsEmpty = !charSpan.IsEmpty;
            Pointer = SpanToPointer(charSpan);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringSpan(string? str) => new(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringSpan(Span<byte> span) => new(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringSpan(ReadOnlySpan<byte> span) => new(span);
    }

    [FixedAddressValueType]
    private static readonly byte EmptyUtf8String = 0;

    public static byte* GetStringUtf8PointerOrStackallocSize(
        in StringSpan stringSpan,
        out int stackAllocBytes,
        out MemoryHandle pooledMemoryPin
    ) {
        stackAllocBytes = 0;
        pooledMemoryPin = default;

        if (stringSpan.Pointer == null)
            return null;

        if (stringSpan.IsEmpty) {
            fixed (byte* emptyUtf8StringPtr = &EmptyUtf8String) {
                return emptyUtf8StringPtr;
            }
        }

        if (stringSpan.IsUtf8)
            return SpanToPointer(stringSpan.Utf8Span);

        ReadOnlySpan<char> utf16 = stringSpan.CharSpan;
        int utf8MaxBytes = Encoding.UTF8.GetMaxByteCount(utf16.Length) + 1;

        const int stackallocSizeLimit = 2048;
        bool stackAllocated = utf8MaxBytes <= stackallocSizeLimit;

        if (stackAllocated) {
            stackAllocBytes = utf8MaxBytes;
            return null;
        }

        IMemoryOwner<byte> pooledMemoryOwner = MemoryPool<byte>.Shared.Rent(utf8MaxBytes);
        Memory<byte> pooledMemory = pooledMemoryOwner.Memory;
        pooledMemoryPin = pooledMemory.Pin();
        Utf16SpanToUtf8SpanPointer(utf16, pooledMemory.Span);
        byte* utf8Memory = (byte*) pooledMemoryPin.Pointer;
        return utf8Memory;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte* Utf16SpanToUtf8SpanPointer(ReadOnlySpan<char> utf16, Span<byte> utf8) {
        int utf8BytesWritten = Encoding.UTF8.GetBytes(utf16, utf8);
        utf8[utf8BytesWritten] = 0;

        return SpanToPointer<byte>(utf8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* SpanToPointer<T>(ReadOnlySpan<T> span) where T : unmanaged =>
        (T*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));

    public static unsafe bool InputFloat2(
        StringSpan label,
        ref Vector2 v,
        StringSpan format,
        ImGuiInputTextFlags flags
    ) {
        MemoryHandle labelUtf8MemoryPin = default;
        MemoryHandle formatUtf8MemoryPin = default;

        try {
            byte* labelUtf8Ptr = GetStringUtf8PointerOrStackallocSize(label, out int labelUtf8StackAllocBytes, out labelUtf8MemoryPin);
            if (labelUtf8StackAllocBytes != 0) {
                labelUtf8Ptr = Utf16SpanToUtf8SpanPointer(label.CharSpan, stackalloc byte[labelUtf8StackAllocBytes]);
            }

            byte* formatUtf8Ptr = GetStringUtf8PointerOrStackallocSize(format, out int formatUtf8StackAllocBytes, out formatUtf8MemoryPin);
            if (formatUtf8StackAllocBytes != 0) {
                formatUtf8Ptr = Utf16SpanToUtf8SpanPointer(format.CharSpan, stackalloc byte[formatUtf8StackAllocBytes]);
            }

            return ImGuiNative.igInputFloat2(
                labelUtf8Ptr,
                (Vector2*) Unsafe.AsPointer(ref v),
                formatUtf8Ptr,
                flags
            ) > 0;
        } finally {
            labelUtf8MemoryPin.Dispose();
            formatUtf8MemoryPin.Dispose();
        }
    }
}
