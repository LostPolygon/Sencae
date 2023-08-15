using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using static SDL2.SDL;
using static SDL2.SDL.SDL_bool;
using ImDrawIdx = System.UInt16;

namespace Sencae;

public struct SDL_Renderer {
}

public struct SDL_Texture {
}

public unsafe struct ImGui_ImplSDLRenderer2_Data {
    public SDL_Renderer* SDLRenderer;
    public SDL_Texture* FontTexture;
}

file struct BackupSDLRendererState {
    public SDL_Rect Viewport;
    public bool ClipEnabled;
    public SDL_Rect ClipRect;
}

public static unsafe class Sdl2Renderer {
    private static readonly IntPtr BackendRendererNamePtr = Marshal.StringToHGlobalAnsi("imgui_impl_sdlrenderer2_cs_port");

    // Backend data stored in io.BackendRendererUserData to allow support for multiple Dear ImGui contexts
    // It is STRONGLY preferred that you use docking branch with multi-viewports (== single Dear ImGui context + multiple windows) instead of multiple Dear ImGui contexts.
    static ImGui_ImplSDLRenderer2_Data* ImGui_ImplSDLRenderer2_GetBackendData() {
        if (ImGui.GetCurrentContext() == IntPtr.Zero)
            return null;

        ImGuiIOPtr io = ImGui.GetIO();
        return (ImGui_ImplSDLRenderer2_Data*) io.BackendRendererUserData;
    }

    // Functions
    public static bool ImGui_ImplSDLRenderer2_Init(SDL_Renderer* renderer) {
        ImGuiIOPtr io = ImGui.GetIO();
        Debug.Assert(io.BackendRendererUserData == IntPtr.Zero, "Already initialized a renderer backend!");
        Debug.Assert(renderer != null, "SDL_Renderer not initialized!");

        // Setup backend capabilities flags
        ImGui_ImplSDLRenderer2_Data* bd = (ImGui_ImplSDLRenderer2_Data*) Marshal.AllocHGlobal(sizeof(ImGui_ImplSDLRenderer2_Data));
        *bd = default;
        io.BackendRendererUserData = (IntPtr) bd;
        io.NativePtr->BackendRendererName = (byte*) BackendRendererNamePtr;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset; // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.

        bd->SDLRenderer = renderer;

        return true;
    }

    public static void ImGui_ImplSDLRenderer2_Shutdown() {
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData();
        Debug.Assert(bd != null, "No renderer backend to shutdown, or already shutdown?");
        ImGuiIOPtr io = ImGui.GetIO();

        ImGui_ImplSDLRenderer2_DestroyDeviceObjects();

        io.NativePtr->BackendRendererName = null;
        io.BackendRendererUserData = IntPtr.Zero;
        io.BackendFlags &= ~ImGuiBackendFlags.RendererHasVtxOffset;

        Marshal.FreeHGlobal((IntPtr) bd);
    }

    static void ImGui_ImplSDLRenderer2_SetupRenderState() {
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData();

        // Clear out any viewports and cliprect set by the user
        // FIXME: Technically speaking there are lots of other things we could backup/setup/restore during our render process.
        NativeMethods.SDL_RenderSetViewport((IntPtr) bd->SDLRenderer, null);
        SDL_RenderSetClipRect((IntPtr) bd->SDLRenderer, IntPtr.Zero);
    }

    public static void ImGui_ImplSDLRenderer2_NewFrame() {
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData();
        Debug.Assert(bd != null, "Did you call ImGui_ImplSDLRenderer2_Init()?");

        if (bd->FontTexture == null)
            ImGui_ImplSDLRenderer2_CreateDeviceObjects();
    }

