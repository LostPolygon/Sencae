using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Sencae.Desktop.Sdl;
using static SDL2.SDL.SDL_bool;
using ImDrawIdx = System.UInt16;

namespace Sencae.Desktop;

public static unsafe class Sdl2Renderer {
    //private static readonly IntPtr BackendRendererNamePtr = Marshal.StringToHGlobalAnsi("imgui_impl_sdlrenderer2_cs_port");

    private static ImGui_ImplSDLRenderer2_Data* ImGui_ImplSDLRenderer2_GetBackendData_Internal() {
        if (ImGui.GetCurrentContext() == IntPtr.Zero)
            return null;

        ImGuiIOPtr io = ImGui.GetIO();
        return (ImGui_ImplSDLRenderer2_Data*) io.BackendRendererUserData;
    }

    // Backend data stored in io.BackendRendererUserData to allow support for multiple Dear ImGui contexts
    // It is STRONGLY preferred that you use docking branch with multi-viewports (== single Dear ImGui context + multiple windows) instead of multiple Dear ImGui contexts.
    public static IntPtr ImGui_ImplSDLRenderer2_GetBackendData() {
        return (IntPtr) ImGui_ImplSDLRenderer2_GetBackendData_Internal();
    }

    // Functions
    public static bool ImGui_ImplSDLRenderer2_Init(IntPtr renderer) {
        ImGuiIOPtr io = ImGui.GetIO();
        Debug.Assert(io.BackendRendererUserData == IntPtr.Zero, "Already initialized a renderer backend!");
        Debug.Assert(renderer != IntPtr.Zero, "SDL_Renderer not initialized!");

        // Setup backend capabilities flags
        ImGui_ImplSDLRenderer2_Data* bd = (ImGui_ImplSDLRenderer2_Data*) Marshal.AllocHGlobal(sizeof(ImGui_ImplSDLRenderer2_Data));
        *bd = default;
        io.BackendRendererUserData = (IntPtr) bd;

        ReadOnlySpan<byte> readOnlySpan = "imgui_impl_sdlrenderer2_cs_port"u8;
        io.NativePtr->BackendRendererName = (byte*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan));
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset; // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.

        bd->SDLRenderer = renderer;

