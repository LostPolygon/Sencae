using NUnit.Framework;

namespace Sencae.Tests; 

using static SharedUtility.DebugEmulationRunner;

public class Instructions {
    private const int BRK_Cycles = 1;
    
    [Test]
    public void LSR() {
        Machine machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            LDA #$01
            LSR

            """
        ));

        Assert.AreEqual(0x02, machine.State.Cpu.A);
        Assert.AreEqual(4 + BRK_Cycles, machine.State.Cycle);
        
        machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            LDA #$01
            STA $10
            LDA #0
            LSR $10

            """
        ));

        Assert.AreEqual(0x02, machine.CpuBus.GetU8(0x10));
        Assert.AreEqual(12 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void JSR() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            .dsection "start",     $00, $20
            .dsection "sr",        $010E
            
            .section "start"
            NOP
            NOP
            NOP
            JSR $C10D // in cartridge space
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
        );

        static void AfterReset(Machine machine) {
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(2, machine.State.Cpu.X);
        Assert.AreEqual(0xC10F, machine.State.Cpu.PC.Value); // +1 due advancing to next opcode
        Assert.AreEqual(16 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void RTS() {
        Cartridge cartridge = CreateCodeCartridge(
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
        );

        static void AfterReset(Machine machine) {
            machine.CpuBus.GetU8(0x01FE) = 0x0D - 1; // in cartridge space
            machine.CpuBus.GetU8(0x01FF) = 0xC1;
            machine.State.Cpu.SP = 0xFD;
        }

        Machine machine = EmulateUntilBreak(cartridge, afterResetAction: AfterReset);

        Assert.AreEqual(2, machine.State.Cpu.X);
        Assert.AreEqual(0xC10F, machine.State.Cpu.PC.Value);
        Assert.AreEqual(16 + BRK_Cycles, machine.State.Cycle);
    }
    
    [Test]
    public void CMP() {
        Machine machine;
        
        machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            LDA #50
            STA $10
            CMP $10

            """
        ));

        Assert.AreEqual(true, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.Carry));
        Assert.AreEqual(true, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.Zero));
        Assert.AreEqual(false, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.NegativeOrSign));
        
        machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            LDA #50
            STA $10
            LDA #51
            CMP $10

            """
        ));

        Assert.AreEqual(true, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.Carry));
        Assert.AreEqual(false, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.Zero));
        Assert.AreEqual(false, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.NegativeOrSign));
        
        machine = EmulateUntilBreak(CreateCodeCartridge(
            """

            LDA #$1E
            STA $10
            CMP $10

            """
        ));

        Assert.AreEqual(true, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.Carry));
        Assert.AreEqual(true, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.Zero));
        Assert.AreEqual(false, machine.State.Cpu.P.HasFlag(CpuStatusRegisterFlags.NegativeOrSign));
    }
}
