using NUnit.Framework;
using Sencae.SharedUtility;
using static Sencae.SharedUtility.DebugEmulationRunner;

namespace Sencae.Tests;

[Parallelizable(ParallelScope.All)]
public class InstrTestV5 {
    [Test]
    public void Test_01_Basics() {
        string message = GenericTest("rom_singles/01-basics");

        Assert.AreEqual("\n01-basics\n\nPassed\n", message);
    }

    [Test]
    public void Test_02_Implied() {
        string message = GenericTest("rom_singles/02-implied");

        Assert.AreEqual("\n02-implied\n\nPassed\n", message);
    }

    [Test]
    public void Test_03_Immediate() {
        string message = GenericTest("rom_singles/03-immediate", 150_000_000);

        Assert.AreEqual("\n03-immediate\n\nPassed\n", message);
    }

    [Test]
    public void Test_04_ZeroPage() {
        string message = GenericTest("rom_singles/04-zero_page");

        Assert.AreEqual("\n04-zero_page\n\nPassed\n", message);
    }

    [Test]
    public void Test_05_zp_xy() {
        string message = GenericTest("rom_singles/05-zp_xy", 150_000_000);

        Assert.AreEqual("\n05-zp_xy\n\nPassed\n", message);
    }

    [Test]
    public void Test_06_Absolute() {
        string message = GenericTest("rom_singles/06-absolute");

        Assert.AreEqual("\n06-absolute\n\nPassed\n", message);
    }

    [Test]
    public void Test_07_AbsXY() {
        string message = GenericTest("rom_singles/07-abs_xy", 150_000_000);

        Assert.AreEqual("\n07-abs_xy\n\nPassed\n", message);
    }

    [Test]
    public void Test_08_IndX() {
        string message = GenericTest("rom_singles/08-ind_x");

        Assert.AreEqual("\n08-ind_x\n\nPassed\n", message);
    }

    [Test]
    public void Test_09_IndY() {
        string message = GenericTest("rom_singles/09-ind_y");

        Assert.AreEqual("\n09-ind_y\n\nPassed\n", message);
    }

    [Test]
    public void Test_10_Branches() {
        string message = GenericTest("rom_singles/10-branches");

        Assert.AreEqual("\n10-branches\n\nPassed\n", message);
    }

    [Test]
    public void Test_11_Stack() {
        string message = GenericTest("rom_singles/11-stack");

        Assert.AreEqual("\n11-stack\n\nPassed\n", message);
    }

    [Test]
    public void Test_12_JMP_JSR() {
        string message = GenericTest("rom_singles/12-jmp_jsr");

        Assert.AreEqual("\n12-jmp_jsr\n\nPassed\n", message);
    }

    [Test]
    public void Test_13_RTS() {
        string message = GenericTest("rom_singles/13-rts");

        Assert.AreEqual("\n13-rts\n\nPassed\n", message);
    }

    [Test]
    public void Test_14_RTI() {
        string message = GenericTest("rom_singles/14-rti");

        Assert.AreEqual("\n14-rti\n\nPassed\n", message);
    }

    [Test]
    public void Test_15_BRK() {
        string message = GenericTest(
            "rom_singles/15-brk",
            3_000_000
            //referenceLogFile: "Resources/instr_test-v5/rom_singles/15-brk_cpu.log"
        );

        Assert.AreEqual("\n15-brk\n\nPassed\n", message);
    }

    [Test]
    public void Test_16_Special() {
        string message = GenericTest("rom_singles/16-special", 250_000_000);

        Assert.AreEqual("\n\n16-special\n\nPassed\n", message);
    }

    [Test]
    public void OfficialOnly() {
        string message = GenericTest("official_only", 210_000_000);

        Assert.AreEqual("All 16 tests passed\n\n\n", message);
    }

    [Test]
    public void AllInstrs() {
        string message = GenericTest("all_instrs", 210_000_000);

        Assert.AreEqual("All 16 tests passed\n\n\n", message);
    }

    private string GenericTest(
        string testRomName,
        ulong? maxCycles = null,
        string? referenceLogFile = null
    ) {
        byte[] testRom = File.ReadAllBytes($"Resources/instr_test-v5/{testRomName}.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        CartridgeDevice cartridgeDevice = CartridgeDeviceFactory.Create(cartridgeData, new byte[16 * 1024]);

        List<string> logOutput = new();
        using Machine machine = new Machine(cartridgeDevice);
        try {
            Func<Machine, string> formatStateFunc =
                referenceLogFile == null ?
                    _ => "" :
                    m => {
                        NintendulatorLog.FormatState(m, new NintendulatorLog.FormatOptions(IncludePpu: true), logOutput);
                        return "";
                    };
            Emulate(
                machine,
                new() {
                    StopOnBRKOrHalt = false,
                    MaxMachineCycles = maxCycles ?? 15000000,
                    FormatState = formatStateFunc,
                    OutputState = _ => {
                    },
                    AfterResetAction = NintendulatorLogCompare.ResetMachineToNintendulatorInitialState
                }
            );
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }

        int? referenceMismatchLine = null;
        if (referenceLogFile != null) {
            List<string> referenceLogOutput = NintendulatorLogCompare.ReadLogFromFile(referenceLogFile);
            var compareLogsResult = NintendulatorLogCompare.CompareLogs(referenceLogOutput, logOutput);
            referenceMismatchLine = compareLogsResult.mismatchLine;
        }

        Assert.AreEqual(0xDE, machine.CpuMemoryBus.Read(0x6001));
        Assert.AreEqual(0xB0, machine.CpuMemoryBus.Read(0x6002));
        Assert.AreEqual(0x61, machine.CpuMemoryBus.Read(0x6003));

        byte status = machine.CpuMemoryBus.Read(0x6000);

        BusMemoryStream stream = new(machine.CpuMemoryBus);
        stream.Seek(0x6004, SeekOrigin.Begin);
        string message = TextUtility.ReadCString(stream)!;

        Assert.That(
            status,

            //Is.GreaterThanOrEqualTo(0x00).And.LessThanOrEqualTo(0x7F),
            Is.EqualTo(0x00),
            $"Test status is 0x{status:X2}, message:\n{message}"
        );
        
        Assert.AreEqual(null, referenceMismatchLine);

        return message;
    }
}
