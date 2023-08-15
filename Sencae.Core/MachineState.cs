namespace Sencae; 

//[DebuggerDisplay("{DebuggerDisplay,nq}")]
//[DebuggerTypeProxy(typeof(MachineStateDebugProxy))]
public struct MachineState {
    public CpuState Cpu;
    public PpuState Ppu;

    public ulong ExecutedInstructionCount;
    public ulong Cycles;

    public void Reset() {
        ExecutedInstructionCount = 0;
        Cycles = 0;
    }
}