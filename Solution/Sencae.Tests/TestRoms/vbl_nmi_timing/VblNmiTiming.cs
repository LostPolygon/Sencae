using NUnit.Framework;

namespace Sencae.Tests.TestRoms;

public class VblNmiTiming {
    [Test]
    public void _1_frame_basics() {
        using Machine machine = RunGenericTest("1.frame_basics");
        AssertPpuNametableMessage(machine, 2, 6, "$01"u8);
    }

    [Test]
    public void _2_vbl_timing() {
        using Machine machine = RunGenericTest("2.vbl_timing");
        AssertPpuNametableMessage(machine, 2, 6, "$01"u8);
    }

    [Test]
    public void _3_even_odd_frames() {
        using Machine machine = RunGenericTest("3.even_odd_frames");
        AssertPpuNametableMessage(machine, 2, 6, "$01"u8);
    }

    [Test]
    public void _4_vbl_clear_timing() {
        using Machine machine = RunGenericTest("4.vbl_clear_timing");
        AssertPpuNametableMessage(machine, 2, 6, "$01"u8);
    }

    [Test]
    public void _5_nmi_suppression() {
        using Machine machine = RunGenericTest("5.nmi_suppression");
        AssertPpuNametableMessage(machine, 2, 6, "$01"u8);
    }
    
    [Test]
    public void _6_nmi_disable() {
        using Machine machine = RunGenericTest("6.nmi_disable");
        AssertPpuNametableMessage(machine, 6, 2, "$01"u8);
    }
    
    [Test]
    public void _7_nmi_timing() {
        using Machine machine = RunGenericTest("7.nmi_timing");
        AssertPpuNametableMessage(machine, 6, 2, "$01"u8);
    }
}
