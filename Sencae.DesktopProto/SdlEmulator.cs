using System;
using System.Runtime.InteropServices;
using SDL2;
using static Sencae.SdlAssertions;

namespace Sencae; 

public unsafe class SdlEmulator {
    private static SDL.SDL_Keycode[] KeyMapping = {
        SDL.SDL_Keycode.SDLK_UP, // 0
        SDL.SDL_Keycode.SDLK_DOWN, // 1
        SDL.SDL_Keycode.SDLK_LEFT, // 2
        SDL.SDL_Keycode.SDLK_RIGHT, // 3
    };

    private readonly SDL.SDL_Surface* _surface;

    public SdlEmulator(SDL.SDL_Surface* surface, Machine machine) {
        _surface = surface;
    }

    public void Reset(ushort? overrideResetCustom = null) {
    }

    public void HandleSdlEvent(in SDL.SDL_Event evt) {
        switch (evt.type) {
        }
    }

    public void Render() {
        if (SdlUtility.SDL_MUSTLOCK((IntPtr) _surface)) {
            AssertSdlSuccess(SDL.SDL_LockSurface((IntPtr) _surface));
        }

        if (SdlUtility.SDL_MUSTLOCK((IntPtr) _surface)) {
            SDL.SDL_UnlockSurface((IntPtr) _surface);
        }
    }

    public void Update(int cycles) {

    }
}