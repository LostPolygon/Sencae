namespace Sencae.SharedUtility; 

public static class MachineStateFormatter {
    public static string FormatCpuStateNesTest(MachineState machine) {
        CpuState cpu = machine.Cpu;
        return
            $"{cpu.PC.Value:X4} " +
            $"A: {cpu.A:X2} " +
            $"X: {cpu.X:X2} " +
            $"Y: {cpu.Y:X2} " +
            $"P: {(int) cpu.P:X2} " +
            $"SP: {cpu.SP:X2}"
            ;

    }

    public static string FormatCpuState(MachineState machine) {
        CpuState cpu = machine.Cpu;
        return
            $"==== Cycle: {machine.Cycle}, " +
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
