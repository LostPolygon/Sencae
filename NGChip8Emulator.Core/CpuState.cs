namespace NGChip8Emulator {
    public unsafe struct CpuState {
        // General purpose registers
        public fixed byte V[16];

        // Delay timer
        public byte DT;

        // Sound timer
        public byte ST;

        // Index Register, used to store memory addresses
        public ushort I;

        // Program Counter
        public ushort PC;

        // Stack pointer
        public byte SP;

        public fixed ushort Stack[16];
    }
}
