using NUnit.Framework;
using Sencae.SharedUtility;
using static Sencae.SharedUtility.DebugEmulationRunner;

namespace Sencae.Tests;

public class NesTestRomTest {
    [Test]
    public void NesTestCpuOnly() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        MapperDevice mapperDevice = MapperDeviceFactory.Create(cartridgeData, new byte[8 * 1024]);

        List<string> logOutput = new();

        Exception? emulationException = null;
        try {
            using Machine machine = Emulate(
                mapperDevice, new() {
                    StopOnBRKOrHalt = false,
                    MaxMachineCycles = 27500,
                    OverrideResetVector = 0xC000,
                    FormatState = machine => {
                        NintendulatorLog.FormatState(machine, new NintendulatorLog.FormatOptions(), logOutput);
                        return "";
                    },
                    OutputState = s => {
                    },
                    AfterResetAction = NintendulatorLogCompare.ResetMachineToNintendulatorInitialState
                });
        } catch (Exception e) {
            if (e is AssertionException)
                throw;

            emulationException = e;
            Console.WriteLine("Exception: " + e);
        }

        List<string> referenceLogOutput = NintendulatorLogCompare.ReadLogFromFile("Resources/nestest_cpuonly.log");
        (int? mismatchLine, _, _) = NintendulatorLogCompare.CompareLogs(referenceLogOutput, logOutput);

        Assert.Null(emulationException);
        Assert.Null(mismatchLine);
        Assert.GreaterOrEqual(logOutput.Count, referenceLogOutput.Count);
    }

    [Test]
    [Explicit("todo")]
    public void NesTestFull() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        MapperDevice mapperDevice = MapperDeviceFactory.Create(cartridgeData, new byte[8 * 1024]);

        List<string> logOutput = new();

        Exception? emulationException = null;
        try {
            using Machine machine = Emulate(
                mapperDevice, new() {
                    StopOnBRKOrHalt = false,
                    MaxMachineCycles = 27500,
                    OverrideResetVector = 0xC000,
                    FormatState = machine => {
                        if (machine.State.Cpu.Internal.Cycles < 7 || machine.State.MachineCycles % 3 != 0)   
                            return "";

                        NintendulatorLog.FormatState(machine, new NintendulatorLog.FormatOptions(IncludePpu: true), logOutput);
                        return "";
                    },
                    OutputState = s => {
                    },
                    AfterResetAction = NintendulatorLogCompare.ResetMachineToNintendulatorInitialState
                });
        } catch (Exception e) {
            if (e is AssertionException)
                throw;

            emulationException = e;
            Console.WriteLine("Exception: " + e);
        }

        List<string> referenceLogOutput = NintendulatorLogCompare.ReadLogFromFile("Resources/nestest.log");
        (int? mismatchLine, _, _) = NintendulatorLogCompare.CompareLogs(referenceLogOutput, logOutput);

        Assert.Null(emulationException);
        Assert.Null(mismatchLine);
        Assert.GreaterOrEqual(logOutput.Count, referenceLogOutput.Count);
    }
    
    [Test]
    [Explicit("todo")]
    public void NesTestFull2() {
        byte[] testRom = File.ReadAllBytes("Resources/1942 (Japan, USA).nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        MapperDevice mapperDevice = MapperDeviceFactory.Create(cartridgeData, new byte[8 * 1024]);

        List<string> logOutput = new();

        Exception? emulationException = null;
        try {
            using Machine machine = Emulate(
                mapperDevice, new() {
                    StopOnBRKOrHalt = false,
                    MaxMachineCycles = 27500,
                    //OverrideResetVector = 0xC000,
                    FormatState = machine => {
                        if (machine.State.Cpu.Internal.Cycles < 7 || machine.State.MachineCycles % 3 != 0)   
                            return "";
                        
                        NintendulatorLog.FormatState(machine, new NintendulatorLog.FormatOptions(IncludePpu: true, SwapPpuPositions: true), logOutput);
                        return "";
                    },
                    OutputState = s => {
                    },
                    AfterResetAction = (m => {
                        NintendulatorLogCompare.ResetMachineToNintendulatorInitialState(m);
                        m.State.Cpu.Internal.Cycles = 7;
                    })
                });
        } catch (Exception e) {
            if (e is AssertionException)
                throw;

            emulationException = e;
            Console.WriteLine("Exception: " + e);
        }

        List<string> referenceLogOutput = NintendulatorLogCompare.ReadLogFromFile("Resources/1942 (Japan, USA).20240212_215257.log");
        (int? mismatchLine, _, _) = NintendulatorLogCompare.CompareLogs(referenceLogOutput, logOutput);

        Assert.Null(emulationException);
        Assert.Null(mismatchLine);
        Assert.GreaterOrEqual(logOutput.Count, referenceLogOutput.Count);
    }
}
