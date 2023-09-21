using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using Sencae.Ppu;

namespace Sencae.Desktop; 

public class SharedResources : IDisposable {
    public IntPtr ShadowTexture { get; }
    public required ImFontPtr FontMonospacedNormal { get; init; } 
    public required ImFontPtr FontFontAwesomeIconsSolid { get; init; }
    public required PpuPalette PpuPalette;

    public SharedResources(IntPtr renderer) {
        ShadowTexture = SDL_image.IMG_LoadTexture(renderer, "Resources/window-shadow.png");
        SDL_SetTextureBlendMode(ShadowTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        SDL_SetTextureScaleMode(ShadowTexture, SDL_ScaleMode.SDL_ScaleModeBest);
    }

    private void ReleaseUnmanagedResources() {
        SDL_DestroyTexture(ShadowTexture);
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SharedResources() {
        ReleaseUnmanagedResources();
    }
}
