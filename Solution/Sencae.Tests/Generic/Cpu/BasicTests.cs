using NUnit.Framework;
using static Sencae.SharedUtility.MachineStateFormatter;

namespace Sencae.Tests;

[Parallelizable(ParallelScope.None)]
public class BasicTests {
    [Test]
    public void Test1() {
        using Machine machine = Emulate(CreateCodeCartridge(
            """

            .0000    a9 01        LDA #$01
            .0002    8d 00 02     STA $0200
            .0005    a9 05        LDA #$05
            .0007    8d 01 02     STA $0201
            .000a    a9 08        LDA #$08
            .000c    8d 02 02     STA $0202

            """
        ));

        Assert.AreEqual(1, machine.CpuMemoryBus.Read(0x200));
        Assert.AreEqual(5, machine.CpuMemoryBus.Read(0x201));
        Assert.AreEqual(8, machine.CpuMemoryBus.Read(0x202));
    }

    [Test]
    public void Test2() {
        MapperDevice mapperDevice = CreateCodeCartridge(
            """

            .0000    a9 c0        LDA #$c0  ; Load the hex value $c0 into the A register
            .0002    aa           TAX       ; Transfer the value in the A register to X
            .0003    e8           INX       ; Increment the value in the X register
            .0004    69 c4        ADC #$c4  ; Add the hex value $c4 to the A register
            .0006    00           BRK       ; Break - we're done

            """
        );

        using Machine machine = Emulate(mapperDevice, new() { FormatState = FormatCpuStateNesTest });

        Assert.AreEqual(0xC1, machine.State.Cpu.Registers.X);
        Assert.AreEqual(0x84, machine.State.Cpu.Registers.A);
        Assert.AreEqual(false, machine.State.Cpu.Registers.P.Overflow.BoolValue);
    }
}
