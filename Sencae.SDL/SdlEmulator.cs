using System;
using System.Runtime.InteropServices;
using SDL2;
using static Sencae.SdlAssertions;

namespace Sencae {
    public unsafe class SdlEmulator {
        private static SDL.SDL_Keycode[] KeyMapping = {
            SDL.SDL_Keycode.SDLK_UP, // 0
            SDL.SDL_Keycode.SDLK_DOWN, // 1
            SDL.SDL_Keycode.SDLK_LEFT, // 2
            SDL.SDL_Keycode.SDLK_RIGHT, // 3
        };

        private readonly SDL.SDL_Surface* _surface;
        private readonly Machine _machine;

        public SdlEmulator(SDL.SDL_Surface* surface, Machine machine) {
            _surface = surface;
            _machine = machine;
        }

        public void Reset(ushort? overrideResetCustom = null) {
            _machine.Reset(overrideResetCustom);
        }

        public void HandleSdlEvent(in SDL.SDL_Event evt) {
            const int inputAddress = 0xFF;
            
            switch (evt.type) {

                case SDL.SDL_EventType.SDL_KEYDOWN:
                    for (int i = 0; i < KeyMapping.Length; i++) {
                        if (evt.key.keysym.sym == KeyMapping[i]) {
                            ref byte input = ref _machine.CpuBus.GetU8(inputAddress);
                            input = KeyMapping[i] switch {
                                SDL.SDL_Keycode.SDLK_UP => 0x77,
                                SDL.SDL_Keycode.SDLK_DOWN => 0x73,
                                SDL.SDL_Keycode.SDLK_LEFT => 0x64,
                                SDL.SDL_Keycode.SDLK_RIGHT => 0x61,
                            };
                        }
                    }

                    break;
                /*case SDL.SDL_EventType.SDL_KEYUP:
                    for (int i = 0; i < KeyMapping.Length; i++) {
                        if (evt.key.keysym.sym == KeyMapping[i]) {
                            machineState.Keys[i] = false;
                        }
                    }

                    break;*/
            }
        }

        public void Render() {
            if (SdlUtility.SDL_MUSTLOCK((IntPtr) _surface)) {
                AssertSdlSuccess(SDL.SDL_LockSurface((IntPtr) _surface));
            }

            int screenLength = NesConstants.ScreenWidth * NesConstants.ScreenHeight;
            Span<byte> emulatorScreenMemory = MemoryMarshal.CreateSpan(ref _machine.CpuBus.GetU8(0x0200), screenLength);
            byte* pixels = (byte*) _surface->pixels;

            for (int i = 0; i < screenLength; i++) {
                pixels[i] = emulatorScreenMemory[i];
            }

            if (SdlUtility.SDL_MUSTLOCK((IntPtr) _surface)) {
                SDL.SDL_UnlockSurface((IntPtr) _surface);
            }
        }

        public void Update(int cycles) {
            _machine.CpuBus.GetU8(0xFE) = (byte) Random.Shared.Next();

            for (int i = 0; i < cycles; i++) {
                if (_machine.State.Cpu.Halted)
                    break;

                Console.WriteLine(FormatCpuState(_machine.State));
                _machine.Clock();
            }
        }

        private static string FormatCpuState(MachineState machine) {
            CpuState cpu = machine.Cpu;
            return
                $"==== Cycle: {machine.Cycle}, " +
                $"AB: {cpu.Internal.AddressBus.Value:X4}, " +
                $"DB: {cpu.Internal.DataBus:X2}, " +
                $"PC: {cpu.PC.Value:X4}, " +
                $"RW: {(cpu.Internal.Read ? "R" : "W")}, " +
                $"OP: {cpu.Internal.Instruction.Mnemonic} ({cpu.Internal.Instruction.Opcode:X2}), " +
                $"A: {cpu.A:X2}, " +
                $"X: {cpu.X:X2}, " +
                $"Y: {cpu.Y:X2}, " +
                $"SP: {cpu.SP:X2}"
                ;
        }
    }
}
