using NUnit.Framework;

namespace Sencae.Tests; 

using static DebugEmulationRunner;

[Parallelizable(ParallelScope.None)]
public class Instructions {
    private const int BRK_Cycles = 7*0 + 2;
    
    [Test]
    public void LDA_LDX_LDY() {
        using Machine machine = Emulate(CreateCodeCartridge(
            """

            LDA #$04
            LDX #$05
            LDY $1234

            """
        ), new () { AfterResetAction = AfterReset });
        
        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.Write(0x1234, 0x13);
        }
        
        Assert.AreEqual(4, machine.State.Cpu.Registers.A);
        Assert.AreEqual(5, machine.State.Cpu.Registers.X);
        Assert.AreEqual(0x13, machine.State.Cpu.Registers.Y);
        Assert.AreEqual(0x8008, machine.State.Cpu.Registers.PC.Value);
        Assert.AreEqual(2 + 2 + 4 + BRK_Cycles, machine.State.Cpu.Internal.Cycles);
    }
    
    [Test]
    public void LSR() {
        Machine machine = Emulate(CreateCodeCartridge(
            """

            LDA #$04
            LSR

            """
        ));

        machine.Dispose();
        Assert.AreEqual(0x02, machine.State.Cpu.Registers.A);
        Assert.AreEqual(4 + BRK_Cycles, machine.State.Cpu.Internal.Cycles);
        
        machine = Emulate(CreateCodeCartridge(
            """

            LDA #$04
            STA $10
            LDA #0
            LSR $10

            """
        ));

        machine.Dispose();
        Assert.AreEqual(0x02, machine.CpuMemoryBus.Read(0x10));
        Assert.AreEqual(2 + 3 + 2 + 5 + BRK_Cycles, machine.State.Cpu.Internal.Cycles);
    }
    
    [Test]
    public void JSR() {
        using Machine machine = Emulate(CreateCodeCartridge(
            """

            .dsection "start",     $00, $20
            .dsection "sr",        $0100

            .section "start"
            NOP
            NOP
            NOP
            JSR $8100 // in cartridge space
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

        Assert.AreEqual(2, machine.State.Cpu.Registers.X);
        Assert.AreEqual(0x8103, machine.State.Cpu.Registers.PC.Value);
        Assert.AreEqual(16 + BRK_Cycles, machine.State.Cpu.Internal.Cycles);
    }
    
    [Test]
    public void RTS() {
        using Machine machine = Emulate(CreateCodeCartridge(
            """

            .dsection "start",     $00, $20
            .dsection "sr",        $0100

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
            machine.CpuMemoryBus.Write(0x01FE, 0xFF); // in cartridge space
            machine.CpuMemoryBus.Write(0x01FF, 0x80);
            machine.State.Cpu.Registers.SP = 0xFD;
        }
        
        Assert.AreEqual(2, machine.State.Cpu.Registers.X);
        Assert.AreEqual(0x8103, machine.State.Cpu.Registers.PC.Value);
        Assert.AreEqual(2 + 2 + 2 + 6 + 2 + 2 + BRK_Cycles, machine.State.Cpu.Internal.Cycles);
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
        machine.Dispose();

        Assert.AreEqual(true, machine.State.Cpu.Registers.P.Carry.BoolValue);
        Assert.AreEqual(true, machine.State.Cpu.Registers.P.Zero.BoolValue);
        Assert.AreEqual(false, machine.State.Cpu.Registers.P.NegativeOrSign.BoolValue);
        
        machine = Emulate(CreateCodeCartridge(
            """

            LDA #50
            STA $10
            LDA #51
            CMP $10

            """
        ));
        machine.Dispose();

        Assert.AreEqual(true, machine.State.Cpu.Registers.P.Carry.BoolValue);
        Assert.AreEqual(false, machine.State.Cpu.Registers.P.Zero.BoolValue);
        Assert.AreEqual(false, machine.State.Cpu.Registers.P.NegativeOrSign.BoolValue);
        
        machine = Emulate(CreateCodeCartridge(
            """

            LDA #$1E
            STA $10
            CMP $10

            """
        ));
        machine.Dispose();

        Assert.AreEqual(true, machine.State.Cpu.Registers.P.Carry.BoolValue);
        Assert.AreEqual(true, machine.State.Cpu.Registers.P.Zero.BoolValue);
        Assert.AreEqual(false, machine.State.Cpu.Registers.P.NegativeOrSign.BoolValue);
    }
    
    [Test]
    public void BRK() {
        using Machine machine = Emulate(CreateCodeCartridge(
            """

            .dsection "start",     $00, $80
            .dsection "sr",        $0100

            .section "start"
            NOP
            BRK
            INY
            INY
            NOP
            NOP
            NOP
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
            RTI

            """
        ), new () {
            AfterResetAction = AfterReset,
            StopOnBRKOrHalt = false,
            MaxMachineCycles = 50,
        });
        
        static void AfterReset(Machine machine) {
            machine.CpuMemoryBus.Write(0xFFFE, 0x00); // in cartridge space
            machine.CpuMemoryBus.Write(0xFFFF, 0x81);
        }

        Assert.AreEqual(2, machine.State.Cpu.Registers.X);
        Assert.AreEqual(1, machine.State.Cpu.Registers.Y);
    }
}
