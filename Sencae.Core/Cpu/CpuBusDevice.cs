using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public sealed unsafe class CpuBusDevice : IBusDevice, IDisposable {
    public readonly Machine Machine;

    private readonly DirectArray<byte> _memory;
    private readonly DirectArray<byte> _ioRegisters;

    public DirectArray<byte> Memory => _memory;

    public CpuBusDevice(Machine machine) {
        Machine = machine;

        _memory = new DirectArray<byte>(machine.State.Cpu.Memory);
        _ioRegisters = new DirectArray<byte>(machine.State.Cpu.IORegisters);
    }

    [MethodImpl(MethodOptions.Options)]
    public bool HandleBus(ushort address, bool isWrite, ref byte bus) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: {
                ref byte mappedData = ref _memory.Data[address % 0x800];
                BusDeviceHandlers.ReadWrite(ref mappedData, isWrite, ref bus);
                return true;
            }
            case >= 0x4000 and <= 0x401F: {
                ref byte mappedData = ref _ioRegisters.Data[address % (0x18 + 0x08)];
                BusDeviceHandlers.ReadWrite(ref mappedData, isWrite, ref bus);
                return true;
            }
        }

        return false;
    }

    public void HardReset(ushort? overrideResetVector = null) {
        ref CpuState cpu = ref Machine.State.Cpu;
        cpu = new CpuState();

        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpu.P = new CpuStatusRegisterFlags {
            InterruptDisable = true,
            BreakFlag = false
        };

        cpu.A = cpu.X = cpu.Y = 0x00;
        cpu.SP = 0xFD;

        for (ushort i = 0x0000; i < 0x2000; i++) {
            cpuMemoryBus.Write(i, 0);
        }

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

        cpu.P.InterruptDisable = true;
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
    }

    private void ReleaseUnmanagedResources() {
        _memory.Dispose();
        _ioRegisters.Dispose();
    }

    private void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if (disposing) {
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~CpuBusDevice() {
        Dispose(false);
    }
}
