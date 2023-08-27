using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public abstract class AppWindow {
    private readonly AnimationTimer _shadowAnimation = new(0.1f);

    private bool _isOpen = true;

    public string Id { get; protected set; }
    public string Title { get; set; }
    public ImGuiWindowFlags Flags { get; set; }
    protected AppWindowContext Context { get; }

    protected AppWindow(string id, string title, AppWindowContext context) {
        Context = context;
        Id = id;
        Title = title;
    }

    public bool IsOpen {
        get => _isOpen;
        set => _isOpen = value;
    }

    public void Draw(float deltaTime) {
        if (!_isOpen) 
            return;
        
        bool collapsed = !ImGui.Begin($"{Title}##{Id}", ref _isOpen, Flags);
        if (!collapsed) {
            DrawContent(deltaTime);
            DrawWindowDropShadow(deltaTime, false);
            ImGui.End();
        } else {
            DrawWindowDropShadow(deltaTime, true);
        }
    }

    protected abstract void DrawContent(float deltaTime);

    private void DrawWindowDropShadow(float deltaTime, bool collapsed) {
        float shadowSize = collapsed ? 25 : 70;
        float shadowInset = collapsed ? 2.75f : 5.25f;
        float shadowAlpha = 100f * _shadowAnimation.CurrentProgress;
        _shadowAnimation.Update(ImGui.IsWindowFocused() ? deltaTime : -deltaTime);
        GuiVisualUtility.DrawCurrentWindowDropShadow(Context.SharedResources.ShadowTexture, shadowSize, (byte) shadowAlpha, shadowInset);
    }
}
