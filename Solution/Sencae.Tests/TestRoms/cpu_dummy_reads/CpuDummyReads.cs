using NUnit.Framework;

namespace Sencae.Tests;

[Parallelizable(ParallelScope.All)]
public class CpuDummyReads {
    [Test]
    public void cpu_dummy_reads() {
        using Machine machine = RunGenericTest("cpu_dummy_reads");
        AssertPpuNametableMessage(machine, 1, 4, "Passed"u8);
    }
}
