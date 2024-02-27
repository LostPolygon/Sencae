namespace Sencae.Desktop;

public class Emulator : IDisposable {
    public NesConstants.NesVariantConstants NesVariantConstants { get; }
    public Machine Machine = null!;

    public bool IsRunning { get; set; }
    public bool IsLoaded { get; private set; }
    
    public ulong RemainingCycles { get; private set; }
    
    public double TimeUntilNextFrame { get; private set; }

    public DateTime LastCyclesPrint;
    public ulong CyclesAtLastCyclesPrint;

    public double TimeScale { get; set; } = 1.0;
    
    public Emulator(NesConstants.NesVariantConstants nesVariantConstants) {
        NesVariantConstants = nesVariantConstants;
    }

    public void LoadRom(string romFilePath) {
        CartridgeData cartridgeData = NesFileParser.Parse(File.ReadAllBytes(romFilePath));

        Machine newMachine = new(MapperDeviceFactory.Create(cartridgeData));
        
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
        double machineCyclesPerSecond = NesVariantConstants.MachineCyclesPerFrame * NesVariantConstants.Framerate;
        ulong cycles = (ulong) (deltaTime * machineCyclesPerSecond);

        DateTime now = DateTime.UtcNow;
        TimeSpan delta = now - LastCyclesPrint;
        if (delta > TimeSpan.FromSeconds(1)) {
            double cyclesPerSecond = (Machine.State.MachineCycles - CyclesAtLastCyclesPrint) / delta.TotalSeconds;
            Console.WriteLine($"Cycles/s: {cyclesPerSecond:0}, ratio: {(machineCyclesPerSecond / cyclesPerSecond * TimeScale):0.0000}");
            LastCyclesPrint = now;
            CyclesAtLastCyclesPrint = Machine.State.MachineCycles;
        }

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
        Machine.Clock();
    }

    public void SingleInstructionStep() {
        do {
            Machine.Clock();
        } while (Machine.State.Cpu.Internal.State != CpuInternalState.ExecutionState.InterpretingFetchedOpcode);
    }
    
    public void RunToInterrupt() {
        do {
            Machine.Clock();
        } while (Machine.State.Cpu.Internal.Instruction.Type is not InstructionType.SpecialNmiHandler);
    }

    public void Dispose() {
        Machine?.Dispose();
    }
}
