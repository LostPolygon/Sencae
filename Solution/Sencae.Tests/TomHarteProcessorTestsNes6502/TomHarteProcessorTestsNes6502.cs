using System.Runtime.ExceptionServices;
using NUnit.Framework;
using SharpCompress;
using SharpCompress.Archives;

namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

[TestFixture]
public class TomHarteProcessorTestsNes6502 {
    [Test, TestCaseSource(nameof(GetOfficialOpcodesTestFilePaths)), Parallelizable(ParallelScope.Children)]
    public void OfficialOpcodes(string testFileId) {
        RunTestFile(testFileId);
    }
    
    [Test, TestCaseSource(nameof(GetUnofficialOpcodesTestFilePaths)), Parallelizable(ParallelScope.Children)]
    [Explicit("todo")]
    public void UnofficialOpcodes(string testFileId) {
        RunTestFile(testFileId);
    }

    private void RunTestFile(string testFileId) {
        TestCaseDto[] testCases = GetTestCasesFromFile(testFileId);

        Console.WriteLine($"Running test file id {EnhanceTestName(testFileId)}");

        int ranTestsCount = 0;
        for (int i = 0; i < testCases.Length; i++) {
            TestCaseDto testCaseDto = testCases[i];
            try {
                //Console.WriteLine($"Running test case #{i} (name '{testCaseDto.Name}')");
                bool ran = RunTestCase(i, testCaseDto);
                if (ran) {
                    ranTestsCount++;
                }
            } catch (Exception e) {
                Console.WriteLine(TestCaseSerialization.SerializeTestCase(testCaseDto));
                ExceptionDispatchInfo.Capture(
                    new Exception($"Test case #{i} (name '{EnhanceTestName(testCaseDto.Name)}') failed", e)
                ).Throw();
            }
        }
        
        Console.WriteLine($"Succeeded {ranTestsCount} tests");
    }

    private bool RunTestCase(int id, TestCaseDto testCaseDto) {
        using TestMachine machine = new() {
            CpuState = {
                Registers = {
                    A = testCaseDto.Initial.A,
                    X = testCaseDto.Initial.X,
                    Y = testCaseDto.Initial.Y,
                    SP = testCaseDto.Initial.S,
                    PC = new UInt16Bytes(testCaseDto.Initial.PC),
                    P = CpuStatusRegisterFlags.FromFlags((CpuStatusRegisterPackedFlags) testCaseDto.Initial.P),
                },

                Internal = {
                    AddressBus = new UInt16Bytes(testCaseDto.Initial.PC),
                    State = CpuInternalState.ExecutionState.FinishingCurrentInstructionAndFetchingNextOpcode
                }
            },
            CpuCycles = 7
        };

        foreach (TestCaseDto.RamEntryDto ramEntryDto in testCaseDto.Initial.Ram) {
            byte ramValue = ramEntryDto.Value;
            machine.CpuMemoryBus.HandleBus(ramEntryDto.Address, MemoryAccessMode.Write, ref ramValue);
        }

        List<(ushort address, bool isRead, byte busData)> busCycles = new();
        machine.CpuDevice.CpuBusHandling += CpuDeviceOnCpuBusHandling;

        void CpuDeviceOnCpuBusHandling(ushort address, bool isRead, byte busData) {
            busCycles.Add((address, isRead, busData));
        }

        for (int i = 0; i < testCaseDto.Cycles.Length+1; i++) {
            machine.Clock();
        }

        machine.CpuDevice.CpuBusHandling -= CpuDeviceOnCpuBusHandling;
        busCycles.RemoveAt(0);

        //Assert.IsFalse(machine.CpuState.Internal.Halted, "Machine halted");

        Assert.AreEqual(testCaseDto.Final.A, machine.CpuState.Registers.A, "CpuState.A mismatch");
        Assert.AreEqual(testCaseDto.Final.X, machine.CpuState.Registers.X, "CpuState.X mismatch");
        Assert.AreEqual(testCaseDto.Final.Y, machine.CpuState.Registers.Y, "CpuState.Y mismatch");
        Assert.AreEqual(testCaseDto.Final.S, machine.CpuState.Registers.SP, "CpuState.SP mismatch");
        Assert.AreEqual(testCaseDto.Final.PC, machine.CpuState.Registers.PC.Value, "CpuState.PC mismatch");
        Assert.AreEqual((CpuStatusRegisterPackedFlags) testCaseDto.Final.P, machine.CpuState.Registers.P.AsFlags, "CpuState.P mismatch");

        for (int i = 0; i < testCaseDto.Final.Ram.Length; i++) {
            TestCaseDto.RamEntryDto ramEntryDto = testCaseDto.Final.Ram[i];
            byte ramValue = 0;
            machine.CpuMemoryBus.HandleBus(ramEntryDto.Address, MemoryAccessMode.ReadObserve, ref ramValue);
            Assert.AreEqual(ramEntryDto.Value, ramValue, $"Address {ramEntryDto.Address} (#{i}) mismatch");
        }

        Assert.AreEqual(testCaseDto.Cycles.Length, busCycles.Count, "Cycle count mismatch");
        for (int i = 0; i < testCaseDto.Cycles.Length; i++) {
            Assert.AreEqual(testCaseDto.Cycles[i].Address, busCycles[i].address, $"Bus cycle {i} Address mismatch");
            Assert.AreEqual(testCaseDto.Cycles[i].IsRead, busCycles[i].isRead, $"Bus cycle {i} IsRead mismatch");
            Assert.AreEqual(testCaseDto.Cycles[i].BusData, busCycles[i].busData, $"Bus cycle {i} BusData mismatch");
        }

        return true;
    }

