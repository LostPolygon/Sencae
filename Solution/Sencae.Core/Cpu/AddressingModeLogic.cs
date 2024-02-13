using System.Runtime.CompilerServices;

namespace Sencae;

public static class AddressingModeLogic {
    //public delegate bool AddressingModeHandler(CpuState cpu, ref CpuInternalState cpuInternal);

    [MethodImpl(MethodOptions.Options)]
    public static bool Dispatch(CpuState cpu, ref CpuInternalState cpuInternal, AddressingMode addressingMode) {
        return addressingMode switch {
            //AddressingMode.Implied => Implied(cpu, ref cpuInternal),
            AddressingMode.Immediate => ImmediateOrRelative(cpu, ref cpuInternal),
            AddressingMode.ZeroPage => ZeroPage(cpu, ref cpuInternal),
            AddressingMode.Absolute => Absolute(cpu, ref cpuInternal),
            AddressingMode.AbsoluteXIndexed => AbsoluteXIndexed(cpu, ref cpuInternal),
            AddressingMode.AbsoluteYIndexed => AbsoluteYIndexed(cpu, ref cpuInternal),
            AddressingMode.ZeroPageIndirectXPreIndexed => ZeroPageIndirectXPreIndexed(cpu, ref cpuInternal),
            AddressingMode.ZeroPageIndirectYPostIndexed => ZeroPageIndirectYPostIndexed(cpu, ref cpuInternal),
            AddressingMode.ZeroPageXIndexed => ZeroPageXIndexed(cpu, ref cpuInternal),
            AddressingMode.ZeroPageYIndexed => ZeroPageYIndexed(cpu, ref cpuInternal),
            AddressingMode.AbsoluteIndirect => AbsoluteIndirect(cpu, ref cpuInternal),
            AddressingMode.Relative => ImmediateOrRelative(cpu, ref cpuInternal),
            _ => true
        };
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool Implied(CpuState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ImmediateOrRelative(CpuState cpu, ref CpuInternalState cpuInternal) {
        // fetch immediate value or relative offset
        cpu.PC.Value++;
        cpuInternal.AddressBus = cpu.PC;
        return true;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool Absolute(CpuState cpu, ref CpuInternalState cpuInternal) {
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
    private static bool AbsoluteXIndexed(CpuState cpu, ref CpuInternalState cpuInternal) =>
        AbsoluteIndexedInternal(cpu, ref cpuInternal, cpu.X);

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteYIndexed(CpuState cpu, ref CpuInternalState cpuInternal) =>
        AbsoluteIndexedInternal(cpu, ref cpuInternal, cpu.Y);

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPage(CpuState cpu, ref CpuInternalState cpuInternal) {
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
    private static bool ZeroPageXIndexed(CpuState cpu, ref CpuInternalState cpuInternal) =>
        ZeroPageIndexedInternal(cpu, ref cpuInternal, cpu.X);

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageYIndexed(CpuState cpu, ref CpuInternalState cpuInternal) =>
        ZeroPageIndexedInternal(cpu, ref cpuInternal, cpu.Y);

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteIndirect(CpuState cpu, ref CpuInternalState cpuInternal) {
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
    private static bool ZeroPageIndirectXPreIndexed(CpuState cpu, ref CpuInternalState cpuInternal) {
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
    private static bool ZeroPageIndirectYPostIndexed(CpuState cpu, ref CpuInternalState cpuInternal) {
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
                
                UInt16Bytes newAddressBus = default;
                newAddressBus.Value = (ushort) (cpuInternal.AddressBus.Value + cpu.Y);
                cpuInternal.PageBoundaryCrossedCarry = (sbyte) (newAddressBus.High - cpuInternal.AddressBus.High);
                cpuInternal.AddressBus.Low = newAddressBus.Low;

                return 
                    cpuInternal.PageBoundaryCrossedCarry == 0 && 
                    (cpuInternal.Instruction.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0;
            case 4:
                cpuInternal.AddressBus.High = (byte) (cpuInternal.AddressBus.High + cpuInternal.PageBoundaryCrossedCarry);
                return true;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool AbsoluteIndexedInternal(CpuState cpu, ref CpuInternalState cpuInternal, byte index) {
        bool completed = Absolute(cpu, ref cpuInternal);

        switch (cpuInternal.OperationCycle) {
            case 2:
                UInt16Bytes newAddressBus = default;
                newAddressBus.Value = (ushort) (cpuInternal.AddressBus.Value + index);
                cpuInternal.PageBoundaryCrossedCarry = (sbyte) (newAddressBus.High - cpuInternal.AddressBus.High);
                cpuInternal.AddressBus.Low = newAddressBus.Low;

                return 
                    cpuInternal.PageBoundaryCrossedCarry == 0 && 
                    (cpuInternal.Instruction.Flags & InstructionFlags.HasMemoryReadWriteLogic) == 0;
            case 3:
                cpuInternal.AddressBus.High = (byte) (cpuInternal.AddressBus.High + cpuInternal.PageBoundaryCrossedCarry);
                return true;
        }

        return completed;
    }

    [MethodImpl(MethodOptions.Options)]
    private static bool ZeroPageIndexedInternal(CpuState cpu, ref CpuInternalState cpuInternal, byte index) {
        bool completed = ZeroPage(cpu, ref cpuInternal);
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
