using ImGuiNET;
using Perfolizer.Horology;
using Sencae.Desktop.Sdl;

namespace Sencae.Desktop;

public unsafe class AppSdlWindow : SdlWindow {
    protected double DeltaTime { get; private set; }
    private ulong _performanceCounterPrev;
    private ulong _performanceCounterCurrent;

    public AppSdlWindow(
        string title,
        int x,
        int y,
        int w,
        int h,
        SDL_WindowFlags windowFlags,
        SDL_RendererFlags rendererFlags
    ) : base(title, x, y, w, h, windowFlags, rendererFlags) {
        ImGui.ImGui_ImplSDL2_InitForSDLRenderer(Window, Renderer);
        Sdl2Renderer.ImGui_ImplSDLRenderer2_Init(Renderer);
    }

    protected override void HandleSdlEvent(in SDL_Event evt) {
        switch (evt.type) {
            // Wait for user to close the window or press Esc
            case SDL_EventType.SDL_MOUSEMOTION:
                break;
            case SDL_EventType.SDL_QUIT:
                IsQuitting = true;
                break;

            case SDL_EventType.SDL_KEYDOWN:
                if (evt.key.keysym.sym == SDL_Keycode.SDLK_ESCAPE) {
                    IsQuitting = true;
                }

                break;
        }

        fixed (SDL_Event* evtPtr = &evt) {
            ImGuiNative.ImGui_ImplSDL2_ProcessEvent(evtPtr);
        }
    }

    protected virtual void RenderContent() {
    }

    public override void StartMainLoop() {
        _performanceCounterCurrent = SDL_GetPerformanceCounter();
        _performanceCounterPrev = 0;

        base.StartMainLoop();
    }

    protected override void MainLoop() {
        try {
            MainLoopCore();
        } catch (ExitGuiException) {
            // Ignore
        }
    }

    private void MainLoopCore() {
        AppProfiler.Begin("Render_Total", showFps: true);

        AppProfiler.Begin("Render_ContentAndImGUI");

        ImGui.ImGui_ImplSDL2_NewFrame();
        Sdl2Renderer.ImGui_ImplSDLRenderer2_NewFrame();
        ImGui.NewFrame();

        _performanceCounterPrev = _performanceCounterCurrent;
        _performanceCounterCurrent = SDL_GetPerformanceCounter();

        DeltaTime = (_performanceCounterCurrent - _performanceCounterPrev) / (double) SDL_GetPerformanceFrequency();

        AppProfiler.Begin("Render_Content", showFps: true);
        RenderContent();
        AppProfiler.End("Render_Content");

        AppProfiler.Begin("Render_ImGUI");
        SDL_RenderSetScale(Renderer, ImGuiGlobals.IO.DisplayFramebufferScale.X, ImGuiGlobals.IO.DisplayFramebufferScale.Y);
        ImGui.Render();
        Sdl2Renderer.ImGui_ImplSDLRenderer2_RenderDrawData(ImGui.GetDrawData());
        AppProfiler.End("Render_ImGUI");

        AppProfiler.End("Render_ContentAndImGUI");

        AppProfiler.Begin("Render_Present", showFps: true);
        SDL_RenderPresent(Renderer);
        AppProfiler.End("Render_Present");

        AppProfiler.End("Render_Total");
    }
}
