using System;
using System.Runtime.CompilerServices;

namespace NGChip8Emulator {
    public unsafe class Emulator {
        private readonly Random _random = new Random();

        public virtual void Reset(ref MachineState machineState) {
            machineState = new MachineState();
            machineState.Cpu.PC = Chip8Data.ProgramStartAddress;

            fixed (byte* machineStateMemory = machineState.Memory) {
                byte* fontStartAddress = machineStateMemory + Chip8Data.HexDigitFontStartAddress;

                Chip8Data.HexDigitFontData.AsSpan().CopyTo(
                    new Span<byte>(
                        fontStartAddress,
                        Chip8Data.HexDigitFontData.Length
                    )
                );
            }
        }

        public void LoadProgram(ref MachineState machineState, byte[] program) {
            program.AsSpan().CopyTo(GetProgramMemorySpan(ref machineState));
        }

        public virtual void UpdateTimers(ref MachineState machineState) {
            if (machineState.Cpu.DT != 0) {
                machineState.Cpu.DT--;
            }

            if (machineState.Cpu.ST != 0) {
                machineState.Cpu.ST--;
                DoSound();
            }
        }

        public void Update(ref MachineState machineState, int instructionCount) {
            bool mustDrawAccumulative = false;
            for (int i = 0; i < instructionCount; i++) {
                EmulateCycle(ref machineState, out bool mustDraw);
                mustDrawAccumulative |= mustDraw;
                if (machineState.WaitingForAnyKey)
                    break;
            }

            machineState.ExecutedInstructionCount += (ulong) instructionCount;

            if (mustDrawAccumulative) {
                Render(ref machineState);
            }
        }

        protected virtual void EmulateCycle(ref MachineState machineState, out bool mustDraw) {
            // Fetch opcode
            int opcode = machineState.Memory[machineState.Cpu.PC] << 8 | machineState.Memory[machineState.Cpu.PC + 1];
            Instruction instruction = new Instruction();
            instruction.Decode(opcode);

            EmulateInstruction(ref machineState, in instruction, out mustDraw, out bool skipAdvanceProgramCounter);
            if (!skipAdvanceProgramCounter && !machineState.WaitingForAnyKey) {
                machineState.Cpu.PC += 2;
            }
        }

