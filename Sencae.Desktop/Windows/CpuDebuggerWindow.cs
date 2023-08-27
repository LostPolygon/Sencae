using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public class CpuDebuggerWindow : AppWindow {
    public Emulator Emulator { get; set; }

    public CpuDebuggerWindow(AppWindowContext context) : base(
        "CpuDebugger",
        "CPU Debugger",
        context
    ) {
    }

    protected override void DrawContent(float deltaTime) {
        ImGui.SetWindowSize(new Vector2(150, 300), ImGuiCond.FirstUseEver);

        if (Emulator == null!)
            return;
    
        bool isRunning = Emulator.IsRunning;
        ImGui.Checkbox("Running ", ref isRunning);
        Emulator.IsRunning = isRunning;
 
        ImGui.Separator();

        if (ImGui.Button("Reset")) {
            Emulator.Machine.Reset();
        }

        if (ImGui.Button("Step Cycle")) {
            Emulator.SingleCycleStep();
        }

        if (ImGui.Button("Step Instruction")) {
            Emulator.SingleInstructionStep();
        }

        ImGui.Separator();
        
        ref CpuState cpu = ref Emulator.Machine.State.Cpu;

        int DrawRegister(string label, string value, uint maxLength) {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text(label);

            ImGui.TableSetColumnIndex(1);

            int newValueInt = Convert.ToInt32(value, 16);
            string newValue = value;
            if (ImGui.InputText("##" + label, ref newValue, maxLength, ImGuiInputTextFlags.CharsHexadecimal)) {
                newValueInt = Convert.ToInt32(newValue, 16);
            }

            return newValueInt;
        }

        ImGui.BeginTable("Stats", 2);
        {
            ImGui.TableNextRow();
            
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("CPU Cycles: ");
            
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(Emulator.Machine.State.CpuProgramCycles.ToString());
            
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
            cpu.A = (byte) DrawRegister("A: ", $"{cpu.A:X2}", 2);
            cpu.X = (byte) DrawRegister("X: ", $"{cpu.X:X2}", 2);
            cpu.Y = (byte) DrawRegister("Y: ", $"{cpu.Y:X2}", 2);

            cpu.SP = (byte) DrawRegister("SP: ", $"{cpu.SP:X2}", 2);
            cpu.PC.Value = (ushort) DrawRegister("PC: ", $"{cpu.PC.Value:X4}", 4);
        }
        ImGui.EndTable();

        ImGui.Separator();
    }
}
