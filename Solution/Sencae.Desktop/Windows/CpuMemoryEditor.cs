using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public class CpuMemoryEditor(
    AppWindowContext context,
    Emulator emulator
) : HexEditorWindowBase(nameof(CpuMemoryEditor),
    "CPU Memory",
    context,
    UInt16.MaxValue) {
    public Emulator Emulator { get; } = emulator;

    protected override void DrawContent(float deltaTime) {
        ImGui.SetWindowSize(new Vector2(400, 500), ImGuiCond.FirstUseEver);
        
        if (!Emulator.IsLoaded)
            return;
        
        base.DrawContent(deltaTime);
    }

    protected override byte GetData(uint address) {
        byte data = 0;
        Emulator.Machine.CpuMemoryBus.HandleBus((ushort) address, MemoryAccessMode.ReadObserve, ref data);

        return data;
    }

    protected override void SetData(uint address, byte data) {
        Emulator.Machine.CpuMemoryBus.Write((ushort) address, data);
    }
}
