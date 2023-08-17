using System.Runtime.CompilerServices;

namespace Sencae;
public unsafe struct PpuState {
    public fixed byte  Memory[2048];

    public fixed byte Registers[8];
}
