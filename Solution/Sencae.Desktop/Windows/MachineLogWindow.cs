using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using ImGuiNET;
using Sencae.SharedUtility;

namespace Sencae.Desktop;

public unsafe class MachineLogWindow : AppWindow {
    public Emulator Emulator { get; }

    private readonly List<MachineLogEntry> _logOutput = new();
    private bool _autoScroll;
    private bool _logging;
    private bool _autoScrollTrigger;

    public MachineLogWindow(AppWindowContext context, Emulator emulator) : base(
        nameof(MachineLogWindow),
        "Machine Log",
        context
    ) {
        Emulator = emulator;
    }

    protected override void DrawWindow(float deltaTime) {
        ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, 30);
        base.DrawWindow(deltaTime);
        ImGui.PopStyleVar(2);
    }

    protected override void DrawContent(float deltaTime) {
        ImGui.SetWindowSize(new Vector2(150, 300), ImGuiCond.FirstUseEver);

        if (!Emulator.IsLoaded)
            return;

        ImGui.Separator();

        ImGui.BeginDisabled(_logging);
        if (ImGui.Button("Start")) {
            Emulator.Machine.CpuClocked -= OnCpuClocked;
            Emulator.Machine.CpuClocked += OnCpuClocked;
            ClearLog();

            _logging = true;
        }

        ImGui.EndDisabled();

        ImGui.SameLine();

        ImGui.BeginDisabled(!_logging);
        if (ImGui.Button("Stop")) {
            Emulator.Machine.CpuClocked -= OnCpuClocked;
            _logging = false;
        }

        ImGui.EndDisabled();

        ImGui.SameLine();
        ImGui.TextUnformatted($"Logging: {_logging}");

        ImGui.SameLine();
        ImGui.TextUnformatted($"Entries: {_logOutput.Count}");

        ImGui.Checkbox("Auto-scroll: ", ref _autoScroll);

        if (ImGui.Button("Save To File")) {
            string fileName = $"MachineLog_{DateTime.UtcNow.ToFileTimeUtc()}.txt";
            string text = String.Join(
                Environment.NewLine,
                _logOutput.Select(NintendulatorLog.FormatMachineLogEntry)
            );
            File.WriteAllText(fileName, text);
        }

        if (ImGui.Button("Clear")) {
            ClearLog();
        }

        ImGui.Separator();

        DrawLog();
    }

    private void DrawLog() {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0)); // Tighten spacing
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

        ImGui.PushFont(Context.SharedResources.FontMonospacedNormal);

        ImGuiListClipper clipperRaw = new();
        ImGuiListClipperPtr clipper = new(&clipperRaw);

        clipper.Begin(_logOutput.Count, ImGui.GetTextLineHeightWithSpacing());

        Span<char> logText = stackalloc char[95];
        Span<byte> logTextUtf8 = stackalloc byte[95];
        while (clipper.Step()) {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++) {
                NintendulatorLog.FormatMachineLogEntry(_logOutput[i], logText, out int charsWritten);
                int logTextUtf8BytesWritten = Encoding.UTF8.GetBytes(logText[..charsWritten], logTextUtf8);
                Debug.Assert(logTextUtf8BytesWritten >= charsWritten);

                byte* logTextUtf8Ptr = (byte*) Unsafe.AsPointer(ref logTextUtf8[0]);
                ImGuiNative.igTextUnformatted(logTextUtf8Ptr, logTextUtf8Ptr + logTextUtf8BytesWritten);
            }
        }

        if (_autoScroll && _autoScrollTrigger && ImGui.GetScrollY() < ImGui.GetScrollMaxY()) {
            ImGui.SetScrollHereY(1.0f);
            _autoScrollTrigger = false;
        }

        clipper.End();

        ImGui.PopStyleVar(2);
        ImGui.PopFont();
    }

    protected override void OnClosing() {
        if (!Emulator.IsLoaded)
            return;

        Emulator.Machine.CpuClocked -= OnCpuClocked;
        _logging = false;
    }

    private void OnCpuClocked() {
        if (!NintendulatorLog.IsInstructionStartCycle(Emulator.Machine))
            return;

        MachineLogEntry logEntry = NintendulatorLog.CreateMachineLogEntry(Emulator.Machine);
        _logOutput.Add(logEntry);
        _autoScrollTrigger = true;
    }

    private void ClearLog() {
        _logOutput.Clear();
        _logOutput.Capacity = 1024;
    }
}
