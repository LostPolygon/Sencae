using NUnit.Framework;

namespace Sencae.Tests.TestRoms;

public class CpuDummyWrites {
    [Test]
    public void cpu_dummy_writes_oam() {
        string message = RunBlarrgTest("cpu_dummy_writes_oam", 6);
        Assert.That(message, Does.EndWith("Passed"));
    }
    
    [Test]
    public void cpu_dummy_writes_ppumem() {
        string message = RunBlarrgTest("cpu_dummy_writes_ppumem");
        Assert.That(message, Does.EndWith("Passed"));
    }
}

