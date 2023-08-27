namespace Sencae.Desktop; 

public unsafe class DynamicSdlTexture : IDisposable {
    public IntPtr Renderer { get; }
    public IntPtr Texture { get; }
    public uint Format { get; }
    public int Width { get; }
    public int Height { get; }

    public DynamicSdlTexture(
        IntPtr renderer,
        int width,
        int height, 
        uint? format = null,
        SDL_TextureAccess access = SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING
        ) {
        Renderer = renderer;
        Format = format ?? SDL_PIXELFORMAT_ARGB8888;
        Width = width;
        Height = height;

        Texture = SDL_CreateTexture(
            Renderer,
            format.Value,
            (int) access,
            Width,
            Height
        );
        
        SDL_SetTextureBlendMode(Texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        SDL_SetTextureScaleMode(Texture, SDL_ScaleMode.SDL_ScaleModeNearest);
    }

    public LockedSurface Lock() {
        return new LockedSurface(Texture);
    }

    private void ReleaseUnmanagedResources() {
        SDL_DestroyTexture(Texture);
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~DynamicSdlTexture() {
        ReleaseUnmanagedResources();
    }

    public readonly struct LockedSurface : IDisposable {
        private IntPtr Texture { get; }
        
        public SDL_Surface* Surface { get; }

        public LockedSurface(IntPtr texture) {
            Texture = texture;
            SDL_LockTextureToSurface(Texture, IntPtr.Zero, out IntPtr tempSurfacePtr);

            Surface = (SDL_Surface*) tempSurfacePtr;
        }

        public void Dispose() {
            SDL_UnlockTexture(Texture);
        }
    }
}
