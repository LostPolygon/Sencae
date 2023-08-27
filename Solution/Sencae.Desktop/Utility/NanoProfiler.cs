using System.Collections.Concurrent;
using System.Diagnostics;
using Perfolizer.Horology;

namespace Sencae.Desktop; 

public class NanoProfiler<TKey> {
    private const double ExponentialMovingAverageFactor = 40; // looks good!
    
    private readonly ConcurrentDictionary<TKey, ProfilerEntry> _entries = new();

    public IReadOnlyDictionary<TKey, ProfilerEntry> Entries => _entries;

    public ProfileBeginMarker Begin(TKey key, TimeUnit? timeUnit = null, bool showFps = false) {
        ProfilerEntry entry = GetProfilerEntry(key, timeUnit, showFps);
        entry.Stopwatch.Restart();

        return new ProfileBeginMarker(this, key);
    }
    
    public void End(TKey key) {
        if (!_entries.TryGetValue(key, out ProfilerEntry? entry))
            throw new InvalidOperationException("Trying to End a profile without Start");
        
        entry.Stopwatch.Stop();
        entry.AddValue(entry.Stopwatch.Elapsed.TotalSeconds);
        
        entry.Stopwatch.Reset();
    }

    /*public void AddRaw(TKey key, double elapsed, TimeUnit? timeUnit = null, bool showFps = false) {
        ProfilerEntry entry = GetProfilerEntry(key, timeUnit);
        entry.AddValue(elapsed);
    }*/

    public void Reset() {
        _entries.Clear();
    }
    
    private ProfilerEntry GetProfilerEntry(TKey key, TimeUnit? timeUnit, bool showFps) {
        ProfileEntryOptions options = new(
            timeUnit ?? TimeUnit.Millisecond,
            showFps
        );
        
        ProfilerEntry entry = _entries.GetOrAdd(key, static (_, options) => new ProfilerEntry(options), options);
        return entry;
    }

    public class ProfilerEntry(ProfileEntryOptions options) {
        public Stopwatch Stopwatch { get; } = new();
        public ProfileEntryOptions Options { get; } = options;
        public ulong Count { get; private set; }
        public double AverageRaw { get; private set; }
        public double Average => TimeUnit.Convert(AverageRaw, TimeUnit.Second, Options.TimeUnit);

        public void AddValue(double elapsed) {
            Count++;
            AverageRaw += (elapsed - AverageRaw) / Math.Min(Count, ExponentialMovingAverageFactor);
        }

        public void Reset() {
            Count = 0;
            AverageRaw = 0;
        }
    }

    public readonly record struct ProfileBeginMarker(NanoProfiler<TKey> Profiler, TKey Key) : IDisposable {
        public void Dispose() {
            Profiler.End(Key);
        }
    }

    public readonly record struct ProfileEntryOptions(
        TimeUnit TimeUnit,
        bool ShowFps
    );
}
