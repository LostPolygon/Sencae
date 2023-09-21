using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

public struct PpuNametable {
    public const int Size = 1024;

    public PpuNametableRows Rows;
    public PpuAttributeTable AttributeTable;

    #pragma warning disable CS0169 // Field is never used

    [InlineArray(30)]
    public struct PpuNametableRows {
        private PpuNametableRow _element0;
    }

    [InlineArray(64)]
    public struct PpuAttributeTable {
        private PpuAttributeTableEntryFlags _element0;

        public(byte bottomRight, byte bottomLeft, byte topRight, byte topLeft) Decode(PpuAttributeTableEntryFlags flags) {
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

    [InlineArray(32)]
    public struct PpuNametableRow {
        private byte _element0;
    }

    #pragma warning restore CS0169 // Field is never used
}
