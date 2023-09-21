using System.Diagnostics;
using System.Runtime.InteropServices;
using static SDL2.SDL.SDL_EventType;

namespace Sencae.Desktop.Sdl;

using static SdlAssertions;

public class SdlMainLoop : IDisposable {
    protected bool IsQuitting { get; set; }
    protected Dictionary<uint, SdlWindow> WindowIdToWindow { get; } = new();

    private readonly WindowsSizeMoveFreezeWorkaroundSupport? _freezeWorkaround;

    public SdlMainLoop() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            _freezeWorkaround = new WindowsSizeMoveFreezeWorkaroundSupport(this);
        }
    }

    public bool RegisterWindow(SdlWindow window) {
        Debug.Assert(!WindowIdToWindow.ContainsKey(window.WindowId), "!WindowIdToWindow.ContainsKey(window.WindowId)");
        return WindowIdToWindow.TryAdd(window.WindowId, window);
    }

    public bool UnregisterWindow(SdlWindow window) {
        Debug.Assert(WindowIdToWindow.ContainsKey(window.WindowId), "WindowIdToWindow.ContainsKey(window.WindowId)");
        return WindowIdToWindow.Remove(window.WindowId);
    }

    public void Run() {
        foreach (SdlWindow window in WindowIdToWindow.Values) {
            window.MainLoopEvent(MainLoopEvent.Starting);
        }

        while (!IsQuitting) {
            while (SDL_PollEvent(out SDL_Event evt) == 1) {
                HandleSdlEvent(in evt);
            }

            MainLoopIteration();
        }

        foreach (SdlWindow window in WindowIdToWindow.Values) {
            window.MainLoopEvent(MainLoopEvent.Ending);
        }
    }

    public void Quit() {
        IsQuitting = true;
    }

    protected virtual void HandleSdlEvent(in SDL_Event evt) {
        _freezeWorkaround?.HandleSdlEvent(evt);

        foreach (SdlWindow window in WindowIdToWindow.Values) {
            window.HandleWindowSdlEvent(evt);
        }
    }

    protected virtual void MainLoopIteration() {
        foreach (SdlWindow window in WindowIdToWindow.Values) {
            window.MainLoopEvent(MainLoopEvent.Iteration);
        }
    }

    public void Dispose() {
        IsQuitting = true;
        _freezeWorkaround?.Dispose();
    }

    private class WindowsSizeMoveFreezeWorkaroundSupport : IDisposable {
        private const uint SIZE_MOVE_TIMER_ID = 373;

        private readonly SdlMainLoop _sdlMainLoop;
        private readonly SDL_EventFilter _eventFilterDelegate;
        private IntPtr _sizeMoveTimer = IntPtr.Zero;

        public WindowsSizeMoveFreezeWorkaroundSupport(SdlMainLoop sdlMainLoop) {
            _sdlMainLoop = sdlMainLoop;
            _eventFilterDelegate = SdlEventFilter;

            SDL_AddEventWatch(_eventFilterDelegate, IntPtr.Zero);
        }

        public void HandleSdlEvent(in SDL_Event evt) {
            if (_sizeMoveTimer == IntPtr.Zero || evt.type != SDL_WINDOWEVENT)
                return;

            // As soon as we start getting messages,
            // we assume that drag/size loop has ended,
            // so the refresh timer can be killed

            // Jump through hoops to get the HWND 
            SDL_SysWMinfo systemInfo = default;
            SDL_VERSION(out systemInfo.version);
            IntPtr window = AssertSdlSuccess(SDL_GetWindowFromID(evt.window.windowID));
            AssertSdlSuccess(SDL_GetWindowWMInfo(window, ref systemInfo));
            IntPtr hwnd = systemInfo.info.win.window;

            bool timerKilled = WinApi.KillTimer(hwnd, (IntPtr) SIZE_MOVE_TIMER_ID);
            Debug.Assert(timerKilled, "killed");
            _sizeMoveTimer = IntPtr.Zero;
        }

        private unsafe int SdlEventFilter(IntPtr userData, IntPtr sdlEventPtr) {
            SDL_Event* sdlEvent = (SDL_Event*) sdlEventPtr;

            if (sdlEvent->syswm.type != SDL_SYSWMEVENT)
                return 0;

            SDL_SysWMmsg* sdlSysWMmsg = (SDL_SysWMmsg*) sdlEvent->syswm.msg;
            if (sdlSysWMmsg->subsystem != SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS)
                return 0;

            SDL_SysWMmsg.SDL_SysWMmsgInfoWin winMessage = sdlSysWMmsg->info.win;
            switch (winMessage) {
                case { msg: WinApi.WM_ENTERSIZEMOVE }:
                    _sizeMoveTimer = WinApi.SetTimer(winMessage.hwnd, (IntPtr) SIZE_MOVE_TIMER_ID, WinApi.USER_TIMER_MINIMUM, IntPtr.Zero);
                    break;
                case { msg: WinApi.WM_TIMER, wParam: SIZE_MOVE_TIMER_ID }:
                    _sdlMainLoop.MainLoopIteration();
                    Thread.Sleep(1);
                    break;
            }

            return 0;
        }

        public void Dispose() {
            SDL_DelEventWatch(_eventFilterDelegate, IntPtr.Zero);
        }

        #pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private struct SDL_SysWMmsg {
            public SDL_version version;
            public SDL_SYSWM_TYPE subsystem;
            public SDL_SysWMmsgInfoUnion info;

            [StructLayout(LayoutKind.Explicit)]
            public struct SDL_SysWMmsgInfoUnion {
                [FieldOffset(0)]
                public SDL_SysWMmsgInfoWin win;
            }

            public struct SDL_SysWMmsgInfoWin {
                public IntPtr hwnd;

                /**< The window for the message */
                public uint msg;

                /**< The type of message */
                public UIntPtr wParam;

                /**< WORD message parameter */
                public IntPtr lParam; /**< LONG message parameter */
            }
        }
        #pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private static class WinApi {
            public const uint WM_ENTERSIZEMOVE = 0x0231;
            public const uint WM_TIMER = 0x0113;
            public const uint USER_TIMER_MINIMUM = 0x0000000A;

            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern IntPtr SetTimer(IntPtr hWnd, IntPtr nIDEvent, uint uElapse, IntPtr lpTimerFunc);

            [DllImport("user32.dll", ExactSpelling = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool KillTimer(IntPtr hWnd, IntPtr uIDEvent);
        }
    }
}
