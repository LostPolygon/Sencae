using System.Numerics;
using ImGuiNET;

namespace Sencae.Desktop;

public class MainAppSdlWindow : AppSdlWindow {
    protected SharedResources SharedResources { get; set; }
    protected Emulator Emulator { get; set; }
    protected CpuDebuggerWindow CpuDebuggerWindow { get; set; }
    protected PpuTileViewerWindow PpuTileViewerWindow { get; set; }
    protected ProfilerWindow ProfilerWindow { get; set; }

    public MainAppSdlWindow(
        string title,
        int x,
        int y,
        int w,
        int h,
        SDL_WindowFlags windowFlags,
        SDL_RendererFlags rendererFlags,
        string[] args
    ) : base(title, x, y, w, h, windowFlags, rendererFlags) {
        Emulator = new Emulator(args[0], NesConstants.Ntsc);

        SharedResources = new SharedResources(Renderer) {
            FontMonospacedNormal = ImGuiGlobals.FontMonospacedNormal
        };
        AppWindowContext appWindowContext = new AppWindowContext(Renderer, SharedResources);

        CpuDebuggerWindow = new CpuDebuggerWindow(appWindowContext) {
            Emulator = Emulator
        };

        PpuTileViewerWindow = new PpuTileViewerWindow(appWindowContext) {
            Emulator = Emulator
        };

        ProfilerWindow = new ProfilerWindow(appWindowContext);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        Emulator.Dispose();
        SharedResources.Dispose();

        PpuTileViewerWindow.Dispose();
    }

    protected override void RenderContent() {
        Vector4 clearColor = new(0.45f, 0.55f, 0.60f, 1.00f);
        SDL_SetRenderDrawColor(Renderer, (byte) (clearColor.X * 255), (byte) (clearColor.Y * 255), (byte) (clearColor.Z * 255), (byte) (clearColor.W * 255));
        SDL_RenderClear(Renderer);

        DrawContentInternal();
    }

    private void DrawContentInternal() {
        DrawMainMenu();

        ImGui.ShowDemoWindow();

        float deltaTime = (float) DeltaTime;

        Emulator.Update(DeltaTime);
        CpuDebuggerWindow.Draw(deltaTime);
        PpuTileViewerWindow.Draw(deltaTime);
        ProfilerWindow.Draw(deltaTime);
    }

    private void DrawMainMenu() {
        if (ImGui.BeginMainMenuBar()) {
            if (ImGui.BeginMenu("File")) {
                if (ImGui.MenuItem("Open...")) {
                }

                ImGui.Separator();
                if (ImGui.MenuItem("Exit")) {
                    IsQuitting = true;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Windows")) {
                static void DrawWindowItem(string name, Func<bool> getter, Action<bool> setter) {
                    bool selected = getter();
                    if (ImGui.MenuItem(name, null, ref selected)) {
                        setter(selected);
                    }
                }

                DrawWindowItem("CPU Debugger", () => CpuDebuggerWindow.IsOpen, v => CpuDebuggerWindow.IsOpen = v);
                DrawWindowItem("CHR Viewer", () => PpuTileViewerWindow.IsOpen, v => PpuTileViewerWindow.IsOpen = v);
                DrawWindowItem("Profiler", () => ProfilerWindow.IsOpen, v => ProfilerWindow.IsOpen = v);

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
