using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public readonly unsafe struct CpuDevice : IBusDevice {
    public readonly Machine Machine;
    
    private readonly MemoryType[] _memoryMap;

    public CpuDevice(Machine machine) {
        Machine = machine;
        _memoryMap = new MemoryType[UInt16.MaxValue];
    }

    private enum MemoryType : byte {
        Ram = 0,
        IORegisters = 1
    }

    [MethodImpl(MethodOptions.Options)]
    public void HandleBus(ushort address, bool isWrite, ref byte bus) {
        /*
        $2000–$2007 ($0008) NES PPU registers
        $2008–$3FFF ($1FF8) Mirrors of $2000–$2007 (repeats every 8 bytes)
        */

        bool isMapped = true;
        byte dummyByte = 0;
        ref byte mappedData = ref dummyByte;
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: {
                mappedData = ref Machine.State.Cpu.Memory[address % 0x800];
                break;
            }
            case >= 0x4000 and <= 0x401F: {
                mappedData = ref Machine.State.Cpu.IORegisters[address % (0x18 + 0x08)];
                break;
            }
            default:
                isMapped = false;
                break;
        }

        if (!isMapped)
            return;

        byte temp;
        if (isWrite) {
            temp = bus;
            mappedData = temp;
        } else {
            temp = mappedData;
            bus = temp;
        }
    }

    public void HardReset(ushort? overrideResetVector = null) {
        ref CpuState cpu = ref Machine.State.Cpu;
        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpu.P = CpuStatusRegisterFlags.InterruptDisable | CpuStatusRegisterFlags.BreakFlag | CpuStatusRegisterFlags.Unused;
        cpu.A = cpu.X = cpu.Y = 0x00;
        cpu.SP = 0xFD;

        cpuMemoryBus.Write(0x4017, 0); // frame irq enabled
        cpuMemoryBus.Write(0x4015, 0); // all APU channels disabled

        for (ushort i = 0x4000; i <= 0x400F; i++) {
            cpuMemoryBus.Write(i, 0);
        }

        for (ushort i = 0x4010; i <= 0x4013; i++) {
            cpuMemoryBus.Write(i, 0);
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

        cpuMemoryBus.Write(0x4015, 0); // all APU channels disabled

        ushort resetVector = overrideResetVector ?? cpuMemoryBus.GetU16(0xFFFC);
        cpu.Internal.State = CpuInternalState.ExecutionState.InterpretOpcode;
        cpu.Internal.DataBus = cpuMemoryBus.Read(resetVector);
        cpu.Internal.Instruction = Instructions.DescriptorByOpcode[0x00];

        cpu.PC.Value = (ushort) (resetVector + 0);
        cpu.Internal.AddressBus.Value = (ushort) (resetVector + 0);
        cpu.Internal.Read = true;

        cpu.Internal.Halted = false;
        
        //Array.Clear(_memoryMap);
    }
}
