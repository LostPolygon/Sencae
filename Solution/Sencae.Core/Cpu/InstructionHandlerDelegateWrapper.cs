namespace Sencae;

public readonly unsafe struct InstructionHandlerDelegateWrapper(delegate*<int, ref CpuRegistersState, ref CpuInternalState, bool> value) {
    public readonly delegate*<int, ref CpuRegistersState, ref CpuInternalState, bool> Value = value;
}
