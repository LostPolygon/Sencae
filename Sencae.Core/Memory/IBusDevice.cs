using System.Runtime.CompilerServices;

namespace Sencae; 

public interface IBusDevice {
    [MethodImpl(MethodOptions.Options)]
    void HandleBus(ushort address, bool isWrite, ref byte bus);
}
