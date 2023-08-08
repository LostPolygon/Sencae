using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using Cyotek.Collections.Generic;
using SDL2;
using static NGChip8Emulator.SdlAssertions;

namespace NGChip8Emulator {
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
        }

        /*public void Run() {
            // Create window
            AssertSdlSuccess(SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO));
            IntPtr window = AssertSdlSuccess(SDL.SDL_CreateWindow(
                "CHIP-8 Emulator",
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                ScreenScale * Chip8Data.ScreenWidth,
                ScreenScale * Chip8Data.ScreenHeight,
                0
            ));
            IntPtr renderer = AssertSdlSuccess(SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED));

            // Create the emulator render surface
            // It uses an 8-bit indexed pixel format, with all values except 0 rendered a solid white
            SDL.SDL_Surface* emulatorSurface =
                (SDL.SDL_Surface*) SDL.SDL_CreateRGBSurfaceWithFormat(
                    0,
                    Chip8Data.ScreenWidth,
                    Chip8Data.ScreenHeight,
                    8,
                    SDL.SDL_PIXELFORMAT_INDEX8
                );
            SDL.SDL_PixelFormat* emulatorSurfaceFormat = (SDL.SDL_PixelFormat*) emulatorSurface->format;

            SDL.SDL_Color[] paletteColors = new SDL.SDL_Color[256];
            for (int i = 0; i < 256; i++) {
                paletteColors[i] = new SDL.SDL_Color {
                    a = 255,
                    b = (byte) i,
                    g = (byte) i,
                    r = (byte) i
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
            _emulator = new SdlEmulator(emulatorSurface, _soundCircularBuffer);
            MachineState emulatorState = new MachineState();
            _emulator.Reset(ref emulatorState);
            _emulator.LoadProgram(ref emulatorState, File.ReadAllBytes(_commandLineOptions.RomFilePath));

            MainLoop(emulatorState, emulatorSurface, renderer);

            // Clean up.
            SDL.SDL_CloseAudioDevice(_sdlAudioDevice);
            SDL.SDL_FreeSurface((IntPtr) emulatorSurface);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }

        private void AudioCallback(IntPtr userdata, IntPtr stream, int len) {
            short* waveData = (short*) stream;
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
            }
        }

        private void MainLoop(MachineState emulatorState, SDL.SDL_Surface* emulatorSurface, IntPtr renderer) {
            bool quit = false;
            Stopwatch updateStopwatch = new Stopwatch();
            double frameDuration = 1.0 / Chip8Data.Framerate;

            while (!quit) {
                updateStopwatch.Restart();
                while (SDL.SDL_PollEvent(out SDL.SDL_Event evt) != 0) {
                    switch (evt.type) {
                        // Wait for user to close the window or press Esc
                        case SDL.SDL_EventType.SDL_QUIT:
                            quit = true;
                            break;

                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            if (evt.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) {
                                quit = true;
                                break;
                            }

                            break;
                    }

                    _emulator.HandleSdlEvent(ref emulatorState, in evt);
                }

                Update(_emulator, ref emulatorState, emulatorSurface, renderer);

                updateStopwatch.Stop();
                double updateElapsed = updateStopwatch.Elapsed.TotalSeconds;
                double delay = frameDuration - updateElapsed;
                if (delay > 0) {
                    SDL.SDL_Delay((uint) (delay * 1000));
                }
            }
        }

        private void Update(SdlEmulator emulator, ref MachineState emulatorState, SDL.SDL_Surface* emulatorSurface, IntPtr renderer) {
            emulator.UpdateTimers(ref emulatorState);
            emulator.Update(ref emulatorState, Chip8Data.InstructionPerFrame);

            // Set renderer drawing color.
            AssertSdlSuccess(SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255));

            // Clear the screen.
            SDL.SDL_RenderClear(renderer);

            // Set target rectangle.
            SDL.SDL_Rect tRect;
            tRect.x = 0;
            tRect.y = 0;
            tRect.w = Chip8Data.ScreenWidth * ScreenScale;
            tRect.h = Chip8Data.ScreenHeight * ScreenScale;

            // Blit the emulator view

            IntPtr emulatorTexture = SDL.SDL_CreateTextureFromSurface(renderer, (IntPtr) emulatorSurface);
            SDL.SDL_RenderCopy(renderer, emulatorTexture, IntPtr.Zero, ref tRect);
            SDL.SDL_DestroyTexture(emulatorTexture);

            // Present the "Painting" (backbuffer) to the screen. Call this once per frame.
            SDL.SDL_RenderPresent(renderer);
        }*/
    }
}
