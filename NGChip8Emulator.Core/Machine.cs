namespace NGChip8Emulator; 

public class Machine {
    public MachineState State;
    public CpuMemoryBus CpuBus;
    public Cartridge Cartridge;

    public Machine(Cartridge cartridge) {
        Cartridge = cartridge;
        CpuBus = new CpuMemoryBus(this);
    }

    public void Reset() {
        State.Cpu.A = 0xAA;
        State.Cpu.X = 0xC0;
        State.Cpu.Y = 0x0;
        State.Cpu.SP = 0xBD;
        
        ushort resetVector = CpuBus.GetU16(0xFFFC);
        State.Cpu.Internal.State = CpuInternalState.ExecutionState.InterpretOpcode;
        State.Cpu.Internal.DataBus = CpuBus.GetU8(resetVector);
        //State.Cpu.Internal.IR = State.Cpu.Internal.DataBus; 
        State.Cpu.PC.Value = (ushort) (resetVector + 0);
        State.Cpu.Internal.AddressBus.Value = (ushort) (resetVector + 0);
        State.Cpu.Internal.Read = true;
    }
}
