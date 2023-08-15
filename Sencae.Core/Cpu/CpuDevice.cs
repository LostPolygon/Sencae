using System.Runtime.CompilerServices;

namespace Sencae;

public readonly unsafe struct CpuDevice : IMemoryDevice {
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

    public void HardReset(ushort? overrideResetVector = null) {
        ref CpuState cpu = ref Machine.State.Cpu;
        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpu.P = CpuStatusRegisterFlags.InterruptDisable | CpuStatusRegisterFlags.BreakFlag | CpuStatusRegisterFlags.Unused;
        cpu.A = cpu.X = cpu.Y = 0x00;
        cpu.SP = 0xFD;

        cpuMemoryBus.GetU8(0x4017) = 0; // frame irq enabled
        cpuMemoryBus.GetU8(0x4015) = 0; // all APU channels disabled

        for (ushort i = 0x4000; i <= 0x400F; i++) {
            cpuMemoryBus.GetU8(i) = 0;
        }

        for (ushort i = 0x4010; i <= 0x4013; i++) {
            cpuMemoryBus.GetU8(i) = 0;
        }

        GenericReset(overrideResetVector);
    }

    public void SoftReset(ushort? overrideResetVector = null) {
        ref CpuState cpu = ref Machine.State.Cpu;
        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpu.P |= CpuStatusRegisterFlags.InterruptDisable;
        cpu.SP -= 3;

        GenericReset(overrideResetVector);
    }

    private void GenericReset(ushort? overrideResetVector) {
        ref CpuState cpu = ref Machine.State.Cpu;
        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpuMemoryBus.GetU8(0x4015) = 0; // all APU channels disabled

        ushort resetVector = overrideResetVector ?? cpuMemoryBus.GetU16(0xFFFC);
        cpu.Internal.State = CpuInternalState.ExecutionState.InterpretOpcode;
        cpu.Internal.DataBus = cpuMemoryBus.GetU8(resetVector);
        cpu.Internal.Instruction = Instructions.DescriptorByOpcode[0x00];

        cpu.PC.Value = (ushort) (resetVector + 0);
        cpu.Internal.AddressBus.Value = (ushort) (resetVector + 0);
        cpu.Internal.Read = true;

        cpu.Internal.Halted = false;
    }
}
