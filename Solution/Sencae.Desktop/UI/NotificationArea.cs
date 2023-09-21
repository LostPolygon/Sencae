using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public class NotificationArea(SharedResources sharedResources) {
    private readonly List<Notification> _notifications = new();

    public void Update(float deltaTime) {
        DateTime now = DateTime.Now;
        for (int i = _notifications.Count - 1; i >= 0; i--) {
            Notification notification = _notifications[i];
            if (notification.CreatedAt + TimeSpan.FromSeconds(notification.Duration) < now) {
                _notifications.RemoveAt(i);
            }
        }
    }

    public void Draw() {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 viewportSize = viewport.Size;

        Vector2 windowSize = new Vector2(300, 100);
        Vector2 windowPadding = new Vector2(15, 15);
        for (int i = 0; i < _notifications.Count; i++) {
            Notification notification = _notifications[i];

            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
            ImGui.SetNextWindowPos(viewportSize - windowSize - windowPadding - (Vector2.UnitY * i * (windowSize.Y + windowPadding.Y)), ImGuiCond.Always);
            if (!notification.Shown) {
                ImGui.SetNextWindowFocus();
                notification.Shown = true;
            }
            
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 4);

            ImGui.Begin(
                $"##Notification_{i}",
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoNavFocus |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoSavedSettings
            );

            if (notification.IsError) {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Error");
            } else {
                ImGui.Text("Info");
            }

            ImGui.Separator();
            ImGui.TextWrapped(notification.Message);

            GuiVisualUtility.DrawCurrentWindowDropShadow(sharedResources.ShadowTexture, 25, 100, 2f);
            ImGui.End();
            
            ImGui.PopStyleVar(1);
        }
    }

    public void ShowInfo(string message, float duration = 5) {
        Add(message, false, duration);
    }

    public void ShowError(string message, float duration = 5) {
        Add(message, true, duration);
    }

    private void Add(string message, bool isError, float duration) {
        _notifications.Add(new(DateTime.Now, duration, message, isError));
    }

    private record Notification(
        DateTime CreatedAt,
        float Duration,
        string Message,
        bool IsError
    ) {
        public bool Shown { get; set; }
    };
}
