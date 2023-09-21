using System.Numerics;
using ImGuiNET;
using Perfolizer.Horology;

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

    public bool IsOpen => _isOpen;

    public void Open() {
        if (_isOpen)
            return;

        OnOpening();
        _isOpen = true;
    }
    
    public void Close() {
        if (!_isOpen)
            return;

        OnClosing();
        _isOpen = false;
    }

    public void Draw(float deltaTime) {
        if (!_isOpen) 
            return;

        DrawWindow(deltaTime);
    }
    
    protected virtual void OnOpening() {
    }
    
    protected virtual void OnClosing() {
    }

    protected virtual void DrawWindow(float deltaTime) {
        bool collapsed = !ImGui.Begin($"{Title}##{Id}", ref _isOpen, Flags);
        if (!collapsed) {
            using (Profiler.Begin($"{Id}_DrawContent", TimeUnit.Millisecond)) {
                DrawContent(deltaTime);
            }

            DrawWindowDropShadow(deltaTime, false);
            ImGui.End();
        } else {
            DrawWindowDropShadow(deltaTime, true);
        }
    }

    protected abstract void DrawContent(float deltaTime);

    private void DrawWindowDropShadow(float deltaTime, bool collapsed) {
        if (ImGui.IsWindowDocked())
            return;
        
        float shadowSize = collapsed ? 25 : 70;
        float shadowInset = collapsed ? 2.75f : 5.25f;
        float shadowAlpha = 100f * _shadowAnimation.CurrentProgress;
        _shadowAnimation.Update(ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) ? deltaTime : -deltaTime);
        GuiVisualUtility.DrawCurrentWindowDropShadow(Context.SharedResources.ShadowTexture, shadowSize, (byte) shadowAlpha, shadowInset);
    }
}
