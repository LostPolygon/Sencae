using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Mime;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cyotek.Collections.Generic;
using ImGuiNET;
using SDL2;
using static SDL2.SDL;
using static Sencae.SdlAssertions;

namespace Sencae;

public class AnimatedWindow {
    public const float Duration = 1f;

    public bool TargetState;
    public float CurrentTime;
}

public unsafe class EmulatorRunner {
    private const int ScreenScale = 15;

    private readonly CommandLineOptions _commandLineOptions;
    private uint _sdlAudioDevice;
    private CircularBuffer<short> _soundCircularBuffer = new CircularBuffer<short>(8192);

    public EmulatorRunner(CommandLineOptions commandLineOptions) {
        _commandLineOptions = commandLineOptions;
    }

    public void Run() {
        // Create window
        SDL.SDL_SetHint("SDL_WINDOWS_DPI_AWARENESS", "permonitorv2");
        SDL.SDL_SetHint("SDL_WINDOWS_DPI_SCALING", "1");

        //Setup SDL hint
        SDL_SetHint(SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
        SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "nearest"); //Can be nearest, linear, or best

        AssertSdlSuccess(SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO | SDL_INIT_TIMER | SDL_INIT_GAMECONTROLLER));
        IntPtr window = AssertSdlSuccess(SDL_CreateWindow(
            "Sencae NES Emulator 2",
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            1280,
            720,
            SDL_WindowFlags.SDL_WINDOW_SHOWN |
            SDL_WindowFlags.SDL_WINDOW_RESIZABLE |
            SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI |
            SDL_WindowFlags.SDL_WINDOW_OPENGL
        ));
        IntPtr renderer = AssertSdlSuccess(SDL_CreateRenderer(
            window,
            -1,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
            SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC
        ));

        IntPtr imGuiContext = ImGui.CreateContext();
        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags |=
            ImGuiConfigFlags.NavEnableGamepad |
            ImGuiConfigFlags.NavEnableKeyboard |
            ImGuiConfigFlags.DpiEnableScaleFonts |
            ImGuiConfigFlags.DpiEnableScaleViewports |
            ImGuiConfigFlags.ViewportsEnable;

        //io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        //io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;

        SpectrumTheme.StyleColorsSpectrumDark();

        /*ImGuiStyle* style = ImGui.GetStyle();

        style->WindowPadding = new(15, 15);
        style->WindowRounding = 5.0f;
        style->FramePadding = new(5, 5);
        style->FrameRounding = 4.0f;
        style->ItemSpacing = new(12, 8);
        style->ItemInnerSpacing = new(8, 6);
        style->IndentSpacing = 25.0f;
        style->ScrollbarSize = 15.0f;
        style->ScrollbarRounding = 9.0f;
        style->GrabMinSize = 5.0f;
        style->GrabRounding = 3.0f;

        Vector4* styleColors = &style->Colors_0;
        styleColors[(int) ImGuiCol.Text] = new Vector4(0.80f, 0.80f, 0.83f, 1.00f);
        styleColors[(int) ImGuiCol.TextDisabled] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        styleColors[(int) ImGuiCol.WindowBg] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        styleColors[(int) ImGuiCol.PopupBg] = new Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        styleColors[(int) ImGuiCol.Border] = new Vector4(0.80f, 0.80f, 0.83f, 0.88f);
        styleColors[(int) ImGuiCol.BorderShadow] = new Vector4(0.92f, 0.91f, 0.88f, 0.00f);
        styleColors[(int) ImGuiCol.FrameBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        styleColors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        styleColors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        styleColors[(int) ImGuiCol.TitleBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        styleColors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(1.00f, 0.98f, 0.95f, 0.75f);
        styleColors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.07f, 0.07f, 0.09f, 1.00f);
        styleColors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        styleColors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        styleColors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        styleColors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        styleColors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        styleColors[(int) ImGuiCol.CheckMark] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        styleColors[(int) ImGuiCol.SliderGrab] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
        styleColors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        styleColors[(int) ImGuiCol.Button] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        styleColors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
        styleColors[(int) ImGuiCol.ButtonActive] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        styleColors[(int) ImGuiCol.Header] = new Vector4(0.10f, 0.09f, 0.12f, 1.00f);
        styleColors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        styleColors[(int) ImGuiCol.HeaderActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        styleColors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        styleColors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
        styleColors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
        styleColors[(int) ImGuiCol.PlotLines] = new Vector4(0.40f, 0.39f, 0.38f, 0.63f);
        styleColors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(0.25f, 1.00f, 0.00f, 1.00f);
        styleColors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.40f, 0.39f, 0.38f, 0.63f);
        styleColors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(0.25f, 1.00f, 0.00f, 1.00f);
        styleColors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.25f, 1.00f, 0.00f, 0.43f);*/

