namespace Sencae.Desktop;

public class Emulator : IDisposable {
    public NesConstants.NesVariantConstants NesVariantConstants { get; }
    public Machine Machine { get; }

    public bool IsRunning { get; set; }
    
    public ulong RemainingCycles { get; private set; }
    
    public double TimeUntilNextFrame { get; private set; }
    
    public Emulator(string romFilePath, NesConstants.NesVariantConstants nesVariantConstants) {
        NesVariantConstants = nesVariantConstants;
        CartridgeData cartridgeData = NesFileParser.Parse(File.ReadAllBytes(romFilePath));

        Machine = new(CartridgeDeviceFactory.Create(cartridgeData));
        Machine.Reset();
    }

    public void Update(double deltaTime) {
        if (!IsRunning)
            return;
        
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

    public void Reset() {
        Machine.Reset();
    }

    public void SingleCycleStep() {
        Machine.CpuClock();
    }

    public void SingleInstructionStep() {
        do {
            Machine.CpuClock();
        } while (Machine.State.Cpu.Internal.State != CpuInternalState.ExecutionState.InterpretOpcode);
    }

    public void Dispose() {
        Machine.Dispose();
    }
}
