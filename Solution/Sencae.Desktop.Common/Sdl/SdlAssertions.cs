namespace Sencae.Desktop.Sdl; 

public static class SdlAssertions {
    public static int AssertSdlSuccessZero(int code) {
        if (code == 0)
            return code;

        throw new Exception(SDL_GetError());
    }

    public static SDL_bool AssertSdlSuccess(SDL_bool code) {
        if (code != SDL_bool.SDL_FALSE)
            return code;

        throw new Exception(SDL_GetError());
    }
    
    public static SDL_bool AssertSdlSuccessZero(SDL_bool code) {
        if (code == SDL_bool.SDL_FALSE)
            return code;

        throw new Exception(SDL_GetError());
    }

    public static uint AssertSdlSuccess(uint code) {
        if (code != 0)
            return code;

        throw new Exception(SDL_GetError());
    }
    
    public static uint AssertSdlSuccessZero(uint code) {
        if (code == 0)
            return code;

        throw new Exception(SDL_GetError());
    }

    public static IntPtr AssertSdlSuccess(IntPtr ptr) {
        if (ptr != IntPtr.Zero)
            return ptr;

        throw new Exception(SDL_GetError());
    }
}