        ImFontConfig cfg;

        //io.Fonts.AddFontFromFileTTF("Ruda-Medium.ttf", 12);
        //io.Fonts.AddFontFromFileTTF("Ruda-Medium.ttf", 10);
        //io.Fonts.AddFontFromFileTTF("Ruda-Medium.ttf", 14);

        ImFontConfig config;
        config.FontDataOwnedByAtlas = 1;
        config.OversampleH = 3;
        config.OversampleV = 3;
        config.GlyphMaxAdvanceX = float.MaxValue;
        config.RasterizerMultiply = 1.0f;
        config.EllipsisChar = unchecked((ushort) -1);
        ImFontConfigPtr configPtr = new(&config);

        io.Fonts.AddFontFromFileTTF("Resources/Fonts/AdobeClean/AdobeClean-Regular.otf", 16, configPtr);

        SDL_SysWMinfo info;
        info = default;
        SDL_VERSION(out info.version);

        ImGui.ImGui_ImplSDL2_InitForSDLRenderer(window, renderer);
        Sdl2Renderer.ImGui_ImplSDLRenderer2_Init((SDL_Renderer*) renderer);

        //ImGui.sdl
        // Setup sound
        SDL_AudioSpec desiredSpec = new SDL_AudioSpec {
            freq = 44100,
            format = AUDIO_S16SYS,
            channels = 1,
            samples = 2048,
            callback = AudioCallback,
            userdata = IntPtr.Zero
        };

        // Don't allow any format changes
        _sdlAudioDevice = AssertSdlSuccessNonZero(SDL_OpenAudioDevice(null, 0, ref desiredSpec, out SDL_AudioSpec _, 0));
        SDL_PauseAudioDevice(_sdlAudioDevice, 0);

        MainLoop(renderer, window);

