using NUnit.Framework;

namespace Sencae.Tests; 

using static SharedUtility.DebugEmulationRunner;

public class Instructions {
    private const int BRK_Cycles = 1;
    
    [Test]
    public void LSR() {
        Machine machine = Emulate(CreateCodeCartridge(
            """

            LDA #$04
            LSR

            """
        ));

        Assert.AreEqual(0x02, machine.State.Cpu.A);
        Assert.AreEqual(4 + BRK_Cycles, machine.State.CpuProgramCycles);
        
        machine = Emulate(CreateCodeCartridge(
            """

            LDA #$04
            STA $10
            LDA #0
            LSR $10

            """
        ));

        Assert.AreEqual(0x02, machine.CpuMemoryBus.Read(0x10));
        Assert.AreEqual(12 + BRK_Cycles, machine.State.CpuProgramCycles);
    }
    
    [Test]
    public void JSR() {
        Machine machine = Emulate(CreateCodeCartridge(
            """

            .dsection "start",     $00, $20
            .dsection "sr",        $010E

            .section "start"
            NOP
            NOP
            NOP
            JSR $C10E // in cartridge space
            NOP
            NOP
            NOP
            NOP
            NOP
            NOP
            NOP
            NOP

            .section "sr"
            INX
            INX

            """
        ));

        Assert.AreEqual(2, machine.State.Cpu.X);
        Assert.AreEqual(0xC111, machine.State.Cpu.PC.Value); // +1 due advancing to next opcode
        Assert.AreEqual(16 + BRK_Cycles, machine.State.CpuProgramCycles);
    }
    
    [Test]
    public void RTS() {
        Machine machine = Emulate(CreateCodeCartridge(
            """

            .dsection "start",     $00, $20
            .dsection "sr",        $010E

            .section "start"
            NOP
            NOP
            NOP
            RTS

            .section "sr"
            // R-T-S jumps 1 byte before the next instruction
            INX
            INX

            """
        ), new () { AfterResetAction = AfterReset });
        
        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.Write(0x01FE, 0x0D); // in cartridge space
            machine.CpuMemoryBus.Write(0x01FF, 0xC1);
            machine.State.Cpu.SP = 0xFD;
        }
        
        Assert.AreEqual(2, machine.State.Cpu.X);
        Assert.AreEqual(0xC111, machine.State.Cpu.PC.Value);
        Assert.AreEqual(16 + BRK_Cycles, machine.State.CpuProgramCycles);
    }
    
    [Test]
    public void CMP() {
        Machine machine;
        
        machine = Emulate(CreateCodeCartridge(
            """

            LDA #50
            STA $10
            CMP $10

            """
        ));

        Assert.AreEqual(true, machine.State.Cpu.P.Carry);
        Assert.AreEqual(true, machine.State.Cpu.P.Zero);
        Assert.AreEqual(false, machine.State.Cpu.P.NegativeOrSign);
        
        machine = Emulate(CreateCodeCartridge(
            """

            LDA #50
            STA $10
            LDA #51
            CMP $10

            """
        ));

        Assert.AreEqual(true, machine.State.Cpu.P.Carry);
        Assert.AreEqual(false, machine.State.Cpu.P.Zero);
        Assert.AreEqual(false, machine.State.Cpu.P.NegativeOrSign);
        
        machine = Emulate(CreateCodeCartridge(
            """

            LDA #$1E
            STA $10
            CMP $10

            """
        ));

        Assert.AreEqual(true, machine.State.Cpu.P.Carry);
        Assert.AreEqual(true, machine.State.Cpu.P.Zero);
        Assert.AreEqual(false, machine.State.Cpu.P.NegativeOrSign);
    }
}
