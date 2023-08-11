using NUnit.Framework;

namespace Sencae.Tests;

using static SharedUtility.DebugEmulationRunner;

public class AddressingModes {
    private const int BRK_Cycles = 1;

    [Test]
    public void Implied() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            INX

            """
        ));

        Assert.AreEqual(1, machine.State.Cpu.X);
        Assert.AreEqual(2 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void Immediate() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            LDA #42

            """
        ));

        Assert.AreEqual(42, machine.State.Cpu.A);
        Assert.AreEqual(2 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void Absolute() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            LDA $0200

            """
        ), new () { AfterResetAction = AfterReset });

        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.GetU8(0x0200) = 42;
        }

        Assert.AreEqual(42, machine.CpuMemoryBus.GetU8(0x0200));
        Assert.AreEqual(4 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void AbsoluteXIndexed() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            INX
            INX
            INX ; X = 3
            LDA #42
            STA $0200,X ; write 42 at $0203

            """
        ));

        Assert.AreEqual(42, machine.CpuMemoryBus.GetU8(0x203));
        Assert.AreEqual(2 + 2 + 2 + 2 + 4 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void AbsoluteYIndexed() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            INY
            INY
            INY ; Y = 3
            LDA #42
            STA $0200,Y ; write 42 at $0203

            """
        ));

        Assert.AreEqual(42, machine.CpuMemoryBus.GetU8(0x203));
        Assert.AreEqual(2 + 2 + 2 + 2 + 4 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void ZeroPageXIndexed() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            INX
            INX
            INX ; X = 3
            LDY $20,X

            """
        ), new () { AfterResetAction = AfterReset });

        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.GetU8(0x20 + 3) = 42;
        }

        Assert.AreEqual(42, machine.State.Cpu.Y);
        Assert.AreEqual(2 + 2 + 2 + 3 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void ZeroPage() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            LDA $20

            """
        ), new () { AfterResetAction = AfterReset });

        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.GetU8(0x20) = 42;
        }

        Assert.AreEqual(42, machine.State.Cpu.A);
        Assert.AreEqual(3 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void ZeroPageYIndexed() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            INY
            INY
            INY ; Y = 3
            LDX $20,Y

            """
        ), new () { AfterResetAction = AfterReset });

        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.GetU8(0x20 + 3) = 42;
        }

        Assert.AreEqual(42, machine.State.Cpu.X);
        Assert.AreEqual(2 + 2 + 2 + 3 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void ZeroPageIndirectXPreIndexed() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            INX
            INX
            INX ; X = 3
            LDA ($4,X)

            """
        ), new () { AfterResetAction = AfterReset });

        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.GetU8(0x7) = 0x20;
            machine.CpuMemoryBus.GetU8(0x20) = 0x42;
        }

        Assert.AreEqual(0x42, machine.State.Cpu.A);
        Assert.AreEqual(2 + 2 + 2 + 5 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void ZeroPageIndirectYPostIndexed() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            INY
            INY
            INY ; Y = 3
            LDA ($4),Y

            """
        ), new () { AfterResetAction = AfterReset });

        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.GetU8(0x4) = 0x20;
            machine.CpuMemoryBus.GetU8(0x23) = 0x42;
        }

        Assert.AreEqual(0x42, machine.State.Cpu.A);
        Assert.AreEqual(2 + 2 + 2 + 5 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void AbsoluteIndirect() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            .dsection "start",    $00, $20
            .dsection "sec",      $020E

            .section "start"
            JMP ($C109) // in cartridge space

            .section "sec"
            INX
            INX

            """
        ), new () { AfterResetAction = AfterReset });

        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.GetU8(0xC109) = 0x0E;
            machine.CpuMemoryBus.GetU8(0xC109 + 1) = 0xC2;
        }

        Assert.AreEqual(2, machine.State.Cpu.X);
        Assert.AreEqual(0xC211, machine.State.Cpu.PC.Value); // +1 due advancing to next opcode
        Assert.AreEqual(5 + 2 + 2 + BRK_Cycles, machine.State.Cycle);
    }
}
