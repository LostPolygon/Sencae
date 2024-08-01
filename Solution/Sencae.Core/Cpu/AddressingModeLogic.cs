using System.Runtime.CompilerServices;

namespace Sencae;

public static class AddressingModeLogic {
    //public delegate bool AddressingModeHandler(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal);

    [MethodImpl(HotPath)]
    public static bool Dispatch(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal, AddressingMode addressingMode) {
        return addressingMode switch {
            AddressingMode.Implied => Implied(ref cpu, ref cpuInternal),
            AddressingMode.Immediate => ImmediateOrRelative(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPage => ZeroPage(ref cpu, ref cpuInternal),
            AddressingMode.Absolute => Absolute(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteXIndexed => AbsoluteXIndexed(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteYIndexed => AbsoluteYIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageIndirectXPreIndexed => ZeroPageIndirectXPreIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageIndirectYPostIndexed => ZeroPageIndirectYPostIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageXIndexed => ZeroPageXIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageYIndexed => ZeroPageYIndexed(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteIndirect => AbsoluteIndirect(ref cpu, ref cpuInternal),
            AddressingMode.Relative => ImmediateOrRelative(ref cpu, ref cpuInternal),
            _ => true
        };
    }

    [MethodImpl(HotPath)]
    private static bool Implied(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    [MethodImpl(HotPath)]
    private static bool ImmediateOrRelative(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    [MethodImpl(HotPath)]
    private static bool Absolute(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0:
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1:
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    private static bool ZeroPage(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0:
                cpuInternal.AddressBus.Value = cpuInternal.DataBus;
                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    private static bool AbsoluteIndirect(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0: // fetch BAH
                // BAL already fetched
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1: // fetch ADL
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                break;
            case 2: // fetch ADH
                bool pageBoundaryBug = cpuInternal.Temporary.Low == 0xFF;
                cpuInternal.Temporary.Low = cpuInternal.DataBus;

                if (pageBoundaryBug) {
                    cpuInternal.AddressBus.Value = (ushort) (cpuInternal.AddressBus.Value & 0xFF00);
                } else {
                    cpuInternal.AddressBus.Value++;
                }

                break;
            case 3:
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    private static bool ZeroPageIndirectXPreIndexed(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0: // BAL → ADL
                // BAL already fetched
                cpuInternal.AddressBus.Value = cpuInternal.DataBus;

                break;
            case 1: // fetch ADL
                cpuInternal.AddressBus.Low = (byte) (cpuInternal.AddressBus.Low + cpu.X);

                break;
            case 2: // fetch ADH
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = (byte) (cpuInternal.AddressBus.Value + 1);

                break;
            case 3:
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    private static bool ZeroPageIndirectYPostIndexed(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0: // fetch ADL
                // IAL already fetched
                cpuInternal.AddressBus.Value = cpuInternal.DataBus;

                break;
            case 1: // fetch ADH
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Low = (byte) (cpuInternal.AddressBus.Low + 1);

                break;
            case 2:
                cpuInternal.AddressBus.High = cpuInternal.DataBus;
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;

                UInt16Bytes newAddressBus = default;
                newAddressBus.Value = (ushort) (cpuInternal.AddressBus.Value + cpu.Y);
                cpuInternal.PageBoundaryCrossedCarry = (sbyte) (newAddressBus.High - cpuInternal.AddressBus.High);
                cpuInternal.AddressBus.Low = newAddressBus.Low;

                return
                    cpuInternal.PageBoundaryCrossedCarry == 0 &&
                    (cpuInternal.Instruction.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0;
            case 3:
                cpuInternal.AddressBus.High = (byte) (cpuInternal.AddressBus.High + cpuInternal.PageBoundaryCrossedCarry);
                return true;
        }

        return false;
    }

    [MethodImpl(HotPath)]
    private static bool AbsoluteIndexedInternal(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal, byte index) {
        bool completed = Absolute(ref cpu, ref cpuInternal);
        switch (cpuInternal.OperationCycle) {
            case 1:
                UInt16Bytes newAddressBus = default;
                newAddressBus.Value = (ushort) (cpuInternal.AddressBus.Value + index);
                cpuInternal.PageBoundaryCrossedCarry = (sbyte) (newAddressBus.High - cpuInternal.AddressBus.High);
                cpuInternal.AddressBus.Low = newAddressBus.Low;

                return
                    cpuInternal.PageBoundaryCrossedCarry == 0 &&
                    (cpuInternal.Instruction.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0;
            case 2:
                cpuInternal.AddressBus.High = (byte) (cpuInternal.AddressBus.High + cpuInternal.PageBoundaryCrossedCarry);
                return true;
        }

        return completed;
    }

    [MethodImpl(HotPath)]
    private static bool ZeroPageIndexedInternal(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal, byte index) {
        bool completed = ZeroPage(ref cpu, ref cpuInternal);
        switch (cpuInternal.OperationCycle) {
            case 0:
                return false;
            case 1:
                cpuInternal.AddressBus.Low = (byte) (cpuInternal.AddressBus.Low + index);
                return true;
        }

        return completed;
    }

    [MethodImpl(HotPath)]
    private static bool ZeroPageXIndexed(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) =>
        ZeroPageIndexedInternal(ref cpu, ref cpuInternal, cpu.X);

    [MethodImpl(HotPath)]
    private static bool ZeroPageYIndexed(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) =>
        ZeroPageIndexedInternal(ref cpu, ref cpuInternal, cpu.Y);

    [MethodImpl(HotPath)]
    private static bool AbsoluteXIndexed(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) =>
        AbsoluteIndexedInternal(ref cpu, ref cpuInternal, cpu.X);

    [MethodImpl(HotPath)]
    private static bool AbsoluteYIndexed(ref CpuRegistersState cpu, ref CpuInternalState cpuInternal) =>
        AbsoluteIndexedInternal(ref cpu, ref cpuInternal, cpu.Y);
}
