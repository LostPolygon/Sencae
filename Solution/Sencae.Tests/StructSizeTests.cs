using NUnit.Framework;
using Sencae.Ppu;
using Sencae.SharedUtility;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace Sencae.Tests; 

public unsafe class StructSizeTests {
    [Test]
    public void All() {
        Assert.AreEqual(2, sizeof(UInt16Bytes));
        Assert.AreEqual(4, sizeof(PpuObjectAttributes));
        Assert.AreEqual(1024, sizeof(PpuNametable));
        Assert.AreEqual(40, sizeof(MachineLogEntry));
    }
}
