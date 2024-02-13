namespace Sencae; 
using Sencae.Ppu;

//[DebuggerDisplay("{DebuggerDisplay,nq}")]
//[DebuggerTypeProxy(typeof(MachineStateDebugProxy))]
public class MachineState {
    public CpuState Cpu = new();
    public PpuState Ppu = new();

    public ulong ExecutedInstructionCount;
    public ulong MachineCycles;
    public ulong CpuCycles;

    public void ResetCounters() {
        ExecutedInstructionCount = 0;
        MachineCycles = 0;
        CpuCycles = 0;
    }
}