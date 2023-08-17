using System.Runtime.CompilerServices;

namespace Sencae;

public class CpuState {
    public byte[] Memory = new byte[2048];
    public byte[] IORegisters = new byte[32];

    // General purpose registers
    public byte A;
    public byte X;
    public byte Y;

    public CpuStatusRegisterFlags P;

    // Program Counter
    public UInt16Bytes PC;

    // Stack pointer
    public byte SP;

    public CpuInternalState Internal;

    public struct CpuStatusRegisterFlags2 {
        public bool Carry;
        public bool Zero;
        public bool InterruptDisable;
        public bool DecimalMode;
        public bool BreakFlag;
        public bool Unused;
        public bool Overflow;
        public bool NegativeOrSign;
    }
}
