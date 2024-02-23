using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public class CpuDebuggerWindow : AppWindow {
    public Emulator Emulator { get; }

    private Vector2 TestVector2;

    public CpuDebuggerWindow(AppWindowContext context, Emulator emulator) : base(
        nameof(CpuDebuggerWindow),
        "CPU Debugger",
        context
    ) {
        Emulator = emulator;
    }

    protected override void DrawContent(float deltaTime) {
        ImGui.SetWindowSize(new Vector2(150, 300), ImGuiCond.FirstUseEver);



        //ImGui.PushFont(Context.SharedResources.FontFontAwesomeIconsSolid);
        ImGui.TextColored(new Vector4(1, 0, 0, 1), IconsFontAwesome5.FolderOpen);

        //ImGui.InputFloat2("Awesome vector:", ref TestVector2, "%f", ImGuiInputTextFlags.CharsScientific);
        //if (ImGuiProto2.InputFloat2("Awesome vector123456789112233445566:", ref TestVector2, "%f", ImGuiInputTextFlags.CharsScientific)) {
        if (ImGuiProto2.InputFloat2("omg!1", ref TestVector2, "%f", ImGuiInputTextFlags.CharsScientific)) {
            Console.WriteLine($"Changed: {TestVector2}");
        }
        
        
        
        if (!Emulator.IsLoaded)
            return;
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

        double timeScale = Emulator.TimeScale;
        if (ImGuiEx.SliderScalar("Time Scale", ref timeScale, 0.0001f, 250f, null!, ImGuiSliderFlags.Logarithmic)) {
            Emulator.TimeScale = timeScale;
        }

        ImGui.Separator();

        CpuState cpu = Emulator.Machine.State.Cpu;

        static int DrawRegister(string label, int value, string format, int maxValue) {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text(label);

            ImGui.TableSetColumnIndex(1);

            if (ImGuiEx.InputScalar("##" + label, ref value, null, null, format, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue)) {
                value = Math.Clamp(value, 0, maxValue);
            }

            return value;
        }

        ImGui.BeginTable("Stats", 2);
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("CPU Cycles: ");

            ImGui.TableSetColumnIndex(1);
            ImGui.Text(Emulator.Machine.State.Cpu.Internal.Cycles.ToString());

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
            cpu.A = (byte) DrawRegister("A: ", cpu.A, "%02X", Byte.MaxValue);
            cpu.X = (byte) DrawRegister("X: ", cpu.X, "%02X", Byte.MaxValue);
            cpu.Y = (byte) DrawRegister("Y: ", cpu.Y, "%02X", Byte.MaxValue);
            cpu.SP = (byte) DrawRegister("SP: ", cpu.SP, "%02X", Byte.MaxValue);
            cpu.PC.Value = (ushort) DrawRegister("PC: ", cpu.PC.Value, "%04X", UInt16.MaxValue);
            
            cpu.Internal.AddressBus.Value = (ushort) DrawRegister("AB: ", cpu.Internal.AddressBus.Value, "%04X", UInt16.MaxValue);
            cpu.Internal.DataBus = (byte) DrawRegister("DB: ", cpu.Internal.DataBus, "%02X", UInt16.MaxValue);
            
            ImGui.TextUnformatted(cpu.Internal.Instruction.Mnemonic);
        }
        ImGui.EndTable();

        ImGui.Separator();
    }
}
