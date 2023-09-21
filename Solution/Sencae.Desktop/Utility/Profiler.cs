using Perfolizer.Horology;

namespace Sencae.Desktop;

public static class Profiler {
    public static NanoProfiler<string> Instance { get; } = new();

    public static NanoProfiler<string>.ProfileBeginMarker Begin(
        string key,
        TimeUnit? timeUnit = null,
        bool showFps = false
    ) => Instance.Begin(key, timeUnit, showFps);

    public static void End(string key) => Instance.End(key);
}
