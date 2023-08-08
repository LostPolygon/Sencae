using System.Diagnostics;
using System.Runtime.InteropServices;
using NGChip8Emulator;
using NUnit.Framework;

namespace Senge.Tests;

public class Class1 {
    [Test]
    public unsafe void Load() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        NesFile nesFile = NesFileParser.Parse(testRom);
        Cartridge cartridge = new Cartridge(nesFile.PrgRom, nesFile.ChrRom);

        Emulator emulator = new Emulator(new Machine(cartridge));

        Machine machine = emulator.Machine;

        ref MachineState state = ref emulator.GetState();
        new byte[] {
                ///*0xa9, 0x33,*/ 0x20, 0x08, 0x13, 0x4c, 0x02, 0x00,
                // 0xe8, 0x88, 0xee, 0x11, 0x00, 0x38, 0x69, 0x02, 
                //0x60, 0x40
                0xa9, 0x01, 0x8d, 0x00, 0x02, 0xa9, 0x05, 0x8d, 0x01, 0x02, 0xa9, 0x08, 0x8d, 0x02, 0x02
                //0x8d, 0x00, 0x02, 0x8d, 0x01, 0x02, 0x8d, 0x02, 0x02
            }
            .AsSpan()
            .CopyTo(MemoryMarshal.CreateSpan(ref state.Cpu.Memory[0], 2048));

        machine.CpuBus.WriteU8(0xFFFC, 0);
        machine.CpuBus.WriteU8(0xFFFD, 0);

        emulator.Reset();

        Debug.WriteLine(FormatCpuState(state));
        for (int i = 0; i < 100; i++) {
            emulator.Clock();
            Debug.WriteLine(FormatCpuState(state));
            if (machine.State.Cpu.Halted)
                break;
        }

        Assert.AreEqual(1, machine.CpuBus.ReadU8(0x200));
        Assert.AreEqual(5, machine.CpuBus.ReadU8(0x201));
        Assert.AreEqual(8, machine.CpuBus.ReadU8(0x202));
    }

    private static string FormatCpuState(in MachineState machine) {
        CpuState cpu = machine.Cpu;
        return
            $"==== Cycle: {machine.Cycle}, " +
            $"AB: {cpu.Internal.AddressBus.Value:X4}, " +
            $"DB: {cpu.Internal.DataBus:X2}, " +
            $"PC: {cpu.PC.Value:X4}, " +
            $"RW: {(cpu.Internal.Read ? 1 : 0)}, " +
            $"OP: {cpu.Internal.IR:X2}, " +
            $"A: {cpu.A:X2}, " +
            $"X: {cpu.X:X2}, " +
            $"Y: {cpu.Y:X2}, " +
            $"SP: {cpu.SP:X2}"
            ;
    }
}
