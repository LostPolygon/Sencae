using ImGuiNET;

namespace Sencae.Desktop;

public class ProfilerWindow(AppWindowContext context) : AppWindow(nameof(ProfilerWindow),
    "Internal Profiler",
    context) {
    protected override void DrawContent(float deltaTime) {
        if (ImGui.Button("Reset")) {
            Profiler.Instance.Reset();
            throw new ExitGuiException();
        }

        ImGui.BeginTable("Stats", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        {
            ImGui.TableSetupColumn("Id");
            ImGui.TableSetupColumn("Average", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableSetupColumn("FPS", ImGuiTableColumnFlags.WidthFixed, 130);
            ImGui.TableHeadersRow();

            DateTime utcNow = DateTime.UtcNow;
            
            var sortedEntries = Profiler.Instance.Entries.OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase);
            foreach (var pair in sortedEntries) {
                bool isStale = utcNow - pair.Value.UpdatedAt > TimeSpan.FromSeconds(0.3);
                
                ImGui.BeginDisabled(isStale);
                
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.Text(pair.Key);

                ImGui.PushFont(Context.SharedResources.FontMonospacedNormal);
                {
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{pair.Value.Average:0.000} {pair.Value.Options.TimeUnit.Name}");
                    
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text($"{(1.0 / pair.Value.AverageRaw):0.00} FPS");
                }
                ImGui.PopFont();
                
                ImGui.EndDisabled();
            }
        }
        ImGui.EndTable();
    }
}
