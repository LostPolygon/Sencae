using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using NativeFileDialogSharp;
using Perfolizer.Horology;
using Sencae.Desktop.Sdl;
using Sencae.Ppu;

namespace Sencae.Desktop;

public unsafe class MainAppSdlWindow : AppSdlWindow {
    protected UIPersistentSettings UIPersistentSettings { get; }
    protected SharedResources SharedResources { get; set; }
    protected Emulator Emulator { get; set; }
    protected CpuDebuggerWindow CpuDebuggerWindow { get; set; }
    protected PpuTileViewerWindow PpuTileViewerWindow { get; set; }
    protected ProfilerWindow ProfilerWindow { get; set; }
    protected CpuMemoryEditor CpuMemoryEditor { get; set; }
    protected MachineLogWindow MachineLogWindow { get; set; }

    private string? _loadingRomPath;
    private NotificationArea _notificationArea;

    public MainAppSdlWindow(
        SdlMainLoop mainLoop,
        string title,
        int x,
        int y,
        int w,
        int h,
        SDL_WindowFlags windowFlags,
        SDL_RendererFlags rendererFlags,
        string[] args,
        UIPersistentSettings uiPersistentSettings
    ) : base(mainLoop, title, x, y, w, h, windowFlags, rendererFlags) {
        Emulator = new Emulator(NesConstants.Ntsc);
        Emulator.IsRunning = true;

        UIPersistentSettings = uiPersistentSettings;
        _loadingRomPath = args.Length != 0 ? args[0] : null;

        Span<byte> paletteBytes = File.ReadAllBytes("Resources/palette.pal").AsSpan();
        PpuPalette ppuPalette = default;

        for (int i = 0; i < PpuPalette.ColorCount; i++) {
            Color color = Color.FromArgb(
                255,
                paletteBytes[i * 3 + 2],
                paletteBytes[i * 3 + 1],
                paletteBytes[i * 3 + 0]
            );

            ppuPalette.Colors[i] = (uint) color.ToArgb();
        }

        SharedResources = new SharedResources(Renderer) {
            FontMonospacedNormal = ImGuiGlobals.FontMonospacedNormal,
            FontFontAwesomeIconsSolid = ImGuiGlobals.FontFontAwesomeIconsSolid,
            PpuPalette = ppuPalette
        };

        _notificationArea = new NotificationArea(SharedResources);

        AppWindowContext appWindowContext = new(Renderer, SharedResources, _notificationArea);

        CpuDebuggerWindow = new CpuDebuggerWindow(appWindowContext, Emulator);

        PpuTileViewerWindow = new PpuTileViewerWindow(appWindowContext, Emulator);

        ProfilerWindow = new ProfilerWindow(appWindowContext);
        CpuMemoryEditor = new CpuMemoryEditor(appWindowContext, Emulator);
        MachineLogWindow = new MachineLogWindow(appWindowContext, Emulator);
    }

    public override void MainLoopEvent(MainLoopEvent mainLoopEvent) {
        base.MainLoopEvent(mainLoopEvent);

        switch (mainLoopEvent) {
            case Sdl.MainLoopEvent.Starting:
                SDL_EventState(SDL_EventType.SDL_DROPFILE, SDL_ENABLE);
                break;
            case Sdl.MainLoopEvent.Iteration:
                break;
            case Sdl.MainLoopEvent.Ending:
                SDL_EventState(SDL_EventType.SDL_DROPFILE, SDL_DISABLE);

                SDL_GetWindowSize(Window, out int windowSizeX, out int windowSizeY);
                UIPersistentSettings.MainWindowSizeX = windowSizeX;
                UIPersistentSettings.MainWindowSizeY = windowSizeY;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mainLoopEvent), mainLoopEvent, null);
        }
    }

    public override unsafe void HandleWindowSdlEvent(in SDL_Event evt) {
        switch (evt.type) {
            // Wait for user to close the window or press Esc
            case SDL_EventType.SDL_QUIT:
                MainLoop.Quit();
                break;

            case SDL_EventType.SDL_KEYDOWN:
                if (evt.key.keysym.sym == SDL_Keycode.SDLK_ESCAPE) {
                    MainLoop.Quit();
                }

                break;

            case SDL_EventType.SDL_DROPFILE:
                IntPtr droppedFile = evt.drop.file;
                string droppedFileStr = Marshal.PtrToStringUTF8(droppedFile)!;
                SdlNative.SDL_free(droppedFile);
                _loadingRomPath = droppedFileStr;

                break;
        }

        fixed (SDL_Event* evtPtr = &evt) {
            ImGuiNative.ImGui_ImplSDL2_ProcessEvent(evtPtr);
        }
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

    private unsafe void DrawDockSpace() {
        ImGuiWindowClass windowClass = default;
        windowClass.DockNodeFlagsOverrideSet = ImGuiDockNodeFlags.AutoHideTabBar;
        windowClass.DockingAlwaysTabBar = 1;
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode, new ImGuiWindowClassPtr(&windowClass));
    }

    private void DrawContentInternal() {
        DrawMainMenu();
        DrawDockSpace();

        ImGui.ShowDemoWindow();

        float deltaTime = (float) DeltaTime;

        if (_loadingRomPath != null) {
            try {
                Emulator.LoadRom(_loadingRomPath);
            } catch (Exception e) {
                _notificationArea.ShowError($"Failed to load ROM '{Path.GetFileName(_loadingRomPath)}':\n{e.Message}");
            }

            _loadingRomPath = null;
        }

        using (Profiler.Begin("Emulator_Update", TimeUnit.Millisecond)) {
            Emulator.Update(DeltaTime);
        }

        CpuDebuggerWindow.Draw(deltaTime);
        PpuTileViewerWindow.Draw(deltaTime);
        ProfilerWindow.Draw(deltaTime);

        CpuMemoryEditor.Draw(deltaTime);
        MachineLogWindow.Draw(deltaTime);

        _notificationArea.Update(deltaTime);
        _notificationArea.Draw();
    }

    private void DrawMainMenu() {
        if (ImGui.BeginMainMenuBar()) {
            if (ImGui.BeginMenu("File")) {
                if (ImGui.MenuItem("Open...")) {
                    Task.Run(() => {
                        DialogResult dialogResult = Dialog.FileOpen("nes", null);
                        if (dialogResult.IsOk && !String.IsNullOrWhiteSpace(dialogResult.Path)) {
                            _loadingRomPath = dialogResult.Path;
                        }
                    });
                }

                ImGui.Separator();
                if (ImGui.MenuItem("Exit")) {
                    MainLoop.Quit();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Windows")) {
                static void DrawWindowItem(string name, AppWindow window) {
                    bool selected = window.IsOpen;
                    if (ImGui.MenuItem(name, null, ref selected)) {
                        if (selected) {
                            window.Open();
                        } else {
                            window.Close();
                        }
                    }
                }

                DrawWindowItem(IconsFontAwesome5.Bug + " CPU Debugger", CpuDebuggerWindow);
                DrawWindowItem("CPU Memory", CpuMemoryEditor);
                DrawWindowItem("CHR Viewer", PpuTileViewerWindow);
                DrawWindowItem("Profiler", ProfilerWindow);
                DrawWindowItem("Machine Log", MachineLogWindow);

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
