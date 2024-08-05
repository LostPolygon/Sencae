using NUnit.Framework;

namespace Sencae.Tests;

[Parallelizable(ParallelScope.All)]
public class BranchTimingTests {
    [Test]
    public void _1_Branch_Basics() {
        using Machine machine = RunGenericTest("1.Branch_Basics");
        AssertPpuNametableMessage(machine, 2, 6, "PASSED"u8);
    }
    
    [Test]
    public void _2_Backward_Branch() {
        using Machine machine = RunGenericTest("2.Backward_Branch");
        AssertPpuNametableMessage(machine, 2, 6, "PASSED"u8);
    }
    
    [Test]
    public void _3_Forward_Branch() {
        using Machine machine = RunGenericTest("3.Forward_Branch");
        AssertPpuNametableMessage(machine, 2, 6, "PASSED"u8);
    }
}
