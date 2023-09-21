using System.Runtime.InteropServices;

namespace Sencae.Desktop.Sdl; 

public static class SdlNative {
    [DllImport("SDL2", CallingConvention = (CallingConvention) 2)]
    public static extern void SDL_free(IntPtr cursor);
}
