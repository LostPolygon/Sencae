using ImGuiNET;

namespace Sencae.Desktop;

public class DearImGuiDemoWindow(AppWindowContext context) : AppWindow(
    nameof(DearImGuiDemoWindow),
    "Dear ImGui Demo",
    context
) {
    protected override void DrawWindow(float deltaTime) {
        ImGui.ShowDemoWindow();
    }

    protected override void DrawContent(float deltaTime) {
        throw new NotImplementedException();
    }
}
