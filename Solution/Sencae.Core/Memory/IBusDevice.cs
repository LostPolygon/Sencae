using System.Runtime.CompilerServices;

namespace Sencae;

public interface IBusDevice {
    [MethodImpl(HotPath)]
    bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus);
}
