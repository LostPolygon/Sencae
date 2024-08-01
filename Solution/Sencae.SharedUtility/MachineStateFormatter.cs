namespace Sencae.SharedUtility; 

public static class MachineStateFormatter {
    public static string FormatCpuStateNesTest(Machine machine) {
        return FormatCpuStateNesTest(machine.State);
    }
    
    public static string FormatCpuStateNesTest(MachineState state) {
        ref CpuRegistersState cpu = ref state.Cpu.Registers;
        
        return
            $"{cpu.PC.Value:X4} " +
            $"A: {cpu.A:X2} " +
            $"X: {cpu.X:X2} " +
            $"Y: {cpu.Y:X2} " +
            $"P: {(int) cpu.P.AsFlags:X2} " +
            $"SP: {cpu.SP:X2}"
            ;
    }

    public static string FormatCpuState(Machine machine) {
        return FormatCpuState(in machine.State);
    }
    
    public static string FormatCpuState(in MachineState state) {
        ref CpuRegistersState cpu = ref state.Cpu.Registers;
        ref CpuInternalState cpuInternal = ref state.Cpu.Internal;
        
        return
            $"Cycle: {state.Cpu.Internal.Cycles}, " +
            $"AB: {cpuInternal.AddressBus.Value:X4}, " +
            $"DB: {cpuInternal.DataBus:X2}, " +
            $"PC: {cpu.PC.Value:X4}, " +
            $"RW: {(cpuInternal.Read ? "R" : "W")}, " +
            $"OP: {cpuInternal.Instruction.Mnemonic} ({cpuInternal.Instruction.Opcode:X2}), " +
            $"A: {cpu.A:X2}, " +
            $"X: {cpu.X:X2}, " +
            $"Y: {cpu.Y:X2}, " +
            $"SP: {cpu.SP:X2}"
            ;
    }
}
