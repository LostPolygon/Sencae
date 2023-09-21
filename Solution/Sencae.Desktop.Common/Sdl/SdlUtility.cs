namespace Sencae.Desktop.Sdl;

public static unsafe class SdlUtility {
    public static bool SDL_MUSTLOCK(IntPtr surface) {
        SDL_Surface* surfaceCast = (SDL_Surface*) surface;
        return (surfaceCast->flags & 2U) > 0U;
    }
}
