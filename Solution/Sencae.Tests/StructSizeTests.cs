using NUnit.Framework;
using Sencae.Ppu;

namespace Sencae.Tests; 

public unsafe class StructSizeTests {
    [Test]
    public void All() {
        Assert.AreEqual(4, sizeof(PpuObjectAttributes));
        Assert.AreEqual(1024, sizeof(PpuNametable));
    }
}
