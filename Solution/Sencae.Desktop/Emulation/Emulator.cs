namespace Sencae.Desktop;

public class Emulator : IDisposable {
    public NesConstants.NesVariantConstants NesVariantConstants { get; }
    public Machine Machine { get; private set; } = null!;

    public bool IsRunning { get; set; }
    public bool IsLoaded { get; private set; }
    
    public ulong RemainingCycles { get; private set; }
    
    public double TimeUntilNextFrame { get; private set; }

    public double TimeScale { get; set; } = 1.0;
    
    public Emulator(NesConstants.NesVariantConstants nesVariantConstants) {
        NesVariantConstants = nesVariantConstants;
    }

    public void LoadRom(string romFilePath) {
        CartridgeData cartridgeData = NesFileParser.Parse(File.ReadAllBytes(romFilePath));
        
        // HACK
        cartridgeData = cartridgeData with {
            ChrRom = cartridgeData.ChrRom.Length == 0 ? new byte[8 * 1024] : cartridgeData.ChrRom,
            IsChrRomWriteable = true,
        };

        Machine newMachine = new(CartridgeDeviceFactory.Create(cartridgeData));
        
        Machine?.Dispose();
        Machine = newMachine;
        Reset();

        IsLoaded = true;
    }

    public void Update(double deltaTime) {
        if (!IsLoaded || !IsRunning)
            return;

        deltaTime *= TimeScale;
        
        double frameDuration = 1.0 / NesVariantConstants.Framerate;
        double cyclesPerSecond = NesVariantConstants.CyclesPerFrame * NesVariantConstants.Framerate;
        ulong cycles = (ulong) (deltaTime * cyclesPerSecond);
        //Debug.WriteLine($"cycles: {cycles}");
        for (ulong i = 0; i < cycles; i++) {
            Machine.Clock();
        }
        
        /*TimeUntilNextFrame -= deltaTime;
        double timeOverflow = -TimeUntilNextFrame;
        if (timeOverflow > 0) {
            ulong cycles = (ulong) (timeOverflow * cyclesPerSecond);
        }
        
        Debug.WriteLine($"timeOverflow: {timeOverflow}");*/

        //double cyclesPerSecond = NesVariantConstants.CyclesPerFrame * NesVariantConstants.Framerate;
        //ulong cycles = (ulong) (deltaTime * cyclesPerSecond);
    }

    public void Reset(bool hardReset = true) {
        Machine.Reset(new CpuResetOptions {
            CpuMemoryResetState = CpuMemoryResetState.AllZero,
        }, hardReset);
    }

    public void SingleCycleStep() {
        Machine.CpuClock();
    }

    public void SingleInstructionStep() {
        do {
            Machine.CpuClock();
        } while (Machine.State.Cpu.Internal.State != CpuInternalState.ExecutionState.InterpretOpcode);
    }
    
    public void RunToInterrupt() {
        do {
            Machine.CpuClock();
        } while (Machine.State.Cpu.Internal.Instruction.Type is InstructionType.BRK or InstructionType.Internal_BRK_NMI);
    }

    public void Dispose() {
        Machine?.Dispose();
    }
}