        // Clean up.
        SDL_CloseAudioDevice(_sdlAudioDevice);
        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
    }

    private static void AudioCallback(IntPtr userdata, IntPtr stream, int len) {
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void ImGuiDrawListCallback(ImDrawList* list, ImDrawCmd* cmd) {
        ImDrawListPtr listPtr = new ImDrawListPtr(list);
        listPtr.AddCircleFilled(new Vector2(100, 100), 600, 0xFFFFFFFF, 10);
        return;

        //Console.WriteLine(((ImDrawCmd*) cmd)->ElemCount);

        static Vector4 RgbColorToVector(uint color) {
            byte a = (byte) (color >> 24);
            byte r = (byte) (color >> 16);
            byte g = (byte) (color >> 8);
            byte b = (byte) (color >> 0);

            return new Vector4(r, g, b, a) * (1f / Byte.MaxValue);
        }

        static uint VectorToRgbColor(Vector4 color) {
            byte a = (byte) (color.W * Byte.MaxValue);
            byte b = (byte) (color.X * Byte.MaxValue);
            byte g = (byte) (color.Y * Byte.MaxValue);
            byte r = (byte) (color.Z * Byte.MaxValue);

            return (uint) ((a << 24) | (r << 16) | (g << 8) | (b << 0));
        }

        float lerp = (float) Math.Abs(Math.Cos((DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0) % 5000.0));

        ImDrawList* cmd_list = list;
        ImDrawVert* vtx_buffer = (ImDrawVert*) cmd_list->VtxBuffer.Data;

        for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++) {
            ImDrawCmd* pcmd = (ImDrawCmd*) cmd_list->CmdBuffer.Address<ImDrawCmd>(cmd_i);

            int num_vertices = (int) (cmd_list->VtxBuffer.Size - pcmd->VtxOffset);
            ImDrawVert* vertex = vtx_buffer + pcmd->VtxOffset;
            int stride = sizeof(ImDrawVert);
            
            for (int i = 0; i < num_vertices; i++) {
                ImDrawVert* vertexTemp = vertex + i;

                Vector4 vector = RgbColorToVector(vertexTemp->col);
                vector *= new Vector4(1, 1, 1, lerp);

                //vector *= Vector4.Zero;
                vertexTemp->col = VectorToRgbColor(vector);
                vertexTemp->col = 0;
            }
        }

        /*ImDrawListPtr imDrawListPtr = new ImDrawListPtr(list);
        imDrawListPtr.
        for (int i = 0; i < imDrawListPtr.VtxBuffer.Size; i++) {
            ImDrawVert* vertexTemp = imDrawListPtr.VtxBuffer[i];

            Vector4 vector = RgbColorToVector(vertexTemp->col);

            vector *= new Vector4(0, 0, 1, 1);

            //vector *= Vector4.Zero;
            vertexTemp->col = VectorToRgbColor(vector);
        }*/
    }

    // https://gist.github.com/kpcftsz/b044b43213564f2fb32e8685a50daf6a
    private static void RenderDropShadow(IntPtr tex_id, float shadowSize, byte shadowAlpha) {
        float inset = 5.5f;

        Vector2 leftTop = ImGui.GetWindowPos();
        Vector2 size = ImGui.GetWindowSize();

        size.X -= inset * 2f;
        size.Y -= inset * 2f;

        leftTop.X += inset;
        leftTop.Y += inset;

        Vector2 bottomRight = new(leftTop.X + size.X, leftTop.Y + size.Y);
        const float uv0 = 0.0f; // left/top region
        const float uv1 = 0.333333f; // leftward/upper region
        const float uv2 = 0.666666f; // rightward/lower region
        const float uv3 = 1.0f; // right/bottom region
        uint shadowColor = (uint) ((shadowAlpha << 24) | 0xFFFFFF);

        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        dl.PushClipRectFullScreen();
        dl.AddImage(tex_id, new(leftTop.X - shadowSize, leftTop.Y - shadowSize), new(leftTop.X, leftTop.Y), new(uv0, uv0), new(uv1, uv1), shadowColor);
        dl.AddImage(tex_id, new(leftTop.X, leftTop.Y - shadowSize), new(bottomRight.X, leftTop.Y), new(uv1, uv0), new(uv2, uv1), shadowColor);
        dl.AddImage(tex_id, new(bottomRight.X, leftTop.Y - shadowSize), new(bottomRight.X + shadowSize, leftTop.Y), new(uv2, uv0), new(uv3, uv1), shadowColor);
        dl.AddImage(tex_id, new(leftTop.X - shadowSize, leftTop.Y), new(leftTop.X, bottomRight.Y), new(uv0, uv1), new(uv1, uv2), shadowColor);
        dl.AddImage(tex_id, new(bottomRight.X, leftTop.Y), new(bottomRight.X + shadowSize, bottomRight.Y), new(uv2, uv1), new(uv3, uv2), shadowColor);
        dl.AddImage(tex_id, new(leftTop.X - shadowSize, bottomRight.Y), new(leftTop.X, bottomRight.Y + shadowSize), new(uv0, uv2), new(uv1, uv3), shadowColor);
        dl.AddImage(tex_id, new(leftTop.X, bottomRight.Y), new(bottomRight.X, bottomRight.Y + shadowSize), new(uv1, uv2), new(uv2, uv3), shadowColor);
        dl.AddImage(tex_id, new(bottomRight.X, bottomRight.Y), new(bottomRight.X + shadowSize, bottomRight.Y + shadowSize), new(uv2, uv2), new(uv3, uv3), shadowColor);
        dl.PopClipRect();
    }

    private void MainLoop(IntPtr renderer, IntPtr window) {
        SDL.SDL_Surface* emulatorSurface =
            (SDL.SDL_Surface*) SDL.SDL_CreateRGBSurfaceWithFormat(
                0,
                NesConstants.Ntsc.ScreenWidth,
                NesConstants.Ntsc.ScreenHeight,
                8,
                SDL.SDL_PIXELFORMAT_ARGB8888
            );

        IntPtr emulatorTexture = SDL.SDL_CreateTexture(
            renderer,
            SDL_PIXELFORMAT_ARGB8888,
            (int) SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            NesConstants.Ntsc.ScreenWidth,
            NesConstants.Ntsc.ScreenHeight
        );

        SDL_SetTextureBlendMode((IntPtr) emulatorTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        SDL_SetTextureScaleMode((IntPtr) emulatorTexture, SDL_ScaleMode.SDL_ScaleModeNearest);

        IntPtr shadowTexture = SDL_image.IMG_LoadTexture(renderer, "Resources/shadow.png");

        Vector4 clear_color = new Vector4(0.45f, 0.55f, 0.60f, 1.00f);
        ImGuiIOPtr io = ImGui.GetIO();

        bool quit = false;
        Stopwatch updateStopwatch = new Stopwatch();
        double frameDuration = 1.0 / NesConstants.Ntsc.Framerate;

        while (!quit) {
            bool wantsImguiUpdate = true;
            while (SDL_PollEvent(out SDL_Event evt) == 1) {
                wantsImguiUpdate = true;
                switch (evt.type) {
                    // Wait for user to close the window or press Esc
                    case SDL_EventType.SDL_MOUSEMOTION:
                        break;
                    case SDL_EventType.SDL_QUIT:
                        quit = true;
                        break;

                    case SDL_EventType.SDL_KEYDOWN:
                        if (evt.key.keysym.sym == SDL_Keycode.SDLK_ESCAPE) {
                            quit = true;
                        }

                        break;
                }

                ImGuiNative.ImGui_ImplSDL2_ProcessEvent(&evt);

                //_emulator.HandleSdlEvent(in evt);
            }

            int screenLength = NesConstants.Ntsc.ScreenWidth * NesConstants.Ntsc.ScreenHeight * 4;
            byte* pixels = (byte*) emulatorSurface->pixels;

            for (int i = 0; i < screenLength; i++) {
                pixels[i] = (byte) Random.Shared.Next();
            }

            IntPtr tempSurfacePtr;
            SDL.SDL_LockTextureToSurface(emulatorTexture, IntPtr.Zero, out tempSurfacePtr);
            SDL_Surface* tempSurface = (SDL_Surface*) tempSurfacePtr;

            //SDL_FillRect(tempSurfacePtr, IntPtr.Zero, 0x50FF1020);
            //SDL_BlitSurface((IntPtr) emulatorSurface, IntPtr.Zero, (IntPtr) tempSurface, IntPtr.Zero);

            for (int i = 0; i < screenLength; i++) {
                ((byte*) tempSurface->pixels)[i] = pixels[i];
            }

            SDL.SDL_UnlockTexture(emulatorTexture);

            //SDL_RenderCopy(renderer,emulatorTexture,IntPtr.Zero, IntPtr.Zero);
            //SDL.SDL_UpdateTexture(emulatorTexture, IntPtr.Zero, (IntPtr) pixels, emulatorSurface->pitch);

            SDL.SDL_Rect tRect;
            tRect.x = 50;
            tRect.y = 50;
            tRect.w = NesConstants.Ntsc.ScreenWidth * ScreenScale;
            tRect.h = NesConstants.Ntsc.ScreenHeight * ScreenScale;

            // Blit the emulator view

            //Update(_emulator, emulatorSurface, renderer);

            if (wantsImguiUpdate) {
                // Start the Dear ImGui frame
                ImGui.ImGui_ImplSDL2_NewFrame();
                Sdl2Renderer.ImGui_ImplSDLRenderer2_NewFrame();
                ImGui.NewFrame();

                SubmitUI(window);

                //ImGui.ShowDemoWindow();

                ImGui.Begin("SDL2/SDL_Renderer Texture Test");

                if (ImGui.IsWindowFocused()) {
                    RenderDropShadow(shadowTexture, 70.0f, 255);
                }

                Vector2 windowSize = ImGui.GetWindowSize();

                ImGui.Image(emulatorTexture, new(windowSize.X));
                
                ImDrawListPtr imDrawListPtr = ImGui.GetWindowDrawList();
                

                ImGui.End();

                //imDrawListPtr.AddCircleFilled(new Vector2(100, 100), 100, 0xFFFFFFFF, 100);
                
                //////////////////
    
                static Vector4 RgbColorToVector(uint color) {
                    byte a = (byte) (color >> 24);
                    byte r = (byte) (color >> 16);
                    byte g = (byte) (color >> 8);
                    byte b = (byte) (color >> 0);

                    return new Vector4(r, g, b, a) * (1f / Byte.MaxValue);
                }

                static uint VectorToRgbColor(Vector4 color) {
                    byte a = (byte) (color.W * Byte.MaxValue);
                    byte b = (byte) (color.X * Byte.MaxValue);
                    byte g = (byte) (color.Y * Byte.MaxValue);
                    byte r = (byte) (color.Z * Byte.MaxValue);

                    return (uint) ((a << 24) | (r << 16) | (g << 8) | (b << 0));
                }

                float lerp = (float) Math.Abs(Math.Cos((DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0) % 5000.0));

                ImDrawList* cmd_list = imDrawListPtr.NativePtr;
                ImDrawVert* vtx_buffer = (ImDrawVert*) cmd_list->VtxBuffer.Data;

                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++) {
                    ImDrawCmd* pcmd = (ImDrawCmd*) cmd_list->CmdBuffer.Address<ImDrawCmd>(cmd_i);

                    int num_vertices = (int) (cmd_list->VtxBuffer.Size - pcmd->VtxOffset);
                    ImDrawVert* vertex = vtx_buffer + pcmd->VtxOffset;
                    int stride = sizeof(ImDrawVert);
            
                    for (int i = 0; i < num_vertices; i++) {
                        ImDrawVert* vertexTemp = vertex + i;

                        Vector4 vector = RgbColorToVector(vertexTemp->col);
                        vector *= new Vector4(1, 0, 1, lerp);

                        //vector *= Vector4.Zero;
                        vertexTemp->col = VectorToRgbColor(vector);
                        //vertexTemp->col = 0;
                    }
                }

                

                //delegate* unmanaged[Cdecl] <ImDrawList*, ImDrawCmd*, void> ptr1 = &ImGuiDrawListCallback;
                //IntPtr fnPtr1 = (IntPtr) ptr1;
                //imDrawListPtr.AddCallback(fnPtr1, IntPtr.Zero);

                //SDL.SDL_GetRenderDriverInfo(0, out SDL_RendererInfo rendererInfo);
                //Console.WriteLine(SDL.SDL_GetCurrentVideoDriver());
            }

            ImGui.Render();
            SDL_SetRenderDrawColor(renderer, (byte) (clear_color.X * 255), (byte) (clear_color.Y * 255), (byte) (clear_color.Z * 255), (byte) (clear_color.W * 255));
            SDL_RenderClear(renderer);
            SDL_RenderSetScale(renderer, io.DisplayFramebufferScale.X, io.DisplayFramebufferScale.Y);
            Sdl2Renderer.ImGui_ImplSDLRenderer2_RenderDrawData(ImGui.GetDrawData());

            SDL_RenderPresent(renderer);

            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
                ImGui.UpdatePlatformWindows();
                ImGui.RenderPlatformWindowsDefault();
            }

            updateStopwatch.Stop();
            double updateElapsed = updateStopwatch.Elapsed.TotalSeconds;
            double delay = frameDuration - updateElapsed;
            if (delay > 0) {
                //SDL_Delay((uint) (delay * 1000));
            }

            //SDL_SetWindowTitle(window, $"Frame: {frames}");
            frames++;
        }

        SDL.SDL_DestroyTexture(emulatorTexture);
    }

    private ulong frames;

    private void Update(SDL_Surface* emulatorSurface, IntPtr renderer) {
        // Set renderer drawing color.
        AssertSdlSuccess(SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255));

        // Clear the screen.
        SDL_RenderClear(renderer);

        // Set target rectangle.
        SDL_Rect tRect;
        tRect.x = 0;
        tRect.y = 0;
        tRect.w = NesConstants.Ntsc.ScreenWidth * ScreenScale;
        tRect.h = NesConstants.Ntsc.ScreenHeight * ScreenScale;

        // Blit the emulator view
        IntPtr emulatorTexture = SDL_CreateTextureFromSurface(renderer, (IntPtr) emulatorSurface);
        SDL_RenderCopy(renderer, emulatorTexture, IntPtr.Zero, ref tRect);
        SDL_DestroyTexture(emulatorTexture);

        // Present the "Painting" (backbuffer) to the screen. Call this once per frame.
        SDL_RenderPresent(renderer);
    }

    /// <summary>
    /// UI
    /// </summary>

    // UI state
    private static MemoryEditor _memoryEditor = new MemoryEditor();

    private static float _f = 0.0f;

    private static int _counter = 0;
    private static int _dragInt = 0;
    private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
    private static bool _showImGuiDemoWindow = true;
    private static bool _showAnotherWindow = false;
    private static bool _showMemoryEditor = false;
    private static byte[] _memoryEditorData = new byte[128];
    private static uint s_tab_bar_flags = (uint) ImGuiTabBarFlags.Reorderable;
    static bool[] s_opened = { true, true, true, true }; // Persistent user state

    static void SetThing(out float i, float val) {
        i = val;
    }

    private static unsafe void SubmitUI(IntPtr window) {
        // Demo code adapted from the official Dear ImGui demo program:
        // https://github.com/ocornut/imgui/blob/master/examples/example_win32_directx11/main.cpp#L172

        // 1. Show a simple window.
        // Tip: if we don't call ImGui.BeginWindow()/ImGui.EndWindow() the widgets automatically appears in a window called "Debug".
        {
            float ddpi;
            float hdpi;
            float vdpi;

            ImGui.Begin("Cool!");
            SDL.SDL_GetDisplayDPI(0, out ddpi, out hdpi, out vdpi);
            ImGui.Text($"0 ddpi: {ddpi}, hdpi: {hdpi}, vdpi: {vdpi}");

            SDL.SDL_GetDisplayDPI(1, out ddpi, out hdpi, out vdpi);
            ImGui.Text("Current monitor: " + SDL.SDL_GetWindowDisplayIndex(window));

            SDL_Rect monitorRect;
            SDL_GetDisplayBounds(0, out monitorRect);

            ImGui.Text($"{monitorRect.w}x{monitorRect.h}");

            ImGui.Text($"1 ddpi: {ddpi}, hdpi: {hdpi}, vdpi: {vdpi}");

            ImGui.Text(string.Empty);
            ImGui.Text("Hello, world!"); // Display some text (you can use a format string too)
            ImGui.SliderFloat("float", ref _f, 0, 1, _f.ToString("0.000")); // Edit 1 float using a slider from 0.0f to 1.0f    

            //ImGui.ColorEdit3("clear color", ref _clearColor);                   // Edit 3 floats representing a color

            ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");

            ImGui.Checkbox("ImGui Demo Window", ref _showImGuiDemoWindow); // Edit bools storing our windows open/close state
            ImGui.Checkbox("Another Window", ref _showAnotherWindow);
            ImGui.Checkbox("Memory Editor", ref _showMemoryEditor);
            if (ImGui.Button("Button")) // Buttons return true when clicked (NB: most widgets return true when edited/activated)
                _counter++;
            ImGui.SameLine(0, -1);
            ImGui.Text($"counter = {_counter}");

            ImGui.DragInt("Draggable Int", ref _dragInt);

            float framerate = ImGui.GetIO().Framerate;

            ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");

            ImGui.End();
        }

        // 2. Show another simple window. In most cases you will use an explicit Begin/End pair to name your windows.
        if (_showAnotherWindow) {
            ImGui.Begin("Another Window", ref _showAnotherWindow);
            ImGui.Text("Hello from another window!");
            if (ImGui.Button("Close Me"))
                _showAnotherWindow = false;
            ImGui.End();
        }

        // 3. Show the ImGui demo window. Most of the sample code is in ImGui.ShowDemoWindow(). Read its code to learn more about Dear ImGui!
        if (_showImGuiDemoWindow) {
            // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
            // Here we just want to make the demo initial state a bit more friendly!
            ImGui.SetNextWindowPos(new(650, 20), ImGuiCond.FirstUseEver);
            ImGui.ShowDemoWindow(ref _showImGuiDemoWindow);
        }

        if (ImGui.TreeNode("Tabs")) {
            if (ImGui.TreeNode("Basic")) {
                ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
                if (ImGui.BeginTabBar("MyTabBar", tab_bar_flags)) {
                    if (ImGui.BeginTabItem("Avocado")) {
                        ImGui.Text("This is the Avocado tab!\nblah blah blah blah blah");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Broccoli")) {
                        ImGui.Text("This is the Broccoli tab!\nblah blah blah blah blah");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Cucumber")) {
                        ImGui.Text("This is the Cucumber tab!\nblah blah blah blah blah");
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }

                ImGui.Separator();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Advanced & Close Button")) {
                // Expose a couple of the available flags. In most cases you may just call BeginTabBar() with no flags (0).
                ImGui.CheckboxFlags("ImGuiTabBarFlags_Reorderable", ref s_tab_bar_flags, (uint) ImGuiTabBarFlags.Reorderable);
                ImGui.CheckboxFlags("ImGuiTabBarFlags_AutoSelectNewTabs", ref s_tab_bar_flags, (uint) ImGuiTabBarFlags.AutoSelectNewTabs);
                ImGui.CheckboxFlags("ImGuiTabBarFlags_NoCloseWithMiddleMouseButton", ref s_tab_bar_flags, (uint) ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
                if ((s_tab_bar_flags & (uint) ImGuiTabBarFlags.FittingPolicyMask) == 0)
                    s_tab_bar_flags |= (uint) ImGuiTabBarFlags.FittingPolicyDefault;
                if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyResizeDown", ref s_tab_bar_flags, (uint) ImGuiTabBarFlags.FittingPolicyResizeDown))
                    s_tab_bar_flags &= ~((uint) ImGuiTabBarFlags.FittingPolicyMask ^ (uint) ImGuiTabBarFlags.FittingPolicyResizeDown);
                if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyScroll", ref s_tab_bar_flags, (uint) ImGuiTabBarFlags.FittingPolicyScroll))
                    s_tab_bar_flags &= ~((uint) ImGuiTabBarFlags.FittingPolicyMask ^ (uint) ImGuiTabBarFlags.FittingPolicyScroll);

                // Tab Bar
                string[] names = { "Artichoke", "Beetroot", "Celery", "Daikon" };

                for (int n = 0; n < s_opened.Length; n++) {
                    if (n > 0) {
                        ImGui.SameLine();
                    }

                    ImGui.Checkbox(names[n], ref s_opened[n]);
                }

                // Passing a bool* to BeginTabItem() is similar to passing one to Begin(): the underlying bool will be set to false when the tab is closed.
                if (ImGui.BeginTabBar("MyTabBar", (ImGuiTabBarFlags) s_tab_bar_flags)) {
                    for (int n = 0; n < s_opened.Length; n++)
                        if (s_opened[n] && ImGui.BeginTabItem(names[n], ref s_opened[n])) {
                            ImGui.Text($"This is the {names[n]} tab!");
                            if ((n & 1) != 0)
                                ImGui.Text("I am an odd tab.");
                            ImGui.EndTabItem();
                        }

                    ImGui.EndTabBar();
                }

                ImGui.Separator();
                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        ImGuiIOPtr io = ImGui.GetIO();
        SetThing(out io.DeltaTime, 2f);

        if (_showMemoryEditor) {
            ImGui.Text("Memory editor currently supported.");
            _memoryEditor.Draw("Memory Editor", _memoryEditorData, _memoryEditorData.Length);
        }
    }
}
