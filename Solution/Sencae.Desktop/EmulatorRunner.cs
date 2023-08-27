using Sencae.Desktop.Sdl;

namespace Sencae.Desktop;

public class EmulatorRunner {
    public void Run(string[] args) {
        SdlInitializer.Initialize();
        ImGuiGlobals.Initialize();

        {
            using MainAppSdlWindow mainWindow = new(
                "Sencae",
                SDL_WINDOWPOS_CENTERED,
                SDL_WINDOWPOS_CENTERED,
                1280,
                720,
                SDL_WindowFlags.SDL_WINDOW_SHOWN |
                SDL_WindowFlags.SDL_WINDOW_RESIZABLE |
                SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI |
                SDL_WindowFlags.SDL_WINDOW_OPENGL,
                SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC,
                args
            );

            mainWindow.StartMainLoop();
        }

        SDL_Quit();
    }
}
