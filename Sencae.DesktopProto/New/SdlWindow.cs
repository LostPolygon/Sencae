using System;

namespace Sencae; 

public class SdlWindow : IDisposable {
    private IntPtr _sdlWindow;
    private IntPtr _sdlRenderer;

    private void ReleaseUnmanagedResources() {
        // TODO release unmanaged resources here
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SdlWindow() {
        ReleaseUnmanagedResources();
    }
}
