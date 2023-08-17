using System.Runtime.CompilerServices;

namespace Sencae;

public interface IBusDevice {
    [MethodImpl(MethodOptions.Options)]
    bool HandleBus(ushort address, bool isWrite, ref byte bus);
}
