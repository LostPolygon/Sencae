using ImGuiNET;
using Sencae.Desktop.Sdl;

namespace Sencae.Desktop; 

public abstract class AppSdlWindow : BaseAppSdlWindow {
    protected AppSdlWindow(
        SdlMainLoop mainLoop, 
        string title, 
        int x,
        int y,
        int w, 
        int h,
        SDL_WindowFlags windowFlags,
        SDL_RendererFlags rendererFlags
    ) : base(mainLoop, title, x, y, w, h, windowFlags, rendererFlags) {
    }

    protected override NanoProfiler<string> Profiler { get; } = Sencae.Desktop.Profiler.Instance;

    protected override void RenderImGui() {
        SDL_RenderSetScale(Renderer, ImGuiGlobals.IO.DisplayFramebufferScale.X, ImGuiGlobals.IO.DisplayFramebufferScale.Y);
        base.RenderImGui();
    }

    public override void MainLoopEvent(MainLoopEvent mainLoopEvent) {
        base.MainLoopEvent(mainLoopEvent);

        switch (mainLoopEvent) {
            case Sdl.MainLoopEvent.Ending:
                ImGui.SaveIniSettingsToDisk(ImGuiGlobals.IO.IniFilename);
                break;
        }
    }
}
