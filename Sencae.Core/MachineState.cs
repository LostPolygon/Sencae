namespace Sencae {
    //[DebuggerDisplay("{DebuggerDisplay,nq}")]
    //[DebuggerTypeProxy(typeof(MachineStateDebugProxy))]
    public unsafe struct MachineState {
        // Machine RAM
        //public fixed byte Memory[4096];

        // Machine RAM
        //public fixed byte Screen[NesConstants.ScreenWidth * NesConstants.ScreenHeight];
        
        public CpuState Cpu;

        public ulong ExecutedInstructionCount;
        public ulong Cycle;
    }
}
