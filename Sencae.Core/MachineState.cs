namespace Sencae; 

//[DebuggerDisplay("{DebuggerDisplay,nq}")]
//[DebuggerTypeProxy(typeof(MachineStateDebugProxy))]
public class MachineState {
    public CpuState Cpu = new CpuState();
    public PpuState Ppu = new PpuState();

    public ulong ExecutedInstructionCount;
    public ulong Cycles;
    public ulong ProgramCycles;

    public void Reset() {
        ExecutedInstructionCount = 0;
        Cycles = 7;
        ProgramCycles = 0;
    }
}