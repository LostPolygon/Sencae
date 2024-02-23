using NUnit.Framework;

namespace Sencae.Tests; 

using static SharedUtility.DebugEmulationRunner;

public class PpuTests {
    // $2001 is write only (writing updates open_bus, reading returns open_bus)
    // A write to $2003, followed by a read from $2001 should return the same value as was written."
    [Test]
    public void PpuOpenBus() {
        static void AfterReset(Machine machine) {
            //machine.State.Cpu.Internal.Cycles = 50000; // PPU doesn't start responding immediately
            machine.State.Ppu.Internal.Reset = false;
        }
        
        Machine machine = Emulate(CreateCodeCartridge(
            """

            LDA #254
            STA $2001
            LDA #0
            LDA $2001

            """
        ), new () { AfterResetAction = AfterReset });

        machine.Dispose();
        Assert.AreEqual(254, machine.State.Cpu.A);
        
        machine = Emulate(CreateCodeCartridge(
            """

            LDA #254
            STA $2003
            LDA #0
            LDA $2001

            """
        ), new () { AfterResetAction = AfterReset });
        
        machine.Dispose();
        Assert.AreEqual(254, machine.State.Cpu.A);
    }
}
