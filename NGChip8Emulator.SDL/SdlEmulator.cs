using System;
using Cyotek.Collections.Generic;
using SDL2;
using static NGChip8Emulator.SdlAssertions;

namespace NGChip8Emulator {
    public unsafe class SdlEmulator  {
        private const int BeepFrequency = 300;
        private const int BeepAmplitude = 20000;
        
        private static SDL.SDL_Keycode[] KeyMapping = {
            SDL.SDL_Keycode.SDLK_x, // 0
            SDL.SDL_Keycode.SDLK_1, // 1
            SDL.SDL_Keycode.SDLK_2, // 2
            SDL.SDL_Keycode.SDLK_3, // 3

            SDL.SDL_Keycode.SDLK_q, // 4
            SDL.SDL_Keycode.SDLK_w, // 5
            SDL.SDL_Keycode.SDLK_e, // 6
            SDL.SDL_Keycode.SDLK_a, // 7

            SDL.SDL_Keycode.SDLK_s, // 8
            SDL.SDL_Keycode.SDLK_d, // 9
            SDL.SDL_Keycode.SDLK_z, // A
            SDL.SDL_Keycode.SDLK_c, // B

            SDL.SDL_Keycode.SDLK_4, // C
            SDL.SDL_Keycode.SDLK_r, // D
            SDL.SDL_Keycode.SDLK_f, // E
            SDL.SDL_Keycode.SDLK_v, // F
        };

        /*private readonly SDL.SDL_Surface* _surface;
        private readonly CircularBuffer<short> _soundCircularBuffer;
        private ulong _totalSoundSamples;

        public SdlEmulator(SDL.SDL_Surface* surface, CircularBuffer<short> soundCircularBuffer) {
            _surface = surface;
            _soundCircularBuffer = soundCircularBuffer;
        }

        public void HandleSdlEvent(ref MachineState machineState, in SDL.SDL_Event evt) {
            switch (evt.type) {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    for (int i = 0; i < KeyMapping.Length; i++) {
                        if (evt.key.keysym.sym == KeyMapping[i]) {
                            machineState.Keys[i] = true;
                        }
                    }

                    break;
                case SDL.SDL_EventType.SDL_KEYUP:
                    for (int i = 0; i < KeyMapping.Length; i++) {
                        if (evt.key.keysym.sym == KeyMapping[i]) {
                            machineState.Keys[i] = false;
                        }
                    }

                    break;
            }
        }

        public override void UpdateTimers(ref MachineState machineState) {
            base.UpdateTimers(ref machineState);

            if (SdlUtility.SDL_MUSTLOCK((IntPtr) _surface)) {
                AssertSdlSuccess(SDL.SDL_LockSurface((IntPtr) _surface));
            }

            Span<byte> emulatorScreenMemory = GetScreenMemorySpan(ref machineState);
            byte* pixels = (byte*) _surface->pixels;
            int screenLength = Chip8Data.ScreenWidth * Chip8Data.ScreenHeight;
            for (int i = 0; i < screenLength; i++) {
                int newPixel;
                if (emulatorScreenMemory[i] == 1) {
                    newPixel = 255;
                }
                else {
                    newPixel = pixels[i];
                    newPixel -= 32;
                    if (newPixel < 0) {
                        newPixel = 0;
                    }
                }

                pixels[i] = (byte) newPixel;
            }

            if (SdlUtility.SDL_MUSTLOCK((IntPtr) _surface)) {
                SDL.SDL_UnlockSurface((IntPtr) _surface);
            }
        }

        protected override void Render(ref MachineState machineState) {
            if (SdlUtility.SDL_MUSTLOCK((IntPtr) _surface)) {
                AssertSdlSuccess(SDL.SDL_LockSurface((IntPtr) _surface));
            }

            Span<byte> emulatorScreenMemory = GetScreenMemorySpan(ref machineState);
            byte* pixels = (byte*) _surface->pixels;
            int screenLength = Chip8Data.ScreenWidth * Chip8Data.ScreenHeight;

            for (int i = 0; i < screenLength; i++) {
                if (emulatorScreenMemory[i] == 1) {
                    pixels[i] = 255;
                }
            }

            if (SdlUtility.SDL_MUSTLOCK((IntPtr) _surface)) {
                SDL.SDL_UnlockSurface((IntPtr) _surface);
            }
        }

        protected override void DoSound() {
            double soundLength = 1.0 / Chip8Data.Framerate * 1.4;
            int soundSampleCount = (int) (soundLength * 44100.0);

            lock (_soundCircularBuffer) {
                for (int i = 0; i < soundSampleCount; i++) {
                    short sample = (short) (BeepAmplitude * FastSquareWave((float) (_totalSoundSamples * BeepFrequency * 2.0 * Math.PI / 44100.0)));
                    _soundCircularBuffer.Put(sample);
                    _totalSoundSamples++;
                }
            }
        }

        private static float FastSquareWave(float x) {
            return x % (float) Math.PI < (float) Math.PI * 0.5 ? -1 : 1;
        }

        private static float FastSin(float x) {
            float floorVal = (x + (float) Math.PI) / ((float) Math.PI * 2f);
            int floor = floorVal >= 0 ? (int) floorVal : (int) (floorVal - 1);
            x = x - (float) Math.PI * 2f * floor;

            x = x < 0 ?
                1.27323954f * x + 0.405284735f * x * x :
                1.27323954f * x - 0.405284735f * x * x;

            return x;
        }*/

    }
}
