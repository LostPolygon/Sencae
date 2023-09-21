using System;
using System.Buffers.Binary;
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
    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        switch (address) {
            case >= 0x0000 and <= 0x1FFF: {
                ref byte mappedData = ref _memory.Data[address % 0x800];
                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                return true;
            }
            case >= 0x4000 and <= 0x401F: {
                ref byte mappedData = ref _ioRegisters.Data[address % (0x18 + 0x08)];
                BusDeviceHandlers.ReadWriteRaw(ref mappedData, accessMode, ref bus);
                if (address is 0x4016 or 0x4017 && accessMode != MemoryAccessMode.Write) {
                    bus = 0;
                }
                return true;
            }
        }

        return false;
    }

    public void HardReset(CpuResetOptions options) {
        ref CpuState cpu = ref Machine.State.Cpu;
        cpu = new CpuState();

        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpu.P = new CpuStatusRegisterFlags {
            InterruptDisable = true,
            BreakFlag = true
        };

        cpu.A = cpu.X = cpu.Y = 0x00;
        cpu.SP = 0xFD;

        switch (options.CpuMemoryResetState) {
            case CpuMemoryResetState.Random:
                for (ushort i = 0x0000; i < 0x2000; i++) {
                    cpuMemoryBus.Write(i, (byte) Random.Shared.Next());
                }
                break;
            case CpuMemoryResetState.Pattern:
                Span<byte> pattern = stackalloc byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
                
                for (ushort i = 0x0000; i < 0x2000; i++) {
                    cpuMemoryBus.Write(i, pattern[i % 4]);
                }
                break;
            case CpuMemoryResetState.AllZero:
                for (ushort i = 0x0000; i < 0x2000; i++) {
                    cpuMemoryBus.Write(i, 0);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        cpuMemoryBus.Write(0x4017, 0); // frame irq enabled
        cpuMemoryBus.Write(0x4015, 0); // all APU channels disabled

        for (ushort i = 0x4000; i <= 0x400F; i++) {
            cpuMemoryBus.Write(i, 0);
        }

        for (ushort i = 0x4010; i <= 0x4013; i++) {
            cpuMemoryBus.Write(i, 0);
        }

        GenericReset(options);
    }

    public void SoftReset(CpuResetOptions options) {
        ref CpuState cpu = ref Machine.State.Cpu;
        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpu.P.InterruptDisable = true;
        cpu.SP -= 3;

        GenericReset(options);
    }

    private void GenericReset(CpuResetOptions options) {
        ref CpuState cpu = ref Machine.State.Cpu;
        CpuMemoryBus cpuMemoryBus = Machine.CpuMemoryBus;

        cpuMemoryBus.Write(0x4015, 0); // all APU channels disabled

        ushort resetVector = options.OverrideResetVector ?? cpuMemoryBus.ReadU16(CpuConstants.ResetVector);
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
