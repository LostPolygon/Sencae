namespace Sencae.Desktop.Sdl; 

public static class SdlAssertions {
    public static int AssertSdlSuccess(int code) {
        if (code == 0)
            return code;

        throw new Exception(SDL.SDL_GetError());
    }

    public static uint AssertSdlSuccess(uint code) {
        if (code == 0)
            return code;

        throw new Exception(SDL.SDL_GetError());
    }

    public static uint AssertSdlSuccessNonZero(uint code) {
        if (code != 0)
            return code;

        throw new Exception(SDL.SDL_GetError());
    }

    public static IntPtr AssertSdlSuccess(IntPtr ptr) {
        if (ptr != IntPtr.Zero)
            return ptr;

        throw new Exception(SDL.SDL_GetError());
    }
}