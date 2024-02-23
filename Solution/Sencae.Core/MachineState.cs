namespace Sencae; 
using Ppu;

//[DebuggerDisplay("{DebuggerDisplay,nq}")]
//[DebuggerTypeProxy(typeof(MachineStateDebugProxy))]
public sealed class MachineState {
    public CpuState Cpu = new();
    public PpuState Ppu = new();

    public ulong MachineCycles;

    public void ResetCounters() {
        MachineCycles = 0;
    }
}