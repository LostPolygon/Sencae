using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public class CpuDebuggerWindow(AppWindowContext context, Emulator emulator) :
    AppWindow(
        nameof(CpuDebuggerWindow),
        "CPU Debugger",
        context
    ) {
    public Emulator Emulator { get; } = emulator;

    private Vector2 TestVector2;

    private bool IsActive;

    protected override void DrawContent(float deltaTime) {
        IsActive = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

        ImGui.SetWindowSize(new Vector2(150, 300), ImGuiCond.FirstUseEver);

        //ImGui.PushFont(Context.SharedResources.FontFontAwesomeIconsSolid);
        /*ImGui.TextColored(new Vector4(1, 0, 0, 1), IconsFontAwesome5.FolderOpen);

        //ImGui.InputFloat2("Awesome vector:", ref TestVector2, "%f", ImGuiInputTextFlags.CharsScientific);
        //if (ImGuiProto2.InputFloat2("Awesome vector123456789112233445566:", ref TestVector2, "%f", ImGuiInputTextFlags.CharsScientific)) {
        if (ImGuiProto2.InputFloat2("omg!1", ref TestVector2, "%f", ImGuiInputTextFlags.CharsScientific)) {
            Console.WriteLine($"Changed: {TestVector2}");
        }
        */

        if (!Emulator.IsLoaded)
            return;

        ref CpuRegistersState cpuRegisters = ref Emulator.Machine.State.Cpu.Registers;
        ref CpuInternalState cpuInternal = ref Emulator.Machine.State.Cpu.Internal;

        //ImGui.PopFont();

        bool isRunning = Emulator.IsRunning;
        ImGui.Checkbox("Running ", ref isRunning);
        Emulator.IsRunning = isRunning;

        ImGui.Separator();

        if (ImGui.Button("Reset")) {
            Emulator.Reset();
        }

        if (ImGui.Button("Soft Reset")) {
            Emulator.Reset(hardReset: false);
        }

        if (ImGui.Button("Step Cycle")) {
            Emulator.SingleCycleStep();
        }

        if (ImGui.Button("Step Instruction")) {
            Emulator.SingleInstructionStep();
        }

        if (ImGui.Button("Run To Interrupt")) {
            Emulator.RunToInterrupt();
        }

        if (ImGui.Button("Run To VBlank")) {
            do {
                Emulator.Machine.Clock();
            } while (Emulator.Machine.State.Ppu.Internal.FrameCycle != 82182);
        }

        double timeScale = Emulator.TimeScale;
        if (ImGuiEx.SliderScalar("Time Scale", ref timeScale, 0.0001f, 250f, null!, ImGuiSliderFlags.Logarithmic)) {
            Emulator.TimeScale = timeScale;
        }

        ImGui.Separator();

        ImGui.TextUnformatted($"PPU position: {Emulator.Machine.State.Ppu.Internal.CyclesInScanline,3}, {Emulator.Machine.State.Ppu.Internal.Scanline,3}");
        ImGui.TextUnformatted($"Frames: {Emulator.Machine.State.Ppu.Internal.Frames}");

        ImGui.Separator();

        void DrawRegister<T>(string label, ref T value, string format, T minValue, T maxValue) where T : unmanaged, IComparable<T> {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text(label);

            ImGui.TableSetColumnIndex(1);

            T newValue = value;
            if (ImGuiEx.InputScalar("##" + label, ref newValue, null, null, format, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue)) {
                lock (Emulator.EmulationThreadState.Lock) {
                    if (newValue.CompareTo(minValue) < 0) {
                        newValue = minValue;
                    } else if (value.CompareTo(maxValue) > 0) {
                        newValue = maxValue;
                    }

                    value = newValue;
                }
            }
        }

        ImGui.BeginTable("Stats", 2);
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("CPU Cycles: ");

            ImGui.TableSetColumnIndex(1);
            ImGui.Text(cpuInternal.Cycles.ToString());

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Machine Cycles: ");

            ImGui.TableSetColumnIndex(1);
            ImGui.Text(Emulator.Machine.State.MachineCycles.ToString());
        }
        ImGui.EndTable();

        ImGui.Separator();

        ImGui.BeginTable("Registers", 2);
        {
            DrawRegister("A: ", ref cpuRegisters.A, "%02X", Byte.MinValue, Byte.MaxValue);
            DrawRegister("X: ", ref cpuRegisters.X, "%02X", Byte.MinValue, Byte.MaxValue);
            DrawRegister("Y: ", ref cpuRegisters.Y, "%02X", Byte.MinValue, Byte.MaxValue);
            DrawRegister("SP: ", ref cpuRegisters.SP, "%02X", Byte.MinValue, Byte.MaxValue);
            DrawRegister("PC: ", ref cpuRegisters.PC.Value, "%04X", UInt16.MinValue, UInt16.MaxValue);

            DrawRegister("AB: ", ref cpuInternal.AddressBus.Value, "%04X", UInt16.MinValue, UInt16.MaxValue);
            DrawRegister("DB: ", ref cpuInternal.DataBus, "%02X", Byte.MinValue, Byte.MaxValue);

            ImGui.TextUnformatted(Instructions.DescriptorByOpcode[cpuInternal.Instruction.OpCode].Mnemonic);
        }
        ImGui.EndTable();

        ImGui.Separator();

        ImGui.TextUnformatted(_controller1Input.ToString());
    }

    private StandardControllerInput _controller1Input;

    public bool HandleSdlEvent(in SDL_Event evt) {
        if (!Emulator.IsLoaded || !IsActive)
            return false;

        /*
        A = 1 << 0,
        B = 1 << 1,
        Select = 1 << 2,
        Start = 1 << 3,
        Up = 1 << 4,
        Down = 1 << 5,
        Left = 1 << 6,
        Right = 1 << 7,
        */
        SDL_Keycode[] keyMapping = [
            SDL_Keycode.SDLK_z,
            SDL_Keycode.SDLK_x,

            SDL_Keycode.SDLK_a,
            SDL_Keycode.SDLK_s,

            SDL_Keycode.SDLK_UP,
            SDL_Keycode.SDLK_DOWN,
            SDL_Keycode.SDLK_LEFT,
            SDL_Keycode.SDLK_RIGHT
        ];

        bool handled = false;
        switch (evt.type) {
            case SDL_EventType.SDL_KEYDOWN:
                for (int i = 0; i < keyMapping.Length; i++) {
                    if (evt.key.keysym.sym == keyMapping[i]) {
                        _controller1Input |= (StandardControllerInput) (1 << i);
                        handled = true;
                    }
                }

                break;
            case SDL_EventType.SDL_KEYUP:
                for (int i = 0; i < keyMapping.Length; i++) {
                    if (evt.key.keysym.sym == keyMapping[i]) {
                        _controller1Input &= (StandardControllerInput) ~(1 << i);
                        handled = true;
                    }
                }

                break;
        }

        Emulator.Machine.StandardControllersDevice.SetController1Input(_controller1Input);

        return handled;
    }
}
