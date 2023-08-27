using System.Runtime.CompilerServices;

namespace Sencae; 

public interface IMemoryDevice {
    [MethodImpl(MethodOptions.Options)]
    ref byte GetU8(ushort address);
    
    [MethodImpl(MethodOptions.Options)]
    public ref ushort GetU16(ushort address) {
        ref byte data = ref GetU8(address);
        ref ushort data16 = ref Unsafe.As<byte, ushort>(ref data);
        return ref data16;
    }
}