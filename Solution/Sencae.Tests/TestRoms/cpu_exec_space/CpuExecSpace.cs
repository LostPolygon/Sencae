using NUnit.Framework;

namespace Sencae.Tests.TestRoms;

public class CpuExecSpace {
    [Test]
    public void test_cpu_exec_space_ppuio() {
        string message = RunBlarrgTest("test_cpu_exec_space_ppuio", 6);
        Assert.That(message, Does.EndWith("Passed"));
    }
    
    [Test]
    [Ignore("Not implemented yet")]
    public void test_cpu_exec_space_apu() {
        string message = RunBlarrgTest("test_cpu_exec_space_apu");
        Assert.That(message, Does.EndWith("Passed"));
    }
}

