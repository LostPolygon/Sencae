using NUnit.Framework;

namespace Sencae.Tests.TestRoms;

public class BlarggPpuTests2005_09_15b {
    [Test]
    public void palette_ram() {
        using Machine machine = RunGenericTest("palette_ram");
        AssertPpuNametableMessage(machine, 2, 5, "$01"u8);
    }

    [Test]
    public void power_up_palette() {
        using Machine machine = RunGenericTest("power_up_palette");
        AssertPpuNametableMessage(machine, 2, 5, "$01"u8);
    }

    [Test]
    public void sprite_ram() {
        using Machine machine = RunGenericTest("sprite_ram");
        AssertPpuNametableMessage(machine, 2, 5, "$01"u8);
    }

    [Test]
    [Ignore(
        "'Ignore vbl_clear_time.nes. It's poorly written and not accurate to one PPU clock. " +
        "4.vbl_clear_timing.nes is the good one to use.' (https://forums.nesdev.org/viewtopic.php?t=3927)"
    )]
    public void vbl_clear_time() {
        using Machine machine = RunGenericTest("vbl_clear_time");
        AssertPpuNametableMessage(machine, 2, 5, "$01"u8);
    }

    [Test]
    public void vram_access() {
        using Machine machine = RunGenericTest("vram_access");
        AssertPpuNametableMessage(machine, 2, 5, "$01"u8);
    }
}
