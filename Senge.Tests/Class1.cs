using System.Diagnostics;
using System.Runtime.InteropServices;
using NGChip8Emulator;
using NUnit.Framework;
using static Senge.Tests.AssemblerUtility;

namespace Senge.Tests;

public class Class1 {
    [Test]
    public unsafe void LoadfILE() {
        byte[] testRom = File.ReadAllBytes("Resources/nestest.nes");
        CartridgeData cartridgeData = NesFileParser.Parse(testRom);
        Cartridge cartridge = new Cartridge(cartridgeData.PrgRom, cartridgeData.ChrRom);

        Machine machine = new Machine(cartridge);

        ref MachineState state = ref machine.GetState();
        new byte[] {
                ///*0xa9, 0x33,*/ 0x20, 0x08, 0x13, 0x4c, 0x02, 0x00,
                // 0xe8, 0x88, 0xee, 0x11, 0x00, 0x38, 0x69, 0x02, 
                //0x60, 0x40
                0xa9, 0x01, 0x8d, 0x00, 0x02, 0xa9, 0x05, 0x8d, 0x01, 0x02, 0xa9, 0x08, 0x8d, 0x02, 0x02

                //0x8d, 0x00, 0x02, 0x8d, 0x01, 0x02, 0x8d, 0x02, 0x02
            }
            .AsSpan()
            .CopyTo(MemoryMarshal.CreateSpan(ref state.Cpu.Memory[0], 2048));

        machine.CpuBus.GetU8(0xFFFC) = 0;
        machine.CpuBus.GetU8(0xFFFD) = 0;

        machine.Reset();

        Debug.WriteLine(FormatCpuState(state));
        for (int i = 0; i < 100; i++) {
            machine.Clock();
            Debug.WriteLine(FormatCpuState(state));
            if (machine.State.Cpu.Halted)
                break;
        }

        Assert.AreEqual(1, machine.CpuBus.GetU8(0x200));
        Assert.AreEqual(5, machine.CpuBus.GetU8(0x201));
        Assert.AreEqual(8, machine.CpuBus.GetU8(0x202));
    }

    public static Cartridge CreateCodeCartridge(string assembly, ushort programRomSize = 16 * 1024) {
        byte[] assembledCode = AssembleFromListing(assembly);
        byte[] programRom = new byte[programRomSize];

        assembledCode.AsSpan().CopyTo(programRom);

        ushort resetVector = (ushort) (0xFFFF - programRomSize);
        programRom[^3] = (byte) resetVector;
        programRom[^2] = (byte) (resetVector >> 8);

        byte[] characterRom = new byte[8 * 1024];

        return new Cartridge(programRom, characterRom);
    }

    [Test]
    public void Test1() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            .0000    a9 01        LDA #$01
            .0002    8d 00 02     STA $0200
            .0005    a9 05        LDA #$05
            .0007    8d 01 02     STA $0201
            .000a    a9 08        LDA #$08
            .000c    8d 02 02     STA $0202

            """
        );

        Machine machine = Emulate(cartridge);

        Assert.AreEqual(1, machine.CpuBus.GetU8(0x200));
        Assert.AreEqual(5, machine.CpuBus.GetU8(0x201));
        Assert.AreEqual(8, machine.CpuBus.GetU8(0x202));
    }

    [Test]
    public void Test2() {
        Cartridge cartridge = CreateCodeCartridge(
            """

            .0000    a9 c0        LDA #$c0  ; Load the hex value $c0 into the A register
            .0002    aa           TAX       ; Transfer the value in the A register to X
            .0003    e8           INX       ; Increment the value in the X register
            .0004    69 c4        ADC #$c4  ; Add the hex value $c4 to the A register
            .0006    00           BRK       ; Break - we're done

            """
        );

        Machine machine = Emulate(cartridge);

        Assert.AreEqual(1, machine.CpuBus.GetU8(0x200));
        Assert.AreEqual(5, machine.CpuBus.GetU8(0x201));
        Assert.AreEqual(8, machine.CpuBus.GetU8(0x202));
    }

    private static Machine Emulate(Cartridge cartridge, int cycles = 10000) {
        Machine machine = new Machine(cartridge);

        ref MachineState state = ref machine.GetState();

        machine.Reset();

        Debug.WriteLine(FormatCpuState(state));
        for (int i = 0; i < cycles; i++) {
            machine.Clock();
            Debug.WriteLine(FormatCpuState(state));
            if (machine.State.Cpu.Halted)
                break;
        }

        return machine;
    }

    private static string FormatCpuState(in MachineState machine) {
        CpuState cpu = machine.Cpu;
        return
            $"==== Cycle: {machine.Cycle}, " +
            $"AB: {cpu.Internal.AddressBus.Value:X4}, " +
            $"DB: {cpu.Internal.DataBus:X2}, " +
            $"PC: {cpu.PC.Value:X4}, " +
            $"RW: {(cpu.Internal.Read ? 1 : 0)}, " +
            $"OP: {cpu.Internal.Instruction.Opcode:X2}, " +
            $"A: {cpu.A:X2}, " +
            $"X: {cpu.X:X2}, " +
            $"Y: {cpu.Y:X2}, " +
            $"SP: {cpu.SP:X2}"
            ;
    }
}