        protected virtual void EmulateInstruction(
            ref MachineState machineState,
            in Instruction instruction,
            out bool mustDraw,
            out bool skipAdvanceProgramCounter
        ) {
            mustDraw = false;
            skipAdvanceProgramCounter = false;
            ref CpuState cpu = ref machineState.Cpu;
            ref byte vX = ref cpu.V[instruction.x];
            ref byte vY = ref cpu.V[instruction.y];
            ref byte vF = ref cpu.V[0xF];

            switch (instruction.op) {
                case 0x00:
                    switch (instruction.kk) {
                        case 0x00:
                            break;
                        case 0xE0:
                            // 00E0 - CLS
                            // Clear the display.
                            ClearScreen(ref machineState);
                            mustDraw = true;

                            break;
                        case 0xEE:
                            // 00EE - RET
                            // Return from a subroutine.
                            //
                            // The interpreter sets the program counter to the address at the top of the stack,
                            // then subtracts 1 from the stack pointer.
                            cpu.PC = cpu.Stack[cpu.SP];
                            cpu.SP--;
                            //skipAdvanceProgramCounter = true;
                            break;
                        default:
                            Console.WriteLine($"Unknown kk {instruction.kk} for instruction ({instruction.ToString()})");
                            break;
                    }

                    break;
                case 0x01:
                    // 1nnn - JP addr
                    // Jump to location nnn.
                    //
                    // The interpreter sets the program counter to nnn.
                    cpu.PC = instruction.nnn;
                    skipAdvanceProgramCounter = true;
                    break;
                case 0x02:

                    // 2nnn - CALL addr
                    // Call subroutine at nnn.
                    //
                    // The interpreter increments the stack pointer, then puts the current PC on the top of the stack.
                    // The PC is then set to nnn.

                    cpu.SP++;
                    cpu.Stack[cpu.SP] = cpu.PC;
                    cpu.PC = instruction.nnn;
                    skipAdvanceProgramCounter = true;
                    break;
                case 0x03:

                    // 3xkk - SE Vx, byte
                    // Skip next instruction if Vx = kk.The interpreter compares register Vx to kk, and if they are equal,
                    // increments the program counter by 2.

                    if (vX == instruction.kk) {
                        cpu.PC += 2;
                    }

                    break;
                case 0x04:
                    // 4xkk - SNE Vx, byte
                    // Skip next instruction if Vx != kk.
                    // The interpreter compares register Vx to kk, and if they are not equal,
                    // increments the program counter by 2.
                    if (vX != instruction.kk) {
                        cpu.PC += 2;
                    }

                    break;
                case 0x05:
                    // 5xy0 - SE Vx, Vy
                    // Skip next instruction if Vx = Vy.
                    //
                    // The interpreter compares register Vx to register Vy, and if they are equal, increments the program counter by 2.
                    if (vX == vY) {
                        cpu.PC += 2;
                    }

                    break;
                case 0x06:
                    // 6xkk - LD Vx, byte
                    // Set Vx = kk.
                    //
                    // The interpreter puts the value kk into register Vx.
                    vX = instruction.kk;
                    break;
                case 0x07:
                    // 7xkk - ADD Vx, byte
                    // Set Vx = Vx + kk.
                    //
                    // Adds the value kk to the value of register Vx, then stores the result in Vx.
                    vX += instruction.kk;
                    break;
                case 0x08:
                    switch (instruction.n) {
                        case 0:
                            // 8xy0 - LD Vx, Vy
                            // Set Vx = Vy.
                            //
                            // Stores the value of register Vy in register Vx.
                            vX = vY;
                            break;
                        case 1:
                            // 8xy1 - OR Vx, Vy
                            // Set Vx = Vx OR Vy.
                            //
                            // Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx.
                            vX |= vY;
                            break;
                        case 2:
                            // 8xy2 - AND Vx, Vy
                            // Set Vx = Vx AND Vy.
                            //
                            // Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
                            vX &= vY;
                            break;
                        case 3:
                            // 8xy3 - XOR Vx, Vy
                            // Set Vx = Vx XOR Vy.
                            //
                            // Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx.
                            vX ^= vY;
                            break;
                        case 4:
                            // 8xy4 - ADD Vx, Vy
                            // Set Vx = Vx + Vy, set VF = carry.
                            //
                            // The values of Vx and Vy are added together. If the result is greater than 8 bits (i.e., > 255,) VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored in Vx.
                            int result = vX + vY;
                            vF = result > byte.MaxValue ? (byte) 1 : (byte) 0;
                            vX = unchecked((byte) result);
                            break;
                        case 5:
                            // 8xy5 - SUB Vx, Vy
                            // Set Vx = Vx - Vy, set VF = NOT borrow.
                            //
                            // If Vx > Vy, then VF is set to 1, otherwise 0. Then Vy is subtracted from Vx, and the results stored in Vx.
                            vF = vX > vY ? (byte) 1 : (byte) 0;
                            vX = unchecked((byte) (vX - vY));
                            break;
                        case 6:
                            // 8xy6 - SHR Vx {, Vy}
                            // Set Vx = Vx SHR 1.
                            //
                            // If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.
                            vF = (vX & 0b0000_0001) == 1 ? (byte) 1 : (byte) 0;
                            vX = (byte) (vX >> 1);
                            break;
                        case 7:
                            // 8xy7 - SUBN Vx, Vy
                            // Set Vx = Vy - Vx, set VF = NOT borrow.
                            //
                            // If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.
                            vF = vY > vX ? (byte) 1 : (byte) 0;
                            vX = unchecked((byte) (vY - vX));
                            break;
                        case 0xE:
                            // 8xyE - SHL Vx {, Vy}
                            // Set Vx = Vx SHL 1.
                            //
                            // If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
                            vF = (byte) (vX & 0b1000_0000 >> 7);
                            vX = (byte) (vX << 1);
                            break;
                        default:
                            Console.WriteLine($"Unknown n {instruction.n} for instruction ({instruction.ToString()})");
                            break;
                    }

                    break;
                case 0x09:
                    // 9xy0 - SNE Vx, Vy
                    // Skip next instruction if Vx != Vy.
                    //
                    // The values of Vx and Vy are compared, and if they are not equal, the program counter is increased by 2.
                    if (vX != vY) {
                        cpu.PC += 2;
                    }

                    break;
                case 0x0A:
                    // Annn - LD I, addr
                    // Set I = nnn.
                    //
                    // The value of register I is set to nnn.
                    cpu.I = instruction.nnn;
                    break;
                case 0x0B:
                    // Bnnn - JP V0, addr
                    // Jump to location nnn + V0.
                    //
                    // The program counter is set to nnn plus the value of V0.
                    cpu.PC = (ushort) (instruction.nnn + cpu.V[0]);
                    skipAdvanceProgramCounter = true;
                    break;
                case 0x0C:
                    // Cxkk - RND Vx, byte
                    // Set Vx = random byte AND kk.
                    //
                    // The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk.
                    // The results are stored in Vx. See instruction 8xy2 for more information on AND.
                    byte rnd = unchecked((byte) (_random.Next() & 0x000000FF & instruction.kk));
                    vX = rnd;
                    break;
                case 0x0D: {
                    // Dxyn - DRW Vx, Vy, nibble
                    // Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
                    //
                    // The interpreter reads n bytes from memory, starting at the address stored in I.
                    // These bytes are then displayed as sprites on screen at coordinates (Vx, Vy).
                    // Sprites are XORed onto the existing screen. If this causes any pixels to be erased,
                    // VF is set to 1, otherwise it is set to 0.
                    // If the sprite is positioned so part of it is outside the coordinates of the display,
                    // it wraps around to the opposite side of the screen.

                    mustDraw = true;

                    int spriteX = vX;
                    int spriteY = vY;
                    int spriteHeight = instruction.n;

                    //Console.WriteLine($"Draw sprite from I = {cpu.I} at ({spriteX}, {spriteY}) with height {instruction.n}");

                    bool isCollision = false;
                    for (int spriteRowIndex = 0; spriteRowIndex < spriteHeight; spriteRowIndex++) {
                        byte spriteRow = machineState.Memory[cpu.I + spriteRowIndex];
                        int spritePixelY = (spriteY + spriteRowIndex) % Chip8Data.ScreenHeight;
                        for (int bitIndex = 0; bitIndex < 8; bitIndex++) {
                            int spritePixelX = (spriteX + (7 - bitIndex)) % Chip8Data.ScreenWidth;
                            int screenPixelIndex = spritePixelY * Chip8Data.ScreenWidth + spritePixelX;

                            int spriteBit = (spriteRow >> bitIndex) & 1;

                            int screenPixelValue = machineState.Screen[screenPixelIndex];
                            int oldScreenPixelValue = screenPixelValue;
                            screenPixelValue ^= spriteBit;
                            machineState.Screen[screenPixelIndex] = (byte) screenPixelValue;

                            if (!isCollision) {
                                isCollision = screenPixelValue == 0 && oldScreenPixelValue == 1;
                            }
                        }
                    }

                    cpu.V[0xF] = isCollision ? (byte) 1 : (byte) 0;
                    break;
                }
                case 0x0E:
                    switch (instruction.kk) {
                        case 0x9E:
                            // Ex9E - SKP Vx
                            // Skip next instruction if key with the value of Vx is pressed.
                            //
                            // Checks the keyboard, and if the key corresponding to the value of Vx is currently in the down position, PC is increased by 2.
                            if (machineState.Keys[vX]) {
                                cpu.PC += 2;
                            }

                            break;
                        case 0xA1:

                            // ExA1 - SKNP Vx
                            // Skip next instruction if key with the value of Vx is not pressed.
                            //
                            // Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position, PC is increased by 2.
                            if (!machineState.Keys[vX]) {
                                cpu.PC += 2;
                            }

                            break;
                        default:
                            Console.WriteLine($"Unknown kk {instruction.kk} for instruction ({instruction.ToString()})");
                            break;
                    }

                    break;
                case 0x0F:
                    switch (instruction.kk) {
                        case 0x07:
                            // Fx07 - LD Vx, DT
                            // Set Vx = delay timer value.
                            //
                            // The value of DT is placed into Vx.
                            vX = cpu.DT;
                            break;
                        case 0x0A: {
                            // Fx0A - LD Vx, K
                            // Wait for a key press, store the value of the key in Vx.
                            //
                            // All execution stops until a key is pressed, then the value of that key is stored in Vx.
                            int key = -1;
                            for (int i = 0; i < 16; i++) {
                                if (machineState.Keys[i]) {
                                    key = i;
                                    break;
                                }
                            }

                            if (key == -1) {
                                machineState.WaitingForAnyKey = true;
                                break;
                            }

                            machineState.WaitingForAnyKey = false;
                            vX = (byte) key;
                            break;
                        }
                        case 0x15:
                            // Fx15 - LD DT, Vx
                            // Set delay timer = Vx.
                            //
                            // DT is set equal to the value of Vx.
                            cpu.DT = vX;
                            break;
                        case 0x18:
                            // Fx18 - LD ST, Vx
                            // Set sound timer = Vx.
                            //
                            // ST is set equal to the value of Vx.
                            cpu.ST = vX;
                            break;
                        case 0x1E:
                            // Fx1E - ADD I, Vx
                            // Set I = I + Vx.
                            //
                            // The values of I and Vx are added, and the results are stored in I.
                            cpu.I = (ushort) (cpu.I + vX);
                            break;
                        case 0x29:
                            // Fx29 - LD F, Vx
                            // Set I = location of sprite for digit Vx.
                            //
                            // The value of I is set to the location for the hexadecimal sprite corresponding to the value of Vx.
                            cpu.I = (ushort) (Chip8Data.HexDigitFontStartAddress + 5 * vX);
                            break;
                        case 0x33:
                            // Fx33 - LD B, Vx
                            // Store BCD representation of Vx in memory locations I, I+1, and I+2.
                            //
                            // The interpreter takes the decimal value of Vx,
                            // and places the hundreds digit in memory at location in I,
                            // the tens digit at location I+1,
                            // and the ones digit at location I+2.
                            machineState.Memory[cpu.I] = (byte) (vX / 100);
                            machineState.Memory[cpu.I + 1] = (byte) (vX / 10 % 10);
                            machineState.Memory[cpu.I + 2] = (byte) (vX % 10);
                            break;
                        case 0x55:

                            // Fx55 - LD[I], Vx
                            // Store registers V0 through Vx in memory starting at location I.
                            //
                            // The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.

                            for (int i = 0; i <= instruction.x; i++) {
                                machineState.Memory[cpu.I + i] = cpu.V[i];
                            }

                            break;
                        case 0x65:

                            // Fx65 - LD Vx, [I]
                            // Read registers V0 through Vx from memory starting at location I.
                            //
                            // The interpreter reads values from memory starting at location I into registers V0 through Vx.

                            for (int i = 0; i <= instruction.x; i++) {
                                cpu.V[i] = machineState.Memory[cpu.I + i];
                            }

                            break;
                        default:
                            Console.WriteLine($"Unknown kk {instruction.kk} for instruction ({instruction.ToString()})");
                            break;
                    }

                    break;
                default:
                    Console.WriteLine($"Unknown op {instruction.op} for instruction ({instruction.ToString()})");
                    break;
            }
        }

