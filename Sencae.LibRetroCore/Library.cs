using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Libretro.NET.Bindings;

namespace Sencae.LibRetroCore;

public static unsafe class Library {
    private static readonly Type[] CdeclConv = { typeof(CallConvCdecl) };

    private static readonly sbyte* LibraryName = (sbyte*) Marshal.StringToHGlobalAnsi("Sencae");
    private static readonly sbyte* LibraryVersion = (sbyte*) Marshal.StringToHGlobalAnsi("0.1.0");
    private static readonly sbyte* ValidExtensions = (sbyte*) Marshal.StringToHGlobalAnsi("nes");

    private static retro_environment_t EnvironmentCallback;
    private static retro_log_printf_t? LogCallback;
    private static retro_audio_sample_batch_t AudioSampleBatchCallback;
    private static retro_video_refresh_t VideoRefreshCallback;
    private static retro_audio_sample_t AudioSampleCallback;
    private static retro_input_poll_t InputPollCallback;
    private static retro_input_state_t InputStateCallback;

    private static uint[] VideoBuffer = new uint[NesConstants.ScreenWidth * NesConstants.ScreenHeight];

    //[DllExport("Init", CallingConvention.Cdecl)]
    [UnmanagedCallersOnly(EntryPoint = "retro_api_version", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static nuint ApiVersion() {
        LogEntrypoint();
        return RetroBindings.RETRO_API_VERSION;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_init", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void Init() {
        LogEntrypoint();

        retro_log_callback logCallback;
        EnvironmentCallback(RetroBindings.RETRO_ENVIRONMENT_GET_LOG_INTERFACE, &logCallback);
        if (logCallback.log != IntPtr.Zero) {
            LogCallback = Marshal.GetDelegateForFunctionPointer<retro_log_printf_t>(logCallback.log);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_deinit", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void Deinit() {
        LogEntrypoint();
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_get_system_info", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void GetSystemInfo(retro_system_info* info) {
        LogEntrypoint();

        info->library_name = LibraryName;
        info->library_version = LibraryVersion;
        info->valid_extensions = ValidExtensions;
        info->need_fullpath = false;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_get_system_av_info", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void GetSystemAVInfo(retro_system_av_info* info) {
        LogEntrypoint();

        info->timing.fps = NesConstants.Framerate;
        info->timing.sample_rate = 44100; // 44.1kHz
        info->geometry.base_width = NesConstants.ScreenWidth;
        info->geometry.base_height = NesConstants.ScreenHeight;
        info->geometry.max_width = NesConstants.ScreenWidth;
        info->geometry.max_height = NesConstants.ScreenHeight;
        info->geometry.aspect_ratio = NesConstants.ScreenWidth / (float) NesConstants.ScreenHeight;

        retro_pixel_format pixelFormat = retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888;
        EnvironmentCallback(RetroBindings.RETRO_ENVIRONMENT_SET_PIXEL_FORMAT, &pixelFormat);
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_set_environment", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void RetroSetEnvironment(IntPtr callback) {
        LogEntrypoint();

        EnvironmentCallback = Marshal.GetDelegateForFunctionPointer<retro_environment_t>(callback);

        bool value = true;
        EnvironmentCallback(RetroBindings.RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME, &value);
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_set_audio_sample_batch", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void SetAudioSampleBatch(IntPtr callback) {
        AudioSampleBatchCallback = Marshal.GetDelegateForFunctionPointer<retro_audio_sample_batch_t>(callback);
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_set_video_refresh", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void SetVideoRefresh(IntPtr callback) {
        VideoRefreshCallback = Marshal.GetDelegateForFunctionPointer<retro_video_refresh_t>(callback);
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_set_audio_sample", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void SetAudioSample(IntPtr callback) {
        AudioSampleCallback = Marshal.GetDelegateForFunctionPointer<retro_audio_sample_t>(callback);
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_set_input_poll", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void SetInputPoll(IntPtr callback) {
        InputPollCallback = Marshal.GetDelegateForFunctionPointer<retro_input_poll_t>(callback);
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_set_input_state", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void SetInputState(IntPtr callback) {
        InputStateCallback = Marshal.GetDelegateForFunctionPointer<retro_input_state_t>(callback);
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_load_game", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static bool LoadGame(retro_game_info* infoPtr) {
        LogEntrypoint();

        return true;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_load_game_special", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static bool LoadGameSpecial(nuint num_info, nuint game_type, retro_game_info* info) {
        return false;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_unload_game", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void UnloadGame() {
        LogEntrypoint();
    }

    // Single frame of emulation
    [UnmanagedCallersOnly(EntryPoint = "retro_run", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void Run() {
        LogEntrypoint();

        bool fillRed = Random.Shared.NextDouble() > 0.5;
        for (int x = 0; x < NesConstants.ScreenWidth; x++) {
            for (int y = 0; y < NesConstants.ScreenHeight; y++) {
                VideoBuffer[y * NesConstants.ScreenWidth + x] =
                    fillRed ? 0xFFFF0000 : (uint) Random.Shared.Next();
            }
        }

        fixed (uint* videoBuffer = VideoBuffer) {
            VideoRefreshCallback(
                videoBuffer,
                NesConstants.ScreenWidth,
                NesConstants.ScreenHeight,
                sizeof(uint) * NesConstants.ScreenWidth
            );
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_reset", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void Reset() {
        LogEntrypoint();
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_get_region", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static nuint GetRegion() {
        return RetroBindings.RETRO_REGION_PAL;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_set_controller_port_device", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void SetControllerPortDevice(nuint device, nuint port) {
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_get_memory_data", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void* GetMemoryData(nuint id) {
        return null;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_get_memory_size", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static nint GetMemorySize(nuint id) {
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_serialize_size", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static nint SerializeSize() {
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_serialize", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static bool Serialize(nint size, void* data) {
        return false;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_unserialize", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static bool Unserialize(nint size, void* data) {
        return false;
    }

    // Cheats
    [UnmanagedCallersOnly(EntryPoint = "retro_cheat_reset", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void CheatReset() {
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_cheat_set", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void CheatSet(char* code, nuint index, bool enabled) {
    }

    private static void Log(string message) {
        message = "[Sencae] " + message;

        if (LogCallback != null) {
            IntPtr stringPtr = Marshal.StringToHGlobalAnsi(message + "\r\n");

            try {
                LogCallback(retro_log_level.RETRO_LOG_DEBUG, (sbyte*) stringPtr);
            } finally {
                Marshal.FreeHGlobal(stringPtr);
            }
        } else {
            Console.WriteLine(message);
        }
    }

    private static void LogEntrypoint([CallerMemberName] string? caller = null) {
        if (caller == null)
            return;

        Log($"Entrypoint: {caller}");
    }
}
