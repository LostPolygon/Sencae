using NUnit.Framework;
using static Sencae.SharedUtility.MachineStateFormatter;
using static Sencae.SharedUtility.DebugEmulationRunner;

namespace Sencae.Tests;

public class BasicTests {
    [Test]
    public void NesTest() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        CartridgeDevice cartridgeDevice = CartridgeDeviceFactory.Create(cartridgeData);

        Machine machine = Emulate(
            cartridgeDevice, new() {
                StopOnBRKOrHalt = false,
                MaxMachineCycles = 50000, 
                OverrideResetVector = 0xC000
            });

        BusMemoryStream stream = new BusMemoryStream(machine.CpuMemoryBus);
        stream.Seek(0x6004, SeekOrigin.Begin);

        string message = TextUtility.ReadCString(stream)!;
    }

    [Test]
    public void Test1() {
        Machine machine = Emulate(CreateCodeCartridge(
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
        CartridgeDevice cartridgeDevice = CreateCodeCartridge(
            """

            .0000    a9 c0        LDA #$c0  ; Load the hex value $c0 into the A register
            .0002    aa           TAX       ; Transfer the value in the A register to X
            .0003    e8           INX       ; Increment the value in the X register
            .0004    69 c4        ADC #$c4  ; Add the hex value $c4 to the A register
            .0006    00           BRK       ; Break - we're done

            """
        );

        Machine machine = Emulate(cartridgeDevice, new() { FormatState = FormatCpuStateNesTest });

        Assert.AreEqual(0xC1, machine.State.Cpu.X);
        Assert.AreEqual(0x84, machine.State.Cpu.A);
        Assert.AreEqual(false, machine.State.Cpu.P.Overflow);
    }
}
