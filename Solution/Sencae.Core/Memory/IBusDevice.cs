using System.Runtime.CompilerServices;

namespace Sencae;

public interface IBusDevice {
    [MethodImpl(MethodOptions.Options)]
    bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus);
}
