using Sencae.Desktop.Sdl;

namespace Sencae.Desktop;

public class EmulatorRunner {
    public void Run(string[] args) {
        SdlInitializer.Initialize();
        ImGuiGlobals.Initialize();

        SettingsStorage<UIPersistentSettings> uiPersistentSettingsStorage = new("ui-state.json");
        uiPersistentSettingsStorage.Load();
        uiPersistentSettingsStorage.Save();
        
        UIPersistentSettings uiPersistentSettings = uiPersistentSettingsStorage.Value;

        {
            using SdlMainLoop mainLoop = new();
            
            using MainAppSdlWindow mainWindow = new(
                mainLoop,
                "Sencae",
                SDL_WINDOWPOS_CENTERED,
                SDL_WINDOWPOS_CENTERED,
                uiPersistentSettings.MainWindowSizeX,
                uiPersistentSettings.MainWindowSizeY,
                SDL_WindowFlags.SDL_WINDOW_SHOWN |
                SDL_WindowFlags.SDL_WINDOW_RESIZABLE |
                SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI |
                SDL_WindowFlags.SDL_WINDOW_OPENGL,
                SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC,
                args,
                uiPersistentSettings
            );

            mainLoop.RegisterWindow(mainWindow);
            mainLoop.Run();
        }
        
        uiPersistentSettingsStorage.Save();
        
        SDL_Quit();
        
        Console.WriteLine("Exited.");
    }
}
