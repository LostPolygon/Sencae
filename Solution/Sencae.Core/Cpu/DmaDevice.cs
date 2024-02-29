using System.Runtime.CompilerServices;

namespace Sencae;

public abstract class DmaDevice(DmaDeviceState state) : IBusDevice {
    public readonly DmaDeviceState State = state;

    [MethodImpl(HotPath)]
    public void Execute(MemoryAccessMode accessMode, ref bool cpuCanExecute) {
        if (accessMode != MemoryAccessMode.Read || !State.IsActive)
            return;

        cpuCanExecute = false;
        ExecuteCycle();
    }

    [MethodImpl(HotPath)]
    protected abstract void ExecuteCycle();

    [MethodImpl(HotPath)]
    public abstract bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus);
}
