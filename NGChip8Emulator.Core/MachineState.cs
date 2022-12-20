using System.Diagnostics;
using System.Linq.Expressions;

namespace NGChip8Emulator {
    //[DebuggerDisplay("{DebuggerDisplay,nq}")]
    //[DebuggerTypeProxy(typeof(MachineStateDebugProxy))]
    public unsafe struct MachineState {
        // Machine RAM
        public fixed byte Memory[4096];

        // Machine RAM
        public fixed byte Screen[Chip8Data.ScreenWidth * Chip8Data.ScreenHeight];

        // Input
        public fixed bool Keys[16];

        public CpuState Cpu;

        public ulong ExecutedInstructionCount;

        public bool WaitingForAnyKey;
    }
}
