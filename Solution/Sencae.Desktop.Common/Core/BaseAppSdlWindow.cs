using ImGuiNET;
using Sencae.Desktop.Sdl;

namespace Sencae.Desktop;

public abstract unsafe class BaseAppSdlWindow : SdlWindow {
    protected double DeltaTime { get; private set; }
    private ulong _performanceCounterPrev;
    private ulong _performanceCounterCurrent;

    public BaseAppSdlWindow(
        SdlMainLoop mainLoop,
        string title,
        int x,
        int y,
        int w,
        int h,
        SDL_WindowFlags windowFlags,
        SDL_RendererFlags rendererFlags
    ) : base(mainLoop, title, x, y, w, h, windowFlags, rendererFlags) {
        ImGui.ImGui_ImplSDL2_InitForSDLRenderer(Window, Renderer);
        ImGuiSdl2Renderer.ImGui_ImplSDLRenderer2_Init(Renderer);
    }
    
    protected abstract NanoProfiler<string> Profiler { get; }
    
    protected virtual void RenderContent() {
    }
    
    protected virtual void RenderImGui() {
        ImGui.Render();
        ImGuiSdl2Renderer.ImGui_ImplSDLRenderer2_RenderDrawData(ImGui.GetDrawData());
    }

    public override void MainLoopEvent(MainLoopEvent mainLoopEvent) {
        base.MainLoopEvent(mainLoopEvent);
        
        switch (mainLoopEvent) {
            case Sdl.MainLoopEvent.Starting: {
                _performanceCounterCurrent = SDL_GetPerformanceCounter();
                _performanceCounterPrev = 0;
                break;
            }
            case Sdl.MainLoopEvent.Iteration: {
                try {
                    MainLoopIterationCore();
                } catch (ExitGuiException) {
                    // Ignore
                }
                break;
            }
        }
    }

    private void MainLoopIterationCore() {
        Profiler.Begin("Render_Total", showFps: true);

        Profiler.Begin("Render_ContentAndImGUI");

        ImGui.ImGui_ImplSDL2_NewFrame();
        ImGuiSdl2Renderer.ImGui_ImplSDLRenderer2_NewFrame();
        ImGui.NewFrame();

        _performanceCounterPrev = _performanceCounterCurrent;
        _performanceCounterCurrent = SDL_GetPerformanceCounter();

        DeltaTime = (_performanceCounterCurrent - _performanceCounterPrev) / (double) SDL_GetPerformanceFrequency();

        Profiler.Begin("Render_Content", showFps: true);
        RenderContent();
        Profiler.End("Render_Content");

        Profiler.Begin("Render_ImGUI");
        RenderImGui();
        Profiler.End("Render_ImGUI");

        Profiler.End("Render_ContentAndImGUI");

        Profiler.Begin("Render_Present", showFps: true);
        SDL_RenderPresent(Renderer);
        Profiler.End("Render_Present");

        Profiler.End("Render_Total");
    }
}
