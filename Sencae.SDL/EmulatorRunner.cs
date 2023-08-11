using System;
using System.Diagnostics;
using System.Drawing;
using Cyotek.Collections.Generic;
using SDL2;
using static Sencae.SdlAssertions;

namespace Sencae; 

public unsafe class EmulatorRunner {
    private const int ScreenScale = 15;

    private readonly CommandLineOptions _commandLineOptions;
    private uint _sdlAudioDevice;
    private SdlEmulator _emulator;
    private CircularBuffer<short> _soundCircularBuffer = new CircularBuffer<short>(8192);

    public EmulatorRunner(CommandLineOptions commandLineOptions) {
        _commandLineOptions = commandLineOptions;
    }

    public void Run() {
        // Create window
        AssertSdlSuccess(SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO));
        IntPtr window = AssertSdlSuccess(SDL.SDL_CreateWindow(
            "Sencae NES Emulator",
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            ScreenScale * NesConstants.ScreenWidth,
            ScreenScale * NesConstants.ScreenHeight,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
        ));
        IntPtr renderer = AssertSdlSuccess(SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));

        // Create the emulator render surface
        // It uses an 8-bit indexed pixel format, with all values except 0 rendered a solid white
        SDL.SDL_Surface* emulatorSurface =
            (SDL.SDL_Surface*) SDL.SDL_CreateRGBSurfaceWithFormat(
                0,
                NesConstants.ScreenWidth,
                NesConstants.ScreenHeight,
                8,
                SDL.SDL_PIXELFORMAT_INDEX8
            );
        SDL.SDL_PixelFormat* emulatorSurfaceFormat = (SDL.SDL_PixelFormat*) emulatorSurface->format;

        SDL.SDL_Color[] paletteColors = new SDL.SDL_Color[256];
        for (int i = 0; i < 256; i++) {
            Color color = i switch {
                0 => Color.Black,
                1 => Color.White,
                2 or 9 => Color.Gray,
                3 or 10 => Color.Red,
                4 or 11 => Color.Green,
                5 or 12 => Color.Blue,
                6 or 13 => Color.Magenta,
                7 or 14 => Color.Yellow,
                _ => Color.Cyan
            };
                
            paletteColors[i] = new SDL.SDL_Color {
                a = color.A,
                b = color.B,
                g = color.G,
                r = color.R
            };
        }

        AssertSdlSuccess(SDL.SDL_SetPaletteColors(emulatorSurfaceFormat->palette, paletteColors, 0, paletteColors.Length));

        // Setup sound
        SDL.SDL_AudioSpec desiredSpec = new SDL.SDL_AudioSpec {
            freq = 44100,
            format = SDL.AUDIO_S16SYS,
            channels = 1,
            samples = 2048,
            callback = AudioCallback,
            userdata = IntPtr.Zero
        };

        // Don't allow any format changes
        _sdlAudioDevice = AssertSdlSuccessNonZero(SDL.SDL_OpenAudioDevice(null, 0, ref desiredSpec, out SDL.SDL_AudioSpec _, 0));
        SDL.SDL_PauseAudioDevice(_sdlAudioDevice, 0);

        // Initialize the emulator
        Machine machine = new(new CartridgeDevice(new byte[16 * 1024], new byte[8 * 1024]));

        byte[] gameRom = {
            0x20, 0x06, 0x06, 0x20, 0x38, 0x06, 0x20, 0x0d, 0x06, 0x20, 0x2a, 0x06, 0x60, 0xa9, 0x02, 0x85,
            0x02, 0xa9, 0x04, 0x85, 0x03, 0xa9, 0x11, 0x85, 0x10, 0xa9, 0x10, 0x85, 0x12, 0xa9, 0x0f, 0x85,
            0x14, 0xa9, 0x04, 0x85, 0x11, 0x85, 0x13, 0x85, 0x15, 0x60, 0xa5, 0xfe, 0x85, 0x00, 0xa5, 0xfe,
            0x29, 0x03, 0x18, 0x69, 0x02, 0x85, 0x01, 0x60, 0x20, 0x4d, 0x06, 0x20, 0x8d, 0x06, 0x20, 0xc3,
            0x06, 0x20, 0x19, 0x07, 0x20, 0x20, 0x07, 0x20, 0x2d, 0x07, 0x4c, 0x38, 0x06, 0xa5, 0xff, 0xc9,
            0x77, 0xf0, 0x0d, 0xc9, 0x64, 0xf0, 0x14, 0xc9, 0x73, 0xf0, 0x1b, 0xc9, 0x61, 0xf0, 0x22, 0x60,
            0xa9, 0x04, 0x24, 0x02, 0xd0, 0x26, 0xa9, 0x01, 0x85, 0x02, 0x60, 0xa9, 0x08, 0x24, 0x02, 0xd0,
            0x1b, 0xa9, 0x02, 0x85, 0x02, 0x60, 0xa9, 0x01, 0x24, 0x02, 0xd0, 0x10, 0xa9, 0x04, 0x85, 0x02,
            0x60, 0xa9, 0x02, 0x24, 0x02, 0xd0, 0x05, 0xa9, 0x08, 0x85, 0x02, 0x60, 0x60, 0x20, 0x94, 0x06,
            0x20, 0xa8, 0x06, 0x60, 0xa5, 0x00, 0xc5, 0x10, 0xd0, 0x0d, 0xa5, 0x01, 0xc5, 0x11, 0xd0, 0x07,
            0xe6, 0x03, 0xe6, 0x03, 0x20, 0x2a, 0x06, 0x60, 0xa2, 0x02, 0xb5, 0x10, 0xc5, 0x10, 0xd0, 0x06,
            0xb5, 0x11, 0xc5, 0x11, 0xf0, 0x09, 0xe8, 0xe8, 0xe4, 0x03, 0xf0, 0x06, 0x4c, 0xaa, 0x06, 0x4c,
            0x35, 0x07, 0x60, 0xa6, 0x03, 0xca, 0x8a, 0xb5, 0x10, 0x95, 0x12, 0xca, 0x10, 0xf9, 0xa5, 0x02,
            0x4a, 0xb0, 0x09, 0x4a, 0xb0, 0x19, 0x4a, 0xb0, 0x1f, 0x4a, 0xb0, 0x2f, 0xa5, 0x10, 0x38, 0xe9,
            0x20, 0x85, 0x10, 0x90, 0x01, 0x60, 0xc6, 0x11, 0xa9, 0x01, 0xc5, 0x11, 0xf0, 0x28, 0x60, 0xe6,
            0x10, 0xa9, 0x1f, 0x24, 0x10, 0xf0, 0x1f, 0x60, 0xa5, 0x10, 0x18, 0x69, 0x20, 0x85, 0x10, 0xb0,
            0x01, 0x60, 0xe6, 0x11, 0xa9, 0x06, 0xc5, 0x11, 0xf0, 0x0c, 0x60, 0xc6, 0x10, 0xa5, 0x10, 0x29,
            0x1f, 0xc9, 0x1f, 0xf0, 0x01, 0x60, 0x4c, 0x35, 0x07, 0xa0, 0x00, 0xa5, 0xfe, 0x91, 0x00, 0x60,
            0xa6, 0x03, 0xa9, 0x00, 0x81, 0x10, 0xa2, 0x00, 0xa9, 0x01, 0x81, 0x10, 0x60, 0xa2, 0x00, 0xea,
            0xea, 0xca, 0xd0, 0xfb, 0x60
        };

        for (int i = 0; i < gameRom.Length; i++) {
            machine.CpuMemoryBus.GetU8((ushort) (0x0600 + i)) = gameRom[i];
        }
            
        _emulator = new SdlEmulator(emulatorSurface, machine);
        _emulator.Reset(0x0600);
            
        MainLoop(emulatorSurface, renderer, window);

        // Clean up.
        SDL.SDL_CloseAudioDevice(_sdlAudioDevice);
        SDL.SDL_FreeSurface((IntPtr) emulatorSurface);
        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }

    private static void AudioCallback(IntPtr userdata, IntPtr stream, int len) {
        /*short* waveData = (short*) stream;
        int sampleCount = len / sizeof(short);
        int readSamples;

        lock (_soundCircularBuffer) {
            short[] buffer = ArrayPool<short>.Shared.Rent(sampleCount);
            readSamples = _soundCircularBuffer.Get(buffer, 0, sampleCount);

            for (int i = 0; i < readSamples; i++) {
                waveData[i] = buffer[i];
            }
        }

        // Fill with silence
        for (int i = readSamples; i < len; i++) {
            waveData[i] = 0;
        }*/
    }

    private void MainLoop(SDL.SDL_Surface* emulatorSurface, IntPtr renderer, IntPtr window) {
        bool quit = false;
        Stopwatch updateStopwatch = new Stopwatch();
        double frameDuration = 1.0 / NesConstants.Framerate;
            
        while (!quit) {
            updateStopwatch.Restart();
            while (SDL.SDL_PollEvent(out SDL.SDL_Event evt) == 1) {
                switch (evt.type) {
                    // Wait for user to close the window or press Esc
                    case SDL.SDL_EventType.SDL_QUIT:
                        quit = true;
                        break;

                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        if (evt.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) {
                            quit = true;
                        }

                        break;
                }

                _emulator.HandleSdlEvent(in evt);
            }

            Update(_emulator, emulatorSurface, renderer);
                
            updateStopwatch.Stop();
            double updateElapsed = updateStopwatch.Elapsed.TotalSeconds;
            double delay = frameDuration - updateElapsed;
            if (delay > 0) {
                SDL.SDL_Delay((uint) (delay * 1000));
            }
                
            SDL.SDL_SetWindowTitle(window, $"Frame: {frames}");
            frames++;
        }
    }

    private ulong frames;

    private void Update(SdlEmulator emulator, SDL.SDL_Surface* emulatorSurface, IntPtr renderer) {
        emulator.Update(NesConstants.CyclesPerFrame);
        emulator.Render();

        // Set renderer drawing color.
        AssertSdlSuccess(SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255));

        // Clear the screen.
        SDL.SDL_RenderClear(renderer);

        // Set target rectangle.
        SDL.SDL_Rect tRect;
        tRect.x = 0;
        tRect.y = 0;
        tRect.w = NesConstants.ScreenWidth * ScreenScale;
        tRect.h = NesConstants.ScreenHeight * ScreenScale;

        // Blit the emulator view
        IntPtr emulatorTexture = SDL.SDL_CreateTextureFromSurface(renderer, (IntPtr) emulatorSurface);
        SDL.SDL_RenderCopy(renderer, emulatorTexture, IntPtr.Zero, ref tRect);
        SDL.SDL_DestroyTexture(emulatorTexture);

        // Present the "Painting" (backbuffer) to the screen. Call this once per frame.
        SDL.SDL_RenderPresent(renderer);
    }
}