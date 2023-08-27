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
    }
    
    [InlineArray(32)]
    private struct PpuNametableRow {
        private byte _element0;
    }

    #pragma warning restore CS0169 // Field is never used
}
