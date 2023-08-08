namespace NGChip8Emulator; 

public unsafe struct CpuState {
    public fixed byte Memory[2048];
        
    // General purpose registers
    public byte A;
    public byte X;
    public byte Y;

    public StatusRegister P;

    // Program Counter
    public UInt16Bytes PC;

    // Stack pointer
    public byte SP;

    public bool Halted;

    public CpuInternalState Internal;

    public struct StatusRegister {
        public bool Carry;          // 0 - C, Carry (0=No Carry, 1=Carry)
        public bool Zero;           // 1 - Z, Zero (0=Nonzero, 1=Zero)
        public bool IrqDisable;     // 2 - I, IRQ Disable (0=IRQ Enable, 1=IRQ Disable)
        public bool DecimalMode;    // 3 - D, Decimal Mode (0=Normal, 1=BCD Mode for ADC/SBC opcodes)
        public bool BreakFlag;      // 4 - B, Break Flag (0=IRQ/NMI, 1=BRK/PHP opcode)
        public bool Unused;         // 5 - -, Not used (Always 1)
        public bool Overflow;       // 6 - V, Overflow (0=No Overflow, 1=Overflow)
        public bool NegativeOrSign; // 7 - N, Negative/Sign (0=Positive, 1=Negative)
    }


}