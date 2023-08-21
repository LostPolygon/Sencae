namespace Sencae.SharedUtility; 

public static class MachineStateFormatter {
    public static string FormatCpuStateNesTest(Machine machine) {
        return FormatCpuStateNesTest(machine.State);
    }
    
    public static string FormatCpuStateNesTest(MachineState state) {
        CpuState cpu = state.Cpu;
        return
            $"{cpu.PC.Value:X4} " +
            $"A: {cpu.A:X2} " +
            $"X: {cpu.X:X2} " +
            $"Y: {cpu.Y:X2} " +
            $"P: {(int) cpu.P.ToFlags():X2} " +
            $"SP: {cpu.SP:X2}"
            ;
    }

    public static string FormatCpuState(Machine machine) {
        return FormatCpuState(in machine.State);
    }
    
    public static string FormatCpuState(in MachineState state) {
        CpuState cpu = state.Cpu;
        
        return
            $"==== Cycle: {state.ProgramCycles}, " +
            $"AB: {cpu.Internal.AddressBus.Value:X4}, " +
            $"DB: {cpu.Internal.DataBus:X2}, " +
            $"PC: {cpu.PC.Value:X4}, " +
            $"RW: {(cpu.Internal.Read ? "R" : "W")}, " +
            $"OP: {cpu.Internal.Instruction.Mnemonic} ({cpu.Internal.Instruction.Opcode:X2}), " +
            $"A: {cpu.A:X2}, " +
            $"X: {cpu.X:X2}, " +
            $"Y: {cpu.Y:X2}, " +
            $"SP: {cpu.SP:X2}"
            ;
    }
}
