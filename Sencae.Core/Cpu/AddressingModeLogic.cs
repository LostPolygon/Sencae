using System;
using System.Runtime.CompilerServices;

namespace Sencae;

public static class AddressingModeLogic {
    //public delegate bool AddressingModeHandler(ref CpuState cpu, ref CpuInternalState cpuInternal);

    [MethodImpl(MethodOptions.Options)]
    public static bool Execute(ref CpuState cpu, ref CpuInternalState cpuInternal, AddressingMode addressingMode) {
        return addressingMode switch {
            AddressingMode.Implied => Implied(ref cpu, ref cpuInternal),
            AddressingMode.Immediate => Immediate(ref cpu, ref cpuInternal),
            AddressingMode.Absolute => Absolute(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteXIndexed => AbsoluteXIndexed(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteYIndexed => AbsoluteYIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPage => ZeroPage(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageXIndexed => ZeroPageXIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageYIndexed => ZeroPageYIndexed(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteIndirect => AbsoluteIndirect(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageIndirectXPreIndexed => ZeroPageIndirectXPreIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageIndirectYPostIndexed => ZeroPageIndirectYPostIndexed(ref cpu, ref cpuInternal),
            AddressingMode.Relative => Relative(ref cpu, ref cpuInternal),

            //_ => throw new ArgumentOutOfRangeException(nameof(addressingMode), addressingMode, null)
        };
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool Implied(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool Immediate(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool Absolute(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1:
                cpuInternal.Temporary = cpuInternal.DataBus;
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 2:
                cpuInternal.AddressBus.Low = (byte) cpuInternal.Temporary;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteXIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return AbsoluteIndexedInternal(ref cpu, ref cpuInternal, cpu.X);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteYIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return AbsoluteIndexedInternal(ref cpu, ref cpuInternal, cpu.Y);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPage(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1:
                cpuInternal.AddressBus = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageXIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return ZeroPageIndexedInternal(ref cpu, ref cpuInternal, cpu.X);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageYIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return ZeroPageIndexedInternal(ref cpu, ref cpuInternal, cpu.Y);
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteIndirect(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0: // fetch BAL
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1: // fetch BAH
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 2: // fetch ADL
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                break;
            case 3: // fetch ADH
                bool pageBoundaryBug = cpuInternal.Temporary.Low == 0xFF;
                cpuInternal.Temporary.Low = cpuInternal.DataBus;

                if (pageBoundaryBug) {
                    cpuInternal.AddressBus.Value = (ushort) (cpuInternal.AddressBus.Value & 0xFF00);
                } else {
                    cpuInternal.AddressBus.Value++;
                }

                break;
            case 4:
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageIndirectXPreIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0: // fetch BAL
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1: // fetch ADL
                cpuInternal.AddressBus.Value = (byte) (cpuInternal.DataBus + cpu.X);

                break;
            case 2: // fetch ADH
                cpuInternal.Temporary = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = (byte) (cpuInternal.AddressBus.Value + 1);

                break;
            case 3:
                cpuInternal.AddressBus.Low = (byte) cpuInternal.Temporary;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageIndirectYPostIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1: // fetch ADL
                cpuInternal.AddressBus.Value = cpuInternal.DataBus;

                break;
            case 2: // fetch ADH
                cpuInternal.Temporary = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = (byte) (cpuInternal.AddressBus.Value + 1);

                break;
            case 3:
                cpuInternal.AddressBus.Low = (byte) cpuInternal.Temporary;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = (ushort) (cpuInternal.AddressBus.Value + cpu.Y);

                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool Relative(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0: // fetch offset
                cpu.Internal.Read = true;
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteIndexedInternal(ref CpuState cpu, ref CpuInternalState cpuInternal, byte index) {
        bool completed = Absolute(ref cpu, ref cpuInternal);

        // FIXME: add extra cycle on page cross
        if (completed) {
            cpuInternal.AddressBus.Value += index;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageIndexedInternal(ref CpuState cpu, ref CpuInternalState cpuInternal, byte index) {
        bool completed = ZeroPage(ref cpu, ref cpuInternal);
        if (completed) {
            cpuInternal.AddressBus.Value = (byte) (cpuInternal.AddressBus.Value + index);
        }

        return completed;
    }
}
