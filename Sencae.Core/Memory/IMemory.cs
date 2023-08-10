using System.Runtime.CompilerServices;

namespace Sencae; 


public interface IMemory {
    ref byte GetU8(ushort address);
    
    [MethodImpl(MethodOptions.Options)]
    ref ushort GetU16(ushort address) {
        return ref Unsafe.As<byte, ushort>(ref GetU8(address));
    }
}