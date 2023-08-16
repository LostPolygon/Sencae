using NUnit.Framework;
using static Sencae.SharedUtility.MachineStateFormatter;
using static Sencae.SharedUtility.DebugEmulationRunner;

namespace Sencae.Tests;

[Parallelizable(ParallelScope.All)]
public class InstrTestV5 {
    [Test]
    public void Test_01_Basics() {
        (string message, _) = GenericTest("rom_singles/01-basics");

        Assert.AreEqual("\n01-basics\n\nPassed\n", message);
    }

    [Test]
    public void Test_02_Implied() {
        (string message, _) = GenericTest("rom_singles/02-implied");

        Assert.AreEqual("\n02-implied\n\nPassed\n", message);
    }

    [Test]
    public void Test_03_Immediate() {
        (string message, _) = GenericTest("rom_singles/03-immediate", 50_000_000);

        Assert.AreEqual("\n03-immediate\n\nPassed\n", message);
    }

    [Test]
    public void Test_04_ZeroPage() {
        (string message, _) = GenericTest("rom_singles/04-zero_page");

        Assert.AreEqual("\n04-zero_page\n\nPassed\n", message);
    }

    [Test]
    public void Test_05_zp_xy() {
        (string message, _) = GenericTest("rom_singles/05-zp_xy", 50_000_000);

        Assert.AreEqual("\n05-zp_xy\n\nPassed\n", message);
    }

    [Test]
    public void Test_06_Absolute() {
        (string message, _) = GenericTest("rom_singles/06-absolute");

        Assert.AreEqual("\n06-absolute\n\nPassed\n", message);
    }

    [Test]
    public void Test_07_AbsXY() {
        (string message, _) = GenericTest("rom_singles/07-abs_xy", 50_000_000);

        Assert.AreEqual("\n01-basics\n\nPassed\n", message);
    }

    [Test]
    public void Test_08_IndX() {
        (string message, _) = GenericTest("rom_singles/08-ind_x");

        Assert.AreEqual("\n08-ind_x\n\nPassed\n", message);
    }

    [Test]
    public void Test_09_IndY() {
        (string message, _) = GenericTest("rom_singles/09-ind_y");

        Assert.AreEqual("\n09-ind_y\n\nPassed\n", message);
    }

    [Test]
    public void Test_10_Branches() {
        (string message, _) = GenericTest("rom_singles/10-branches");

        Assert.AreEqual("\n10-branches\n\nPassed\n", message);
    }

    [Test]
    public void Test_11_Stack() {
        (string message, _) = GenericTest("rom_singles/11-stack");

        Assert.AreEqual("\n11-stack\n\nPassed\n", message);
    }

    [Test]
    public void Test_12_JMP_JSR() {
        (string message, _) = GenericTest("rom_singles/12-jmp_jsr");

        Assert.AreEqual("\n12-jmp_jsr\n\nPassed\n", message);
    }

    [Test]
    public void Test_13_RTS() {
        (string message, _) = GenericTest("rom_singles/13-rts");

        Assert.AreEqual("\n13-rts\n\nPassed\n", message);
    }

    [Test]
    public void Test_14_RTI() {
        (string message, _) = GenericTest("rom_singles/14-rti");

        Assert.AreEqual("\n14-rti\n\nPassed\n", message);
    }

    [Test]
    public void Test_15_BRK() {
        (string message, _) = GenericTest(
            "rom_singles/15-brk",
            1_000_000,
            referenceLogFile: "Resources/instr_test-v5/rom_singles/15-brk_cpu.log"
        );

        Assert.AreEqual("\n01-basics\n\nPassed\n", message);
    }

    [Test]
    public void Test_16_Special() {
        (string message, _) = GenericTest("rom_singles/16-special", 50_000_000);

        Assert.AreEqual("\n\n16-special\n\nPassed\n", message);
    }

    [Test]
    public void OfficialOnly() {
        (string message, _) = GenericTest("official_only", 70_000_000);

        Assert.AreEqual("All 16 tests passed\n\n\n", message);
    }

    [Test]
    public void AllInstrs() {
        (string message, _) = GenericTest("all_instrs", 70_000_000);

        Assert.AreEqual("All 16 tests passed\n\n\n", message);
    }

    private(string message, Machine machine) GenericTest(
        string testRomName,
        ulong? maxCycles = null,
        string? referenceLogFile = null
    ) {
        byte[] testRom = File.ReadAllBytes($"Resources/instr_test-v5/{testRomName}.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        CartridgeDevice cartridgeDevice =
            new(
                cartridgeData.Mapper,
                cartridgeData.PrgRom,
                cartridgeData.ChrRom,
                new byte[16 * 1024]
            );

        List<string> logOutput = new();
        Machine machine = new Machine(cartridgeDevice);
        try {
            Func<Machine, string> formatStateFunc =
                referenceLogFile == null ?
                    _ => "" :
                    m => {
                        NintendulatorLog.FormatState(m, logOutput);
                        return "";
                    };
            Emulate(
                machine,
                new() {
                    StopOnBRKOrHalt = false,
                    MaxCycles = maxCycles ?? 5000000,
                    FormatState = formatStateFunc,
                    OutputState = _ => {
                    },
                    AfterResetAction = NintendulatorLog.ResetMachineToNintendulatorInitialState
                }
            );
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }



        if (referenceLogFile != null) {
            List<string> referenceLogOutput = NintendulatorLog.ReadLogFromFile(referenceLogFile);
            (int? mismatchLine, _, _) = NintendulatorLog.CompareLogs(referenceLogOutput, logOutput);
        }

        Assert.AreEqual(0xDE, machine.CpuMemoryBus.Read(0x6001));
        Assert.AreEqual(0xB0, machine.CpuMemoryBus.Read(0x6002));
        Assert.AreEqual(0x61, machine.CpuMemoryBus.Read(0x6003));

        byte status = machine.CpuMemoryBus.Read(0x6000);

        BusMemoryStream stream = new(machine.CpuMemoryBus);
        stream.Seek(0x6004, SeekOrigin.Begin);
        string message = TextUtility.ReadCString(stream);

        Assert.That(
            status,

            //Is.GreaterThanOrEqualTo(0x00).And.LessThanOrEqualTo(0x7F),
            Is.EqualTo(0x00),
            $"Test status is 0x{status:X2}, message:\n{message}"
        );

        return (message, machine);
    }
}