    private static string[] GetOfficialOpcodesTestFilePaths() =>
        Enumerable
            .Range(0, 256)
            .Select(x => x.ToString("x2"))
            .Where(x => (GetInstructionFromTestName(x).Flags & InstructionFlags.Unofficial) == 0)
            .ToArray();
    
    private static string[] GetUnofficialOpcodesTestFilePaths() =>
        Enumerable
            .Range(0, 256)
            .Select(x => x.ToString("x2"))
            .Where(x => (GetInstructionFromTestName(x).Flags & InstructionFlags.Unofficial) != 0)
            .ToArray();

    private static TestCaseDto[] GetTestCasesFromFile(string testFileId) {
        byte[] decompressedBuffer = GetTestFileJsonUtf8(testFileId);
        TestCaseDto[] testCaseDtos = TestCaseSerialization.DeserializeTestFile(decompressedBuffer);

        return testCaseDtos;
    }

    private static byte[] GetTestFileJsonUtf8(string testFileId) {
        // Check if a cached decompressed version exists
        string testFileCachedPath = Path.Combine(Path.GetTempPath(), $"6502test-{testFileId}.json");
        
        string testFilePath = TestFileIdToTestFilePath(testFileId);
        if (File.Exists(testFileCachedPath)) {
            testFilePath = testFileCachedPath;
        }
        
        byte[] fileBytes = File.ReadAllBytes(testFilePath);
        if (Path.GetExtension(testFilePath).Equals(".json", StringComparison.InvariantCultureIgnoreCase))
            return fileBytes;

        using MemoryStream stream = new(fileBytes);
        using IArchive archive = ArchiveFactory.Open(testFilePath);
        IArchiveEntry firstEntry = archive.Entries.First();

        byte[] decompressedBuffer = new byte[firstEntry.Size];
        using Stream entryStream = firstEntry.OpenEntryStream();
        entryStream.ReadFully(decompressedBuffer);

        if (!CIUtility.IsRunningUnderCI) {
            File.WriteAllBytes(testFileCachedPath, decompressedBuffer);
        }

        return decompressedBuffer;
    }

    private static string TestFileIdToTestFilePath(string testFileId) {
        const string basePath = "Resources/TomHarte_ProcessorTests_nes6502/v1";
        return $"{basePath}/{testFileId}.json.7z";
    }

    private static string EnhanceTestName(string testName) {
        InstructionDescriptor instruction = GetInstructionFromTestName(testName);
        return $"{instruction.Mnemonic} - {testName}";
    }

    private static InstructionDescriptor GetInstructionFromTestName(string testName) {
        string opcodeHex = testName.Split(" ").First();
        byte opcode = Convert.FromHexString(opcodeHex)[0];
        InstructionDescriptor instruction = Sencae.Instructions.DescriptorByOpcode[opcode];
        return instruction;
    }
}
