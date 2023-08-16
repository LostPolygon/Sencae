using NUnit.Framework;
using static Sencae.SharedUtility.DebugEmulationRunner;

namespace Sencae.Tests;

public class NesTestRomTest {
    [Test]
    public void NesTest() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        CartridgeDevice cartridgeDevice = new(
            cartridgeData.Mapper,
            cartridgeData.PrgRom,
            cartridgeData.ChrRom,
            new byte[8 * 1024]
        );

        List<string> logOutput = new();

        Exception? emulationException = null;
        try {
            Machine machine = Emulate(
                cartridgeDevice, new() {
                    StopOnBRKOrHalt = false,
                    MaxCycles = 50000,
                    OverrideResetVector = 0xC000,
                    FormatState = machine => {
                        NintendulatorLog.FormatState(machine, logOutput);
                        return "";
                    },
                    OutputState = s => {
                    },
                    AfterResetAction = NintendulatorLog.ResetMachineToNintendulatorInitialState
                });
        } catch (Exception e) {
            if (e is AssertionException)
                throw;

            emulationException = e;
            Console.WriteLine("Exception: " + e);
        }

        List<string> referenceLogOutput = NintendulatorLog.ReadLogFromFile("Resources/nestest_cpuonly.log");
        (int? mismatchLine, _, _) = NintendulatorLog.CompareLogs(referenceLogOutput, logOutput);

        Assert.Null(emulationException);
        Assert.Null(mismatchLine);
    }
}