    public static void ImGui_ImplSDLRenderer2_RenderDrawData(ImDrawData* draw_data) {
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData();

        // If there's a scale factor set by the user, use that instead
        // If the user has specified a scale factor to SDL_Renderer already via SDL_RenderSetScale(), SDL will scale whatever we pass
        // to SDL_RenderGeometryRaw() by that scale factor. In that case we don't want to be also scaling it ourselves here.
        SDL_RenderGetScale((IntPtr) bd->SDLRenderer, out float rsx, out float rsy);
        Vector2 render_scale;
        render_scale.X = rsx == 1.0f ? draw_data->FramebufferScale.X : 1.0f;
        render_scale.Y = rsy == 1.0f ? draw_data->FramebufferScale.Y : 1.0f;
        render_scale = Vector2.One;

        // Avoid rendering when minimized, scale coordinates for retina displays (screen coordinates != framebuffer coordinates)
        int fb_width = (int) (draw_data->DisplaySize.X * render_scale.X);
        int fb_height = (int) (draw_data->DisplaySize.Y * render_scale.Y);
        if (fb_width == 0 || fb_height == 0)
            return;

        // Backup SDL_Renderer state that will be modified to restore it afterwards

        BackupSDLRendererState old = default;
        old.ClipEnabled = SDL_RenderIsClipEnabled((IntPtr) bd->SDLRenderer) == SDL_TRUE;
        SDL_RenderGetViewport((IntPtr) bd->SDLRenderer, out old.Viewport);
        SDL_RenderGetClipRect((IntPtr) bd->SDLRenderer, out old.ClipRect);

        // Will project scissor/clipping rectangles into framebuffer space
        Vector2 clip_off = draw_data->DisplayPos; // (0,0) unless using multi-viewports
        Vector2 clip_scale = Vector2.One;

        // Render command lists
        ImGui_ImplSDLRenderer2_SetupRenderState();
        for (int n = 0; n < draw_data->CmdListsCount; n++) {
            ImDrawList* cmd_list = draw_data->CmdLists[n];
            ImDrawVert* vtx_buffer = (ImDrawVert*) cmd_list->VtxBuffer.Data;
            ImDrawIdx* idx_buffer = (ImDrawIdx*) cmd_list->IdxBuffer.Data;

            for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++) {
                ImDrawCmd* pcmd = (ImDrawCmd*) cmd_list->CmdBuffer.Address<ImDrawCmd>(cmd_i);
                if (pcmd->UserCallback != IntPtr.Zero) {
                    // User callback, registered via ImDrawList::AddCallback()
                    // (ImDrawCallback_ResetRenderState is a special callback value used by the user to request the renderer to reset render state.)
                    const nint ImDrawCallback_ResetRenderState = -1;
                    if (pcmd->UserCallback == ImDrawCallback_ResetRenderState) {
                        ImGui_ImplSDLRenderer2_SetupRenderState();
                    } else {
                        delegate* unmanaged[Cdecl] <void*, void*, void> userCallback = 
                            (delegate* unmanaged[Cdecl] <void*, void*, void>)pcmd->UserCallback;
                        userCallback(cmd_list, pcmd);
                    }
                } else {
                    // Project scissor/clipping rectangles into framebuffer space
                    Vector4 clip_min = new((pcmd->ClipRect.X - clip_off.X) * clip_scale.X, (pcmd->ClipRect.Y - clip_off.Y) * clip_scale.Y, 0, 0);
                    Vector4 clip_max = new((pcmd->ClipRect.Z - clip_off.X) * clip_scale.X, (pcmd->ClipRect.W - clip_off.Y) * clip_scale.Y, 0, 0);
                    if (clip_min.X < 0.0f) {
                        clip_min.X = 0.0f;
                    }

                    if (clip_min.Y < 0.0f) {
                        clip_min.Y = 0.0f;
                    }

                    if (clip_max.X > fb_width) {
                        clip_max.X = fb_width;
                    }

                    if (clip_max.Y > fb_height) {
                        clip_max.Y = fb_height;
                    }

                    if (clip_max.X <= clip_min.X || clip_max.Y <= clip_min.Y)
                        continue;

                    SDL_Rect r = new() {
                        x = (int) clip_min.X,
                        y = (int) clip_min.Y,
                        w = (int) (clip_max.X - clip_min.X),
                        h = (int) (clip_max.Y - clip_min.Y)
                    };
                    SDL_RenderSetClipRect((IntPtr) bd->SDLRenderer, ref r);
                    ImDrawVert* vertex = vtx_buffer + pcmd->VtxOffset;
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
                    SDL_Texture* tex = (SDL_Texture*) pcmd->TextureId;
                    NativeMethods.SDL_RenderGeometryRaw(
                        (IntPtr) bd->SDLRenderer,
                        (IntPtr) tex,
                        (float*) &vertex->pos,
                        stride,
                        (int*) &vertex->col,
                        stride,
                        (float*) &vertex->uv,
                        stride,
                        (int) (cmd_list->VtxBuffer.Size - pcmd->VtxOffset),
                        (IntPtr) (idx_buffer + pcmd->IdxOffset),
                        (int) pcmd->ElemCount,
                        sizeof(ImDrawIdx)
                    );
                }
            }
        }

        // Restore modified SDL_Renderer state
        SDL_RenderSetViewport((IntPtr) bd->SDLRenderer, ref old.Viewport);
        NativeMethods.SDL_RenderSetClipRect((IntPtr) bd->SDLRenderer, old.ClipEnabled ? &old.ClipRect : null);
    }

    // Called by Init/NewFrame/Shutdown
    public static bool ImGui_ImplSDLRenderer2_CreateFontsTexture() {
        ImGuiIOPtr io = ImGui.GetIO();
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData();

        // Build texture atlas
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height); 
        // Load as RGBA 32-bit (75% of the memory is wasted, but default font is so small) because it is more likely to be compatible with user's existing shaders. If your ImTextureId represent a higher-level concept than just a GL texture id, consider calling GetTexDataAsAlpha8() instead to save on GPU memory.

        // Upload texture to graphics system
        // (Bilinear sampling is required by default. Set 'io.Fonts->Flags |= ImFontAtlasFlags_NoBakedLines' or 'style.AntiAliasedLinesUseTex = false' to allow point/nearest sampling)
        bd->FontTexture = (SDL_Texture*) SDL_CreateTexture((IntPtr) bd->SDLRenderer, SDL_PIXELFORMAT_ABGR8888, (int) SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC, width, height);
        if (bd->FontTexture == null) {
            SDL_Log("error creating texture");
            return false;
        }

        SDL_UpdateTexture((IntPtr) bd->FontTexture, IntPtr.Zero, (IntPtr) pixels, 4 * width);
        SDL_SetTextureBlendMode((IntPtr) bd->FontTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        SDL_SetTextureScaleMode((IntPtr) bd->FontTexture, SDL_ScaleMode.SDL_ScaleModeLinear);

        // Store our identifier
        io.Fonts.TexID = (IntPtr) bd->FontTexture;

        return true;
    }

    public static void ImGui_ImplSDLRenderer2_DestroyFontsTexture() {
        ImGuiIOPtr io = ImGui.GetIO();
        ImGui_ImplSDLRenderer2_Data* bd = ImGui_ImplSDLRenderer2_GetBackendData();
        if (bd->FontTexture != null) {
            io.Fonts.TexID = IntPtr.Zero;
            SDL_DestroyTexture((IntPtr) bd->FontTexture);
            bd->FontTexture = null;
        }
    }

    public static bool ImGui_ImplSDLRenderer2_CreateDeviceObjects() {
        return ImGui_ImplSDLRenderer2_CreateFontsTexture();
    }

    public static void ImGui_ImplSDLRenderer2_DestroyDeviceObjects() {
        ImGui_ImplSDLRenderer2_DestroyFontsTexture();
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
