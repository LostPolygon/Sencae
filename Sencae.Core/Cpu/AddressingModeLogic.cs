using System.Runtime.CompilerServices;

namespace Sencae;

public static class AddressingModeLogic {
    //public delegate bool AddressingModeHandler(ref CpuState cpu, ref CpuInternalState cpuInternal);

    [MethodImpl(MethodOptions.Options)]
    public static bool Dispatch(ref CpuState cpu, ref CpuInternalState cpuInternal, AddressingMode addressingMode) {
        return addressingMode switch {
            AddressingMode.Implied => Implied(ref cpu, ref cpuInternal),
            AddressingMode.Immediate => ImmediateOrRelative(ref cpu, ref cpuInternal),
            AddressingMode.Absolute => Absolute(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteXIndexed => AbsoluteXIndexed(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteYIndexed => AbsoluteYIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPage => ZeroPage(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageXIndexed => ZeroPageXIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageYIndexed => ZeroPageYIndexed(ref cpu, ref cpuInternal),
            AddressingMode.AbsoluteIndirect => AbsoluteIndirect(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageIndirectXPreIndexed => ZeroPageIndirectXPreIndexed(ref cpu, ref cpuInternal),
            AddressingMode.ZeroPageIndirectYPostIndexed => ZeroPageIndirectYPostIndexed(ref cpu, ref cpuInternal),
            AddressingMode.Relative => ImmediateOrRelative(ref cpu, ref cpuInternal),
            _ => true
        };
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool Implied(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ImmediateOrRelative(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        // fetch immediate value or relative offset
        cpu.PC.Value++;
        cpuInternal.AddressBus = cpu.PC;
        return true;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool Absolute(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1:
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 2:
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteXIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        AbsoluteIndexedInternal(ref cpu, ref cpuInternal, cpu.X);

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteYIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        AbsoluteIndexedInternal(ref cpu, ref cpuInternal, cpu.Y);

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPage(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0:
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1:
                cpuInternal.AddressBus.Value = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageXIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        ZeroPageIndexedInternal(ref cpu, ref cpuInternal, cpu.X);

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageYIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) =>
        ZeroPageIndexedInternal(ref cpu, ref cpuInternal, cpu.Y);

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
            case 1: // BAL → ADL
                cpuInternal.AddressBus.Value = cpuInternal.DataBus;

                break;
            case 2: // fetch ADL
                cpuInternal.AddressBus.Low = (byte) (cpuInternal.AddressBus.Low + cpu.X);

                break;
            case 3: // fetch ADH
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Value = (byte) (cpuInternal.AddressBus.Value + 1);

                break;
            case 4:
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                cpuInternal.AddressBus.High = cpuInternal.DataBus;

                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageIndirectYPostIndexed(ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (cpuInternal.OperationCycle) {
            case 0: // fetch IAL
                cpu.PC.Value++;
                cpuInternal.AddressBus = cpu.PC;
                break;
            case 1: // fetch ADL
                cpuInternal.AddressBus.Value = cpuInternal.DataBus;

                break;
            case 2: // fetch ADH
                cpuInternal.Temporary.Low = cpuInternal.DataBus;
                cpuInternal.AddressBus.Low = (byte) (cpuInternal.AddressBus.Low + 1);
 
                break;
            case 3:
                cpuInternal.AddressBus.High = cpuInternal.DataBus;
                cpuInternal.AddressBus.Low = cpuInternal.Temporary.Low;
                
                UInt16Bytes newAB = default;
                newAB.Value = (ushort) (cpuInternal.AddressBus.Value + cpu.Y);
                cpuInternal.PageBoundaryCrossedCarry = (sbyte) (newAB.High - cpuInternal.AddressBus.High);
                cpuInternal.AddressBus.Low = newAB.Low;

                return 
                    cpuInternal.PageBoundaryCrossedCarry == 0 && 
                    !cpuInternal.Instruction.Flags.HasFlag(InstructionFlags.HasMemoryReadWriteLogic);
            case 4:
                cpuInternal.AddressBus.High = (byte) (cpuInternal.AddressBus.High + cpuInternal.PageBoundaryCrossedCarry);
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteIndexedInternal(ref CpuState cpu, ref CpuInternalState cpuInternal, byte index) {
        bool completed = Absolute(ref cpu, ref cpuInternal);

        switch (cpuInternal.OperationCycle) {
            case 2:
                UInt16Bytes newAB = default;
                newAB.Value = (ushort) (cpuInternal.AddressBus.Value + index);
                cpuInternal.PageBoundaryCrossedCarry = (sbyte) (newAB.High - cpuInternal.AddressBus.High);
                cpuInternal.AddressBus.Low = newAB.Low;

                return 
                    cpuInternal.PageBoundaryCrossedCarry == 0 && 
                    !cpuInternal.Instruction.Flags.HasFlag(InstructionFlags.HasMemoryReadWriteLogic);
            case 3:
                cpuInternal.AddressBus.High = (byte) (cpuInternal.AddressBus.High + cpuInternal.PageBoundaryCrossedCarry);
                return true;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageIndexedInternal(ref CpuState cpu, ref CpuInternalState cpuInternal, byte index) {
        bool completed = ZeroPage(ref cpu, ref cpuInternal);
        switch (cpuInternal.OperationCycle) {
            case 1:
                cpuInternal.AddressBus.Low = (byte) (cpuInternal.AddressBus.Low + index);
                return false;
            case 2:
                return true;
        }

        return completed;
    }
}
