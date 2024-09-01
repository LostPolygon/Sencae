using Perfolizer.Horology;

namespace Sencae.Desktop;

public class Emulator(NesConstants.NesVariantConstants nesVariantConstants) : IDisposable {
    public NesConstants.NesVariantConstants NesVariantConstants { get; } = nesVariantConstants;
    public Machine? Machine { get; private set; }

    public bool IsRunning { get; set; }
    public bool IsLoaded { get; private set; }

    public ulong RemainingCycles { get; private set; }

    public double TimeUntilNextFrame { get; private set; }

    public DateTime LastCyclesPrint;
    public ulong CyclesAtLastCyclesPrint;

    public double TimeScale { get; set; } = 1.0;

    public EmulationThreadState? EmulationThreadState { get; private set; }

    public void LoadRom(string romFilePath) {
        EmulationThreadState?.StopAndJoin();

        CartridgeData cartridgeData = NesFileParser.Parse(File.ReadAllBytes(romFilePath));

        Machine newMachine = new(MapperDeviceFactory.Create(cartridgeData));

        Machine?.Dispose();
        Machine = newMachine;

        EmulationThreadState = new EmulationThreadState {
            Thread = new Thread(EmulationThread) {
                IsBackground = true,
                Name = $"Emulation Thread for '{romFilePath}'",
            }
        };

        Reset();

        IsLoaded = true;
        EmulationThreadState.Thread.Start(EmulationThreadState);
    }

    private void EmulationThread(object stateObject) {
        EmulationThreadState state = (EmulationThreadState) stateObject;

        while (!state.Interrupted) {
            double frameDeltaTime = 1.0 / nesVariantConstants.Framerate;
            double deltaTimeScaled = frameDeltaTime / TimeScale;
            lock (state.Lock) {
                using (Profiler.Begin("Emulator_MT_Update", TimeUnit.Millisecond)) {
                    Update(frameDeltaTime);
                }
            }

            //Thread.Sleep(TimeSpan.FromSeconds(deltaTimeScaled));
        }
    }

    public void Update(double deltaTime) {
        if (!IsLoaded || !IsRunning)
            return;

        double frameDuration = 1.0 / NesVariantConstants.Framerate;
        double machineCyclesPerSecond = NesVariantConstants.MachineCyclesPerFrame * NesVariantConstants.Framerate;
        ulong cycles = (ulong) (deltaTime * machineCyclesPerSecond);

        DateTime now = DateTime.UtcNow;
        TimeSpan delta = now - LastCyclesPrint;
        if (delta > TimeSpan.FromSeconds(1)) {
            double cyclesPerSecond = (Machine.State.MachineCycles - CyclesAtLastCyclesPrint) / delta.TotalSeconds;
            Console.WriteLine($"Cycles/s: {cyclesPerSecond:0}, ratio: {(cyclesPerSecond / machineCyclesPerSecond * TimeScale):0.0000}");
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
        lock (EmulationThreadState.Lock) {
            Machine.Reset(new CpuResetOptions {
                CpuMemoryResetState = CpuMemoryResetState.AllZero,
            }, hardReset);
        }
    }

    public void SingleCycleStep() {
        lock (EmulationThreadState.Lock) {
            Machine.Clock();
        }
    }

    public void SingleInstructionStep() {
        lock (EmulationThreadState.Lock) {
            do {
                Machine.Clock();
            } while (Machine.State.Cpu.Internal.State != CpuInternalState.ExecutionState.InterpretingFetchedOpcode);
        }
    }

    public void RunToInterrupt() {
        lock (EmulationThreadState.Lock) {
            do {
                Machine.Clock();
            } while (Machine.State.Cpu.Internal.Instruction.Type is not InstructionType.BRK);
        }
    }

    public void Dispose() {
        EmulationThreadState?.StopAndJoin();
        Machine?.Dispose();
    }
}
