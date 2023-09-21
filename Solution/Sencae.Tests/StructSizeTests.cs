using NUnit.Framework;
using Sencae.Ppu;
using Sencae.SharedUtility;

namespace Sencae.Tests; 

public unsafe class StructSizeTests {
    [Test]
    public void All() {
        Assert.AreEqual(2, sizeof(UInt16Bytes));
        Assert.AreEqual(4, sizeof(PpuObjectAttributes));
        Assert.AreEqual(1024, sizeof(PpuNametable));
        Assert.AreEqual(32, sizeof(MachineLogEntry));
    }
}
