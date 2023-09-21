namespace Sencae.Desktop.Sdl;

using static SdlAssertions;

public abstract class SdlWindow : IDisposable {
    public SdlMainLoop MainLoop { get; }
    public IntPtr Window { get; }
    public uint WindowId { get; }
    public IntPtr Renderer { get; }

    public string Title {
        get => SDL_GetWindowTitle(Window);
        set => SDL_SetWindowTitle(Window, value);
    }

    public SdlWindow(
        SdlMainLoop mainLoop,
        string title,
        int x,
        int y,
        int w,
        int h,
        SDL_WindowFlags windowFlags,
        SDL_RendererFlags rendererFlags
    ) {
        MainLoop = mainLoop;
        Window = AssertSdlSuccess(SDL_CreateWindow(
            title,
            x,
            y,
            w,
            h,
            windowFlags
        ));
        WindowId = AssertSdlSuccess(SDL_GetWindowID(Window));
        Renderer = AssertSdlSuccess(SDL_CreateRenderer(
            Window,
            -1,
            rendererFlags
        ));
    }

    public virtual void HandleWindowSdlEvent(in SDL_Event evt) {
    }

    public virtual void MainLoopEvent(MainLoopEvent mainLoopEvent) {
    }

    private void ReleaseUnmanagedResources() {
        SDL_DestroyRenderer(Renderer);
        SDL_DestroyWindow(Window);
    }

    protected virtual void Dispose(bool disposing) {
        ReleaseUnmanagedResources();
        if (disposing) {
            // TODO release managed resources here
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SdlWindow() {
        Dispose(false);
    }
}
