using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public unsafe class CpuDevice : MemoryDevice {
    public readonly Machine Machine;

    public CpuDevice(Machine machine) {
        Machine = machine;
    }

    [MethodImpl(MethodOptions.Options)]
    public ref byte GetU8(ushort address) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: return ref Machine.State.Cpu.Memory[address % 0x800];
            case >= 0x4000 and <= 0x401F: return ref Machine.State.Cpu.IORegisters[address % (0x18 + 0x08)];
        }

        return ref DeviceUtility.OpenBus(address);
    }

    public void PowerUp() {
        /*
    P = $34[1] (IRQ disabled)[2]
    A, X, Y = 0
    S = $FD
    $4017 = $00 (frame irq enabled)
    $4015 = $00 (all channels disabled)
    $4000-$400F = $00
    $4010-$4013 = $00 [4]
    All 15 bits of noise channel LFSR = $0000[5]. The first time the LFSR is clocked from the all-0s state, it will shift in a 1.
    APU Frame Counter:

        2A03E, G, various clones: APU Frame Counter reset.
        2A03letterless: APU frame counter powers up at a value equivalent to 15

    Internal memory ($0000-$07FF) has unreliable startup state. Some machines may have consistent RAM contents at power-on, but others do not.

        Emulators often implement a consistent RAM startup state (e.g. all $00 or $FF, or a particular pattern), and flash carts like the PowerPak may partially or fully initialize RAM before starting a program, so an NES programmer must be careful not to rely on the startup contents of RAM.
         */
    }

    public void Reset(ushort? overrideResetVector = null) {
        ref CpuState cpu = ref Machine.State.Cpu;
        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpu.P = CpuStatusRegisterFlags.InterruptDisable | CpuStatusRegisterFlags.BreakFlag | CpuStatusRegisterFlags.Unused;

        cpu.A = cpu.X = cpu.Y = 0x00;
        cpu.SP = 0xFD;

        cpuMemoryBus.GetU8(0x4017) = 0; // frame irq enabled
        cpuMemoryBus.GetU8(0x4015) = 0; // all APU channels disabled

        cpuMemoryBus.GetU8(0x4010) = 0;
        cpuMemoryBus.GetU8(0x4011) = 0;
        cpuMemoryBus.GetU8(0x4012) = 0;
        cpuMemoryBus.GetU8(0x4013) = 0;

        ushort resetVector = overrideResetVector ?? cpuMemoryBus.GetU16(0xFFFC);
        cpu.Internal.State = CpuInternalState.ExecutionState.InterpretOpcode;
        cpu.Internal.DataBus = cpuMemoryBus.GetU8(resetVector);
        cpu.Internal.Instruction = Instructions.DescriptorByOpcode[0x00];

        cpu.PC.Value = (ushort) (resetVector + 0);
        cpu.Internal.AddressBus.Value = (ushort) (resetVector + 0);
        cpu.Internal.Read = true;
    }
}
