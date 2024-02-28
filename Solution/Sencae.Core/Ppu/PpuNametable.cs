using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sencae.Ppu;

public struct PpuNametable {
    public const int Size = 1024;
    public const int AttributeTableSize = 64;

    public PpuNametableRows Rows;
    public PpuAttributeTable AttributeTable;

    #pragma warning disable CS0169 // Field is never used

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = ByteLength)]
    public struct PpuNametableRows {
        private PpuNametableRow _element0;

        public const int Length = 30;
        public const int ByteLength = PpuNametableRow.Length * Length;
        public Span<PpuNametableRow> AsSpan() => MemoryMarshal.CreateSpan(ref _element0, Length);
#if !NETSTANDARD
        public readonly ReadOnlySpan<PpuNametableRow> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in _element0, Length);
#endif
        public static ReadOnlySpan<PpuNametableRow> AsReadOnlySpan(ref PpuNametableRows obj) => 
            MemoryMarshal.CreateReadOnlySpan(ref obj._element0, Length);
    }

    [StructLayout(LayoutKind.Sequential, Size = ByteLength)]
    public struct PpuAttributeTable {
        private PpuAttributeTableEntryFlags _element0;

        public const int Length = AttributeTableSize;
        public const int ByteLength = sizeof(PpuAttributeTableEntryFlags) * AttributeTableSize;

        public Span<PpuAttributeTableEntryFlags> AsSpan() => MemoryMarshal.CreateSpan(ref _element0, Length);

#if !NETSTANDARD
        public readonly ReadOnlySpan<PpuAttributeTableEntryFlags> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in _element0, Length);
#endif
        
        public static ReadOnlySpan<PpuAttributeTableEntryFlags> AsReadOnlySpan(ref PpuAttributeTable obj) => 
            MemoryMarshal.CreateReadOnlySpan(ref obj._element0, Length);
        
        public static(byte bottomRight, byte bottomLeft, byte topRight, byte topLeft) Decode(PpuAttributeTableEntryFlags flags) {
            /*
            7654 3210
            |||| ||++- Color bits 3-2 for top left quadrant of this byte
            |||| ++--- Color bits 3-2 for top right quadrant of this byte
            ||++------ Color bits 3-2 for bottom left quadrant of this byte
            ++-------- Color bits 3-2 for bottom right quadrant of this byte
            */

            (byte bottomRight, byte bottomLeft, byte topRight, byte topLeft) result = new() {
                topLeft = (byte) (flags & PpuAttributeTableEntryFlags.TopLeftQuadrantColorBits),
                topRight = (byte) ((byte) (flags & PpuAttributeTableEntryFlags.TopRightQuadrantColorBits) >> 2),
                bottomLeft = (byte) ((byte) (flags & PpuAttributeTableEntryFlags.BottomLeftQuadrantColorBits) >> 4),
                bottomRight = (byte) ((byte) (flags & PpuAttributeTableEntryFlags.BottomRightQuadrantColorBits) >> 6),
            };
            return result;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = ByteLength)]
    public struct PpuNametableRow {
        private byte _element0;

        public const int Length = 32;
        public const int ByteLength = sizeof(byte) * Length;
        
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element0, Length);

#if !NETSTANDARD
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(in _element0, Length);
#endif
        
        public static ReadOnlySpan<byte> AsReadOnlySpan(ref PpuNametableRow obj) => 
            MemoryMarshal.CreateReadOnlySpan(ref obj._element0, Length);
    }

    #pragma warning restore CS0169 // Field is never used
}
