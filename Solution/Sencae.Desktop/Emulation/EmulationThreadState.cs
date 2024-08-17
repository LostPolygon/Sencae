namespace Sencae.Desktop;

public class EmulationThreadState {
    public required Thread Thread { get; init; }
    public readonly object Lock = new();
    public volatile bool Interrupted;

    public void StopAndJoin() {
        Interrupted = true;
        if (Thread.ThreadState is not ThreadState.Stopped and not ThreadState.Unstarted) {
            Thread.Join();
        }
    }
}
