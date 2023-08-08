namespace NGChip8Emulator; 

public struct Cpu {
    public required IMemory Memory;
    public required CpuState State;

    
    
    public void PowerUp() {
        State.P = default;
        State.P.IrqDisable = true;

        State.A = State.X = State.Y = 0;
        State.SP = 0xFD;
        
        Memory.WriteU8(0x4017, 0x00); // frame irq enabled
        Memory.WriteU8(0x4015, 0x00); // all APU channels disabled
        
        Memory.WriteU8(0x4010, 0x00);
        Memory.WriteU8(0x4011, 0x00);
        Memory.WriteU8(0x4012, 0x00);
        Memory.WriteU8(0x4013, 0x00);
        
        /*
    P = $34[1] (IRQ disabled)[2]
    A, X, Y = 0
    S = $FD
    $4017 = $00 (frame irq enabled)
    $4015 = $00 (all channels disabled)
    $4000-$400F = $00
    $4010-$4013 = $00 [4]
    All 15 bits of noise channel LFSR = $0000[5]. The first time the LFSR is clocked from the all-0s state, it will shift in a 1.
    APU Frame Counter:

        2A03E, G, various clones: APU Frame Counter reset.
        2A03letterless: APU frame counter powers up at a value equivalent to 15

    Internal memory ($0000-$07FF) has unreliable startup state. Some machines may have consistent RAM contents at power-on, but others do not.

        Emulators often implement a consistent RAM startup state (e.g. all $00 or $FF, or a particular pattern), and flash carts like the PowerPak may partially or fully initialize RAM before starting a program, so an NES programmer must be careful not to rely on the startup contents of RAM.
         */
    }

    public void Reset() {
        State.SP -= 3;
        State.P.IrqDisable = true;
        Memory.WriteU8(0x4015, 0x00); // all APU channels disabled
    }
}
