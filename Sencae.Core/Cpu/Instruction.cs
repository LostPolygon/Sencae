namespace Sencae {
    public ref struct Instruction {
        public int opcode;
        public byte op;
        public ushort nnn;
        public byte n;
        public byte x;
        public byte y;
        public byte kk;

        public void Decode(int opcode) {
            this.opcode = opcode;
            op = (byte) ((opcode & 0xF000) >> 12);
            nnn = (ushort) (opcode & 0x0FFF);
            n = (byte) (opcode & 0x000F);
            x = (byte) ((opcode & 0x0F00) >> 8);
            y = (byte) ((opcode & 0x00F0) >> 4);
            kk = (byte) (opcode & 0x00FF);
        }

        public override string ToString() {
            return
                $"Raw: {opcode:X}, " +
                $"{nameof(op)}: {op:X}, " +
                $"{nameof(nnn)}: {nnn}, " +
                $"{nameof(n)}: {n}, " +
                $"{nameof(x)}: {x}, " +
                $"{nameof(y)}: {y}, " +
                $"{nameof(kk)}: {kk}";
        }
    }
}
