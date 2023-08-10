using System;
using SDL2;

namespace Sencae;

public static unsafe class SdlUtility {
    public static bool SDL_MUSTLOCK(IntPtr surface) {
        SDL.SDL_Surface* surfaceCast = (SDL.SDL_Surface*) surface;
        return (surfaceCast->flags & 2U) > 0U;
    }
}
