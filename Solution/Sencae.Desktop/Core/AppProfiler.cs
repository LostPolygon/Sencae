using Perfolizer.Horology;

namespace Sencae.Desktop; 

public static class AppProfiler {
    public static NanoProfiler<string> Profiler { get; } = new();

    public static NanoProfiler<string>.ProfileBeginMarker Begin(string key, TimeUnit? timeUnit = null, bool showFps = false) {
        return Profiler.Begin(key, timeUnit, showFps);
    }
    
    public static void End(string key) {
        Profiler.End(key);
    }
}
