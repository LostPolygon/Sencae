using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using Sencae.Ppu;

namespace Sencae.Tests;

using static DebugEmulationRunner;

public static unsafe class NesTestUtility {
    private const ulong CpuCyclesPerSecond = 1_790_000;
    private const ulong MachineCyclesPerSecond = 3 * CpuCyclesPerSecond;

    public static Machine RunGenericTest(
        string romPath,
        double machineRunDuration = 4,
        [CallerFilePath] string? callerFilePath = null
    ) {
        // ReSharper disable once ExplicitCallerInfoArgument
        romPath = GetRelativePath(romPath, callerFilePath: callerFilePath);

        ulong maxMachineCycles = (ulong) (MachineCyclesPerSecond * machineRunDuration);
        byte[] testRom = File.ReadAllBytes(romPath);
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        MapperDevice mapperDevice = MapperDeviceFactory.Create(cartridgeData, new byte[8 * 1024]);

        Machine machine = new Machine(mapperDevice);
        try {
            Emulate(
                machine,
                new() {
                    CpuOnly = false,
                    SkipOutputState = true,
                    StopOnBRKOrHalt = false,
                    MaxMachineCycles = maxMachineCycles,
                    AfterResetAction = (m => {
                        //NintendulatorLogCompare.ResetMachineToNintendulatorInitialState(m);
                        //m.State.Cpu.Internal.Cycles = 7;
                    })
                }
            );
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }

        return machine;
    }

    public static string RunBlarrgTest(
        string romPath,
        double machineRunDuration = 4,
        [CallerFilePath] string? callerFilePath = null
    ) {
        // ReSharper disable once ExplicitCallerInfoArgument
        romPath = GetRelativePath(romPath, callerFilePath: callerFilePath);

        ulong maxMachineCycles = (ulong) (MachineCyclesPerSecond * machineRunDuration);
        byte[] testRom = File.ReadAllBytes(romPath);
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        MapperDevice mapperDevice = MapperDeviceFactory.Create(cartridgeData, new byte[8 * 1024]);

        using Machine machine = new Machine(mapperDevice);
        try {
            Emulate(
                machine,
                new() {
                    CpuOnly = false,
                    SkipOutputState = true,
                    StopOnBRKOrHalt = false,
                    MaxMachineCycles = maxMachineCycles,
                    AfterResetAction = (m => {
                        //NintendulatorLogCompare.ResetMachineToNintendulatorInitialState(m);
                        //m.State.Cpu.Internal.Cycles = 7;
                    })
                }
            );
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }

        Assert.AreEqual(0xDE, machine.CpuMemoryBus.Read(0x6001));
        Assert.AreEqual(0xB0, machine.CpuMemoryBus.Read(0x6002));
        Assert.AreEqual(0x61, machine.CpuMemoryBus.Read(0x6003));

        byte status = machine.CpuMemoryBus.Read(0x6000);

        BusMemoryStream stream = new(machine.CpuMemoryBus);
        stream.Seek(0x6004, SeekOrigin.Begin);
        string message = TextUtility.ReadCString(stream)?.TrimEnd() ?? "[NO MESSAGE]";

        Assert.That(
            status,
            Is.EqualTo(0x00),
            $"Test status is 0x{status:X2}, message:\n{message}"
        );

        Console.WriteLine(message);

        return message;
    }

    public static string GetRelativePath(
        string romFilePath,
        string extension = ".nes",
        [CallerFilePath] string? callerFilePath = null
    ) {
        ArgumentException.ThrowIfNullOrWhiteSpace(callerFilePath);

        if (Path.GetExtension(romFilePath) == extension && File.Exists(romFilePath))
            return romFilePath;

        romFilePath = Path.Combine(Path.GetDirectoryName(callerFilePath)!, romFilePath);
        if (File.Exists(romFilePath))
            return romFilePath;

        if (Path.GetExtension(romFilePath) != extension) {
            romFilePath += extension;
        }

        if (File.Exists(romFilePath))
            return romFilePath;

        throw new FileNotFoundException(romFilePath);
    }

    public static PpuNametable GetPpuNametable(Machine machine, int nametableIndex = 0) {
        int baseAddress = PpuConstants.Nametable0StartAddress + sizeof(PpuNametable) * nametableIndex;

        PpuNametable nametable = default;
        Span<byte> nametableDataRaw = MemoryMarshal.CreateSpan(ref nametable, 1).AsBytes();
        for (ushort i = 0; i < PpuNametable.Size; i++) {
            nametableDataRaw[i] = machine.PpuMemoryBus.ReadObserve((ushort) (baseAddress + i));
        }

        return nametable;
    }

    public static void AssertPpuNametableMessage(Machine machine, int column, int row, ReadOnlySpan<byte> message) {
        PpuNametable ppuNametable = GetPpuNametable(machine);

        Range ppuMessageRowRange = new(column, ppuNametable.Rows[row].AsReadOnlySpan().Length);
        byte[] ppuMessage = ppuNametable.Rows[row].AsReadOnlySpan()[ppuMessageRowRange].ToArray();
        Assert.AreEqual(
            message.ToArray(),
            ppuMessage[..message.Length],
            $"Expected: \"{Encoding.ASCII.GetString(message)}\", got: \"{Encoding.ASCII.GetString(ppuMessage).TrimEnd()}\""
        );
    }
}
