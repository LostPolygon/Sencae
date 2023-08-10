using NUnit.Framework;

namespace Sencae.Tests;

using static SharedUtility.DebugEmulationRunner;

public class AddressingModes {
    private const int BRK_Cycles = 1;
    
    [Test]
    public void Implied() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            INX

            """
        );

        static void AfterReset(Machine machine) {
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(1, machine.State.Cpu.X);
        Assert.AreEqual(2 + BRK_Cycles, machine.State.Cycle);
    }

    [Test]
    public void Immediate() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            LDA #42

            """
        );

        static void AfterReset(Machine machine) {
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(42, machine.State.Cpu.A);
        Assert.AreEqual(2 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void Absolute() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            LDA $0200

            """
        );

        static void AfterReset(Machine machine) {
            machine.CpuBus.GetU8(0x0200) = 42;
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(42, machine.CpuBus.GetU8(0x0200));
        Assert.AreEqual(4 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void AbsoluteXIndexed() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            INX
            INX
            INX ; X = 3
            LDA #42
            STA $0200,X ; write 42 at $0203

            """
        );

        static void AfterReset(Machine machine) {
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(42, machine.CpuBus.GetU8(0x203));
        Assert.AreEqual(2 + 2 + 2 + 2 + 4 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void AbsoluteYIndexed() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            INY
            INY
            INY ; Y = 3
            LDA #42
            STA $0200,Y ; write 42 at $0203

            """
        );

        static void AfterReset(Machine machine) {
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(42, machine.CpuBus.GetU8(0x203));
        Assert.AreEqual(2 + 2 + 2 + 2 + 4 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void ZeroPageXIndexed() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            INX
            INX
            INX ; X = 3
            LDY $20,X

            """
        );

        static void AfterReset(Machine machine) {
            machine.CpuBus.GetU8(0x20 + 3) = 42;
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(42, machine.State.Cpu.Y);
        Assert.AreEqual(2 + 2 + 2 + 3 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void ZeroPage() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            LDA $20

            """
        );

        static void AfterReset(Machine machine) {
            machine.CpuBus.GetU8(0x20) = 42;
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(42, machine.State.Cpu.A);
        Assert.AreEqual(3 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void ZeroPageYIndexed() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            INY
            INY
            INY ; Y = 3
            LDX $20,Y

            """
        );

        static void AfterReset(Machine machine) {
            machine.CpuBus.GetU8(0x20 + 3) = 42;
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(42, machine.State.Cpu.X);
        Assert.AreEqual(2 + 2 + 2 + 3 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void ZeroPageIndirectXPreIndexed() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            INX
            INX
            INX ; X = 3
            LDA ($4,X)

            """
        );

        static void AfterReset(Machine machine) {
            machine.CpuBus.GetU8(0x7) = 0x20;
            machine.CpuBus.GetU8(0x20) = 0x42;
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(0x42, machine.State.Cpu.A);
        Assert.AreEqual(2 + 2 + 2 + 5 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void ZeroPageIndirectYPostIndexed() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            INY
            INY
            INY ; Y = 3
            LDA ($4),Y

            """
        );

        static void AfterReset(Machine machine) {
            machine.CpuBus.GetU8(0x4) = 0x20;
            machine.CpuBus.GetU8(0x23) = 0x42;
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(0x42, machine.State.Cpu.A);
        Assert.AreEqual(2 + 2 + 2 + 5 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void AbsoluteIndirect() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            .dsection "start",    $00, $20
            .dsection "sec",      $020E
            
            .section "start"
            JMP ($C109) // in cartridge space
            
            .section "sec"
            INX
            INX

            """
        );

        static void AfterReset(Machine machine) {
            machine.CpuBus.GetU8(0xC109) = 0x0D;
            machine.CpuBus.GetU8(0xC109 + 1) = 0xC2;
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(2, machine.State.Cpu.X);
        Assert.AreEqual(0xC20F, machine.State.Cpu.PC.Value); // +1 due advancing to next opcode
        Assert.AreEqual(5 + 2 + 2 + BRK_Cycles, machine.State.Cycle);
    }
}