        return true;
    }

    public static void ImGui_ImplSDLRenderer2_Shutdown() {
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData_Internal();
        Debug.Assert(bd != null, "No renderer backend to shutdown, or already shutdown?");
        ImGuiIOPtr io = ImGui.GetIO();

        ImGui_ImplSDLRenderer2_DestroyDeviceObjects();

        io.NativePtr->BackendRendererName = null;
        io.BackendRendererUserData = IntPtr.Zero;
        io.BackendFlags &= ~ImGuiBackendFlags.RendererHasVtxOffset;

        Marshal.FreeHGlobal((IntPtr) bd);
    }

    static void ImGui_ImplSDLRenderer2_SetupRenderState() {
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData_Internal();

        // Clear out any viewports and cliprect set by the user
        // FIXME: Technically speaking there are lots of other things we could backup/setup/restore during our render process.
        NativeMethods.SDL_RenderSetViewport(bd->SDLRenderer, null);
        SDL_RenderSetClipRect(bd->SDLRenderer, IntPtr.Zero);
    }

    public static void ImGui_ImplSDLRenderer2_NewFrame() {
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData_Internal();
        Debug.Assert(bd != null, "Did you call ImGui_ImplSDLRenderer2_Init()?");

        if (bd->FontSDLTexture == IntPtr.Zero)
            ImGui_ImplSDLRenderer2_CreateDeviceObjects();
    }

    public static void ImGui_ImplSDLRenderer2_RenderDrawData(ImDrawData* drawData) {
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData_Internal();

        // If there's a scale factor set by the user, use that instead
        // If the user has specified a scale factor to SDL_Renderer already via SDL_RenderSetScale(), SDL will scale whatever we pass
        // to SDL_RenderGeometryRaw() by that scale factor. In that case we don't want to be also scaling it ourselves here.
        Vector2 renderScale = default;
        /*SDL_RenderGetScale(bd->SDLRenderer, out renderScale.X, out renderScale.Y);
        renderScale.X = renderScale.X == 1.0f ? draw_data->FramebufferScale.X : 1.0f;
        renderScale.Y = renderScale.X == 1.0f ? draw_data->FramebufferScale.Y : 1.0f;*/
        renderScale = Vector2.One;

        // Avoid rendering when minimized, scale coordinates for retina displays (screen coordinates != framebuffer coordinates)
        int framebufferWidth = (int) (drawData->DisplaySize.X * renderScale.X);
        int framebufferHeight = (int) (drawData->DisplaySize.Y * renderScale.Y);
        if (framebufferWidth == 0 || framebufferHeight == 0)
            return;

        // Backup SDL_Renderer state that will be modified to restore it afterwards

        BackupSDLRendererState oldRendererState;
        oldRendererState.ClipEnabled = SDL_RenderIsClipEnabled(bd->SDLRenderer) == SDL_TRUE;
        SDL_RenderGetViewport(bd->SDLRenderer, out oldRendererState.Viewport);
        SDL_RenderGetClipRect(bd->SDLRenderer, out oldRendererState.ClipRect);

        // Will project scissor/clipping rectangles into framebuffer space
        Vector2 clipOff = drawData->DisplayPos; // (0,0) unless using multi-viewports
        Vector2 clipScale = Vector2.One;

        // Render command lists
        ImGui_ImplSDLRenderer2_SetupRenderState();
        for (int cmdListIndex = 0; cmdListIndex < drawData->CmdListsCount; cmdListIndex++) {
            ImDrawList* cmdList = drawData->CmdLists[cmdListIndex];
            ImDrawVert* vtxBuffer = (ImDrawVert*) cmdList->VtxBuffer.Data;
            ImDrawIdx* idxBuffer = (ImDrawIdx*) cmdList->IdxBuffer.Data;

            for (int cmdIndex = 0; cmdIndex < cmdList->CmdBuffer.Size; cmdIndex++) {
                ImDrawCmd* cmd = (ImDrawCmd*) cmdList->CmdBuffer.Address<ImDrawCmd>(cmdIndex);

                if (cmd->UserCallback != IntPtr.Zero) {
                    // User callback, registered via ImDrawList::AddCallback()
                    // (ImDrawCallback_ResetRenderState is a special callback value used by the user to request the renderer to reset render state.)
                    const nint ImDrawCallback_ResetRenderState = -1;
                    if (cmd->UserCallback == ImDrawCallback_ResetRenderState) {
                        ImGui_ImplSDLRenderer2_SetupRenderState();
                    } else {
                        delegate* unmanaged[Cdecl] <void*, void*, void> userCallback =
                            (delegate* unmanaged[Cdecl] <void*, void*, void>) cmd->UserCallback;
                        userCallback(cmdList, cmd);
                    }
                } else {
                    // Project scissor/clipping rectangles into framebuffer space
                    Vector4 clipMin = new(
                        (cmd->ClipRect.X - clipOff.X) * clipScale.X,
                        (cmd->ClipRect.Y - clipOff.Y) * clipScale.Y,
                        0,
                        0
                    );
                    Vector4 clipMax = new(
                        (cmd->ClipRect.Z - clipOff.X) * clipScale.X,
                        (cmd->ClipRect.W - clipOff.Y) * clipScale.Y,
                        0,
                        0
                    );
                    if (clipMin.X < 0.0f) {
                        clipMin.X = 0.0f;
                    }

                    if (clipMin.Y < 0.0f) {
                        clipMin.Y = 0.0f;
                    }

                    if (clipMax.X > framebufferWidth) {
                        clipMax.X = framebufferWidth;
                    }

                    if (clipMax.Y > framebufferHeight) {
                        clipMax.Y = framebufferHeight;
                    }

                    if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                        continue;

                    SDL_Rect clipRect = new() {
                        x = (int) clipMin.X,
                        y = (int) clipMin.Y,
                        w = (int) (clipMax.X - clipMin.X),
                        h = (int) (clipMax.Y - clipMin.Y)
                    };
                    SDL_RenderSetClipRect(bd->SDLRenderer, ref clipRect);
                    ImDrawVert* vertex = vtxBuffer + cmd->VtxOffset;
                    int stride = sizeof(ImDrawVert);

                    /*static Vector4 RgbColorToVector(uint color) {
                        byte a = (byte) (color >> 24);
                        byte r = (byte) (color >> 16);
                        byte g = (byte) (color >> 8);
                        byte b = (byte) (color >> 0);

                        return new Vector4(r, g, b, a) * (1f / Byte.MaxValue);
                    }

                    static uint VectorToRgbColor(Vector4 color) {
                        byte a = (byte) (color.W * Byte.MaxValue);
                        byte r = (byte) (color.X * Byte.MaxValue);
                        byte g = (byte) (color.Y * Byte.MaxValue);
                        byte b = (byte) (color.Z * Byte.MaxValue);

                        return (uint) ((a << 24) | (r << 16) | (g << 8) | (b << 0));
                    }

                    float lerp = (float) Math.Abs(Math.Cos((DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0) % 5000.0));
                    if (cmd_i != 3) {
                        lerp = 1;
                    }

                    int num_vertices = (int) (cmd_list->VtxBuffer.Size - pcmd.VtxOffset);
                    for (int i = 0; i < num_vertices; i++) {
                        ImDrawVert* vertexTemp = vertex + i;

                        Vector4 vector = RgbColorToVector(vertexTemp->col);
                        vector *= new Vector4(1, 1, 1, lerp);
                        //vector *= Vector4.Zero;
                        vertexTemp->col = VectorToRgbColor(vector);
                    }*/

                    // Bind texture, Draw
                    IntPtr texture = cmd->TextureId;
                    SdlAssertions.AssertSdlSuccess(NativeMethods.SDL_RenderGeometryRaw(
                        bd->SDLRenderer,
                        texture,
                        (float*) &vertex->pos,
                        stride,
                        (int*) &vertex->col,
                        stride,
                        (float*) &vertex->uv,
                        stride,
                        (int) (cmdList->VtxBuffer.Size - cmd->VtxOffset),
                        (IntPtr) (idxBuffer + cmd->IdxOffset),
                        (int) cmd->ElemCount,
                        sizeof(ImDrawIdx)
                    ));
                }
            }
        }

        // Restore modified SDL_Renderer state
        SDL_RenderSetViewport(bd->SDLRenderer, ref oldRendererState.Viewport);
        NativeMethods.SDL_RenderSetClipRect(bd->SDLRenderer, oldRendererState.ClipEnabled ? &oldRendererState.ClipRect : null);
    }

    // Called by Init/NewFrame/Shutdown
    public static bool ImGui_ImplSDLRenderer2_CreateFontsTexture() {
        ImGuiIOPtr io = ImGui.GetIO();
        ImGui_ImplSDLRenderer2_Data* backendData = ImGui_ImplSDLRenderer2_GetBackendData_Internal();

        // Build texture atlas
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);

        // Load as RGBA 32-bit (75% of the memory is wasted, but default font is so small)
        // because it is more likely to be compatible with user's existing shaders.
        // If your ImTextureId represent a higher-level concept than just a GL texture id,
        // consider calling GetTexDataAsAlpha8() instead to save on GPU memory.

        // Upload texture to graphics system
        // (Bilinear sampling is required by default. Set 'io.Fonts->Flags |= ImFontAtlasFlags_NoBakedLines' or 'style.AntiAliasedLinesUseTex = false' to allow point/nearest sampling)
        backendData->FontSDLTexture = SDL_CreateTexture(
            backendData->SDLRenderer,
            SDL_PIXELFORMAT_ABGR8888,
            (int) SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC,
            width,
            height
        );
        if (backendData->FontSDLTexture == IntPtr.Zero) {
            SDL_Log("error creating texture");
            return false;
        }

        SDL_UpdateTexture(backendData->FontSDLTexture, IntPtr.Zero, (IntPtr) pixels, 4 * width);
        SDL_SetTextureBlendMode(backendData->FontSDLTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        SDL_SetTextureScaleMode(backendData->FontSDLTexture, SDL_ScaleMode.SDL_ScaleModeLinear);

        // Store our identifier
        io.Fonts.TexID = backendData->FontSDLTexture;

        return true;
    }

    public static void ImGui_ImplSDLRenderer2_DestroyFontsTexture() {
        ImGuiIOPtr io = ImGui.GetIO();
        ImGui_ImplSDLRenderer2_Data* backendData = ImGui_ImplSDLRenderer2_GetBackendData_Internal();
        if (backendData->FontSDLTexture != IntPtr.Zero) {
            io.Fonts.TexID = IntPtr.Zero;
            SDL_DestroyTexture(backendData->FontSDLTexture);
            backendData->FontSDLTexture = IntPtr.Zero;
        }
    }

    public static bool ImGui_ImplSDLRenderer2_CreateDeviceObjects() {
        return ImGui_ImplSDLRenderer2_CreateFontsTexture();
    }

    public static void ImGui_ImplSDLRenderer2_DestroyDeviceObjects() {
        ImGui_ImplSDLRenderer2_DestroyFontsTexture();
    }

    private struct ImGui_ImplSDLRenderer2_Data {
        public IntPtr SDLRenderer;
        public IntPtr FontSDLTexture;
    }

    private struct BackupSDLRendererState {
        public SDL_Rect Viewport;
        public bool ClipEnabled;
        public SDL_Rect ClipRect;
    }

    private static class NativeMethods {
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderSetViewport(IntPtr renderer, SDL_Rect* rect);

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderSetClipRect(IntPtr renderer, SDL_Rect* rect);

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderGeometryRaw(
            IntPtr renderer,
            IntPtr texture,
            float* xy,
            int xy_stride,
            int* color,
            int color_stride,
            float* uv,
            int uv_stride,
            int num_vertices,
            IntPtr indices,
            int num_indices,
            int size_indices
        );
    }
}
