namespace Sencae.Desktop.Sdl;

using static SDL;
using static SDL_EventType;
using static SdlAssertions;

public static class SdlInitializer {
    public static void Initialize() {
        // Create window
        SDL_SetHint("SDL_WINDOWS_DPI_AWARENESS", "permonitorv2");
        SDL_SetHint("SDL_WINDOWS_DPI_SCALING", "1");

        //Setup SDL hint
        SDL_SetHint(SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
        SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "best");
        
        AssertSdlSuccessZero(SDL_Init(
            SDL_INIT_VIDEO |
            //SDL_INIT_AUDIO |
            SDL_INIT_TIMER |
            SDL_INIT_GAMECONTROLLER
        ));
        
        // Allow listening for window messages on Windows
        SDL_EventState(SDL_SYSWMEVENT, SDL_ENABLE);
    }
}