        private static void ClearScreen(ref MachineState machineState) {
            GetScreenMemorySpan(ref machineState).Fill(0);
        }

        protected virtual void Render(ref MachineState machineState) {
        }

        protected virtual void DoSound() {
        }

        protected static Span<byte> GetProgramMemorySpan(ref MachineState machineState) {
            fixed (byte* machineStateMemory = machineState.Memory) {
                byte* programStartAddress = machineStateMemory + Chip8Data.ProgramStartAddress;
                return new Span<byte>(
                    programStartAddress,
                    Chip8Data.MemorySize - Chip8Data.ProgramStartAddress
                );
            }
        }

        protected static Span<byte> GetMemorySpan(ref MachineState machineState) {
            fixed (byte* machineStateMemory = machineState.Memory) {
                return new Span<byte>(
                    machineStateMemory,
                    Chip8Data.MemorySize
                );
            }
        }

        protected static Span<byte> GetScreenMemorySpan(ref MachineState machineState) {
            fixed (byte* machineStateScreen = machineState.Screen) {
                return new Span<byte>(
                    machineStateScreen,
                    Chip8Data.ScreenWidth * Chip8Data.ScreenHeight
                );
            }
        }

        /*private ref struct InstructionExecutionResult {
            public bool MustDraw;
            public bool WaitingForAnyKey;
        }*/
    }
}
