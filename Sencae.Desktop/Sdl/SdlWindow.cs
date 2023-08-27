using static Sencae.Desktop.Sdl.SdlAssertions;

namespace Sencae.Desktop.Sdl;

public abstract class SdlWindow : IDisposable {
    public IntPtr Window { get; }
    public IntPtr Renderer { get; }

    protected bool IsQuitting { get; set; }

    public string Title {
        get => SDL_GetWindowTitle(Window);
        set => SDL_SetWindowTitle(Window, value);
    }

    protected virtual void QuitRequested() {
    }

    public SdlWindow(
        string title,
        int x,
        int y,
        int w,
        int h,
        SDL_WindowFlags windowFlags,
        SDL_RendererFlags rendererFlags
    ) {
        Window = AssertSdlSuccess(SDL_CreateWindow(
            title,
            x,
            y,
            w,
            h,
            windowFlags
        ));
        Renderer = AssertSdlSuccess(SDL_CreateRenderer(
            Window,
            -1,
            rendererFlags
        ));
    }

    public virtual void StartMainLoop() {
        while (!IsQuitting) {
            while (SDL_PollEvent(out SDL_Event evt) == 1) {
                HandleSdlEvent(in evt);
            }

            MainLoop();
        }
    }

    protected virtual void MainLoop() {
    }

    protected virtual void HandleSdlEvent(in SDL_Event evt) {
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
