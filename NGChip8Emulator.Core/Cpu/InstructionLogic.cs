using System.Runtime.CompilerServices;

namespace NGChip8Emulator;

public static class InstructionLogic {
    const int afterAddressingFinished = -1;
    const int afterNextOpcodeFetched = 100;

    [MethodImpl(MethodOptions.Options)]
    public static bool BRK(int operationCycle, ref MachineState machineState, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        if (machineState.Cycle > 3) {
            cpu.Halted = true;
        }

        return true;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool TAX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.X = cpu.A;
                break;
        }

        return false;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool INX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.X++;
                        
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpu.X & 0b10000000) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, cpu.X == 0);
                        
                break;
        }

        return false;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool ADC(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                ushort result16 = (ushort) (cpu.A + cpuInternal.DataBus + (cpu.P.HasFlag(CpuStatusRegisterFlags.Carry) ? 1 : 0));
                byte result = (byte) result16;

                cpu.P.SetFlag(CpuStatusRegisterFlags.Carry, result16 > byte.MaxValue);
                cpu.P.SetFlag(CpuStatusRegisterFlags.NegativeOrSign, (cpu.X & 0b10000000) != 0);
                cpu.P.SetFlag(CpuStatusRegisterFlags.Zero, cpu.X == 0);

                bool overflow = ((cpu.A ^ result) & (cpuInternal.DataBus ^ result) & 0b10000000) != 0;
                
                cpu.P.SetFlag(CpuStatusRegisterFlags.Overflow, overflow);
                cpu.A = result;
                break;
        }

        return false;
    }
    
    [MethodImpl(MethodOptions.Options)]
    public static bool STA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                cpuInternal.Read = false;
                cpuInternal.DataBus = cpu.A;
                return true;
            case afterNextOpcodeFetched:
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool LDA(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        switch (operationCycle) {
            case afterAddressingFinished:
                return true;
            case afterNextOpcodeFetched:
                cpu.A = cpuInternal.DataBus;
                break;
        }

        return false;
    }

    [MethodImpl(MethodOptions.Options)]
    public static bool XXX(int operationCycle, ref CpuState cpu, ref CpuInternalState cpuInternal) {
        return true;
    }
}
