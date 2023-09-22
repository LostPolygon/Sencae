using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Libretro.NET.Bindings;
using static Libretro.NET.Bindings.RetroBindings;

namespace Sencae.LibRetroCore;

public static unsafe class Library {
    private static readonly sbyte* LibraryName = (sbyte*) Marshal.StringToHGlobalAnsi("Sencae");
    private static readonly sbyte* LibraryVersion = (sbyte*) Marshal.StringToHGlobalAnsi("0.1.0");
    private static readonly sbyte* ValidExtensions = (sbyte*) Marshal.StringToHGlobalAnsi("nes");

    private static retro_environment_t EnvironmentCallback = null!;
    private static retro_log_printf_t LogCallback = null!;
    private static retro_audio_sample_batch_t AudioSampleBatchCallback = null!;
    private static retro_video_refresh_t VideoRefreshCallback = null!;
    private static retro_audio_sample_t AudioSampleCallback = null!;
    private static retro_input_poll_t InputPollCallback = null!;
    private static retro_input_state_t InputStateCallback = null!;

    private static Machine? _machine;
    private static uint[] VideoBuffer = new uint[NesConstants.Ntsc.ScreenWidth * NesConstants.Ntsc.ScreenHeight];

    [UnmanagedCallersOnly(EntryPoint = "retro_api_version", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static nuint ApiVersion() {
        LogEntrypoint();
        return RETRO_API_VERSION;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_init", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void Init() {
        LogEntrypoint();

        retro_log_callback logCallback;
        EnvironmentCallback(RETRO_ENVIRONMENT_GET_LOG_INTERFACE, &logCallback);
        if (logCallback.log != IntPtr.Zero) {
            LogCallback = Marshal.GetDelegateForFunctionPointer<retro_log_printf_t>(logCallback.log);
        }

        sbyte* buttonLeft = (sbyte*) Marshal.StringToHGlobalAnsi("Left!");
        sbyte* buttonRight = (sbyte*) Marshal.StringToHGlobalAnsi("Right!");
        sbyte* buttonUp = (sbyte*) Marshal.StringToHGlobalAnsi("Up!");
        sbyte* buttonDown = (sbyte*) Marshal.StringToHGlobalAnsi("Down!");

        retro_input_descriptor* inputDescriptors = stackalloc retro_input_descriptor[] {
            new() { id = 0, index = 0, device = RETRO_DEVICE_JOYPAD, port = RETRO_DEVICE_ID_JOYPAD_LEFT, description = buttonLeft },
            new() { id = 0, index = 0, device = RETRO_DEVICE_JOYPAD, port = RETRO_DEVICE_ID_JOYPAD_RIGHT, description = buttonRight },
            new() { id = 0, index = 0, device = RETRO_DEVICE_JOYPAD, port = RETRO_DEVICE_ID_JOYPAD_UP, description = buttonUp },
            new() { id = 0, index = 0, device = RETRO_DEVICE_JOYPAD, port = RETRO_DEVICE_ID_JOYPAD_DOWN, description = buttonDown },
            default
        };

        EnvironmentCallback(RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS, &inputDescriptors);

        // Initialize the emulator
        Machine machine = new(CartridgeDeviceFactory.Create(new CartridgeData(0, new byte[16 * 1024], new byte[8 * 1024], true)));

        byte[] gameRom = {
            0x20, 0x06, 0x06, 0x20, 0x38, 0x06, 0x20, 0x0d, 0x06, 0x20, 0x2a, 0x06, 0x60, 0xa9, 0x02, 0x85,
            0x02, 0xa9, 0x04, 0x85, 0x03, 0xa9, 0x11, 0x85, 0x10, 0xa9, 0x10, 0x85, 0x12, 0xa9, 0x0f, 0x85,
            0x14, 0xa9, 0x04, 0x85, 0x11, 0x85, 0x13, 0x85, 0x15, 0x60, 0xa5, 0xfe, 0x85, 0x00, 0xa5, 0xfe,
            0x29, 0x03, 0x18, 0x69, 0x02, 0x85, 0x01, 0x60, 0x20, 0x4d, 0x06, 0x20, 0x8d, 0x06, 0x20, 0xc3,
            0x06, 0x20, 0x19, 0x07, 0x20, 0x20, 0x07, 0x20, 0x2d, 0x07, 0x4c, 0x38, 0x06, 0xa5, 0xff, 0xc9,
            0x77, 0xf0, 0x0d, 0xc9, 0x64, 0xf0, 0x14, 0xc9, 0x73, 0xf0, 0x1b, 0xc9, 0x61, 0xf0, 0x22, 0x60,
            0xa9, 0x04, 0x24, 0x02, 0xd0, 0x26, 0xa9, 0x01, 0x85, 0x02, 0x60, 0xa9, 0x08, 0x24, 0x02, 0xd0,
            0x1b, 0xa9, 0x02, 0x85, 0x02, 0x60, 0xa9, 0x01, 0x24, 0x02, 0xd0, 0x10, 0xa9, 0x04, 0x85, 0x02,
            0x60, 0xa9, 0x02, 0x24, 0x02, 0xd0, 0x05, 0xa9, 0x08, 0x85, 0x02, 0x60, 0x60, 0x20, 0x94, 0x06,
            0x20, 0xa8, 0x06, 0x60, 0xa5, 0x00, 0xc5, 0x10, 0xd0, 0x0d, 0xa5, 0x01, 0xc5, 0x11, 0xd0, 0x07,
            0xe6, 0x03, 0xe6, 0x03, 0x20, 0x2a, 0x06, 0x60, 0xa2, 0x02, 0xb5, 0x10, 0xc5, 0x10, 0xd0, 0x06,
            0xb5, 0x11, 0xc5, 0x11, 0xf0, 0x09, 0xe8, 0xe8, 0xe4, 0x03, 0xf0, 0x06, 0x4c, 0xaa, 0x06, 0x4c,
            0x35, 0x07, 0x60, 0xa6, 0x03, 0xca, 0x8a, 0xb5, 0x10, 0x95, 0x12, 0xca, 0x10, 0xf9, 0xa5, 0x02,
            0x4a, 0xb0, 0x09, 0x4a, 0xb0, 0x19, 0x4a, 0xb0, 0x1f, 0x4a, 0xb0, 0x2f, 0xa5, 0x10, 0x38, 0xe9,
            0x20, 0x85, 0x10, 0x90, 0x01, 0x60, 0xc6, 0x11, 0xa9, 0x01, 0xc5, 0x11, 0xf0, 0x28, 0x60, 0xe6,
            0x10, 0xa9, 0x1f, 0x24, 0x10, 0xf0, 0x1f, 0x60, 0xa5, 0x10, 0x18, 0x69, 0x20, 0x85, 0x10, 0xb0,
            0x01, 0x60, 0xe6, 0x11, 0xa9, 0x06, 0xc5, 0x11, 0xf0, 0x0c, 0x60, 0xc6, 0x10, 0xa5, 0x10, 0x29,
            0x1f, 0xc9, 0x1f, 0xf0, 0x01, 0x60, 0x4c, 0x35, 0x07, 0xa0, 0x00, 0xa5, 0xfe, 0x91, 0x00, 0x60,
            0xa6, 0x03, 0xa9, 0x00, 0x81, 0x10, 0xa2, 0x00, 0xa9, 0x01, 0x81, 0x10, 0x60, 0xa2, 0x00, 0xea,
            0xea, 0xca, 0xd0, 0xfb, 0x60
        };

        for (int i = 0; i < gameRom.Length; i++) {
            machine.CpuMemoryBus.Write((ushort) (0x0600 + i), gameRom[i]);
        }

        _machine = machine;
        _machine.Reset(
            new CpuResetOptions {
                OverrideResetVector = 0x0600
            }
        );
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_deinit", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void Deinit() {
        LogEntrypoint();
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_get_system_info", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void GetSystemInfo([DNNE.C99Type("void*")] retro_system_info* info) {
        LogEntrypoint();

        info->library_name = LibraryName;
        info->library_version = LibraryVersion;
        info->valid_extensions = ValidExtensions;
        info->need_fullpath = false;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_get_system_av_info", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void GetSystemAVInfo([DNNE.C99Type("void*")] retro_system_av_info* info) {
        LogEntrypoint();

        info->timing.fps = NesConstants.Ntsc.Framerate;
        info->timing.sample_rate = 44100; // 44.1kHz
        info->geometry.base_width = (uint) NesConstants.Ntsc.ScreenWidth;
        info->geometry.base_height = (uint) NesConstants.Ntsc.ScreenHeight;
        info->geometry.max_width = (uint) NesConstants.Ntsc.ScreenWidth;
        info->geometry.max_height = (uint) NesConstants.Ntsc.ScreenHeight;
        info->geometry.aspect_ratio = NesConstants.Ntsc.ScreenWidth / (float) NesConstants.Ntsc.ScreenHeight;

        retro_pixel_format pixelFormat = retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888;
        EnvironmentCallback(RETRO_ENVIRONMENT_SET_PIXEL_FORMAT, &pixelFormat);
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_set_environment", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void RetroSetEnvironment(IntPtr callback) {
        LogEntrypoint();

        EnvironmentCallback = Marshal.GetDelegateForFunctionPointer<retro_environment_t>(callback);

        bool value = true;
        EnvironmentCallback(RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME, &value);
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
    public static int LoadGame([DNNE.C99Type("void*")] retro_game_info* infoPtr) {
        LogEntrypoint();

        return 1;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_load_game_special", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int LoadGameSpecial(uint num_info, nuint game_type, [DNNE.C99Type("void*")] retro_game_info* infoPtr) {
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_unload_game", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void UnloadGame() {
        LogEntrypoint();
    }

    // Single frame of emulation
    [UnmanagedCallersOnly(EntryPoint = "retro_run", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void Run() {
        //LogEntrypoint();
        if (_machine == null)
            return;

        InputPollCallback();

        if (InputStateCallback(0, RETRO_DEVICE_JOYPAD, 0, RETRO_DEVICE_ID_JOYPAD_LEFT) != 0) {
            _machine.CpuMemoryBus.Write(0xFF, 0x61);
        }

        if (InputStateCallback(0, RETRO_DEVICE_JOYPAD, 0, RETRO_DEVICE_ID_JOYPAD_RIGHT) != 0) {
            _machine.CpuMemoryBus.Write(0xFF, 0x64);
        }

        if (InputStateCallback(0, RETRO_DEVICE_JOYPAD, 0, RETRO_DEVICE_ID_JOYPAD_UP) != 0) {
            _machine.CpuMemoryBus.Write(0xFF, 0x77);
        }

        if (InputStateCallback(0, RETRO_DEVICE_JOYPAD, 0, RETRO_DEVICE_ID_JOYPAD_DOWN) != 0) {
            _machine.CpuMemoryBus.Write(0xFF, 0x73);
        }

        for (int i = 0; i < 100; i++) {
            //Console.WriteLine(_machine.State.Cycles);
            _machine.CpuMemoryBus.Write(0xFE, (byte) Random.Shared.Next());
            _machine.CpuClock();
        }

        Color[] paletteColors = new Color[256];
        for (int i = 0; i < 256; i++) {
            Color color = i switch {
                0 => Color.Black,
                1 => Color.White,
                2 or 9 => Color.Gray,
                3 or 10 => Color.Red,
                4 or 11 => Color.Green,
                5 or 12 => Color.Blue,
                6 or 13 => Color.Magenta,
                7 or 14 => Color.Yellow,
                _ => Color.Cyan
            };
            paletteColors[i] = color;
        }

        int screenLength = NesConstants.Ntsc.ScreenWidth * NesConstants.Ntsc.ScreenHeight;
        for (int i = 0; i < screenLength; i++) {
            VideoBuffer[i] = (uint) paletteColors[_machine.CpuMemoryBus.Read((ushort) (0x0200 + i))].ToArgb();

            //VideoBuffer[i] = (uint) (emulatorScreenMemory[i] != 0 ? 0xFFFF0000 : 0xFF00FF00);
        }

        fixed (uint* videoBuffer = VideoBuffer) {
            VideoRefreshCallback(
                videoBuffer,
                (uint) NesConstants.Ntsc.ScreenWidth,
                (uint) NesConstants.Ntsc.ScreenHeight,
                sizeof(uint) * (uint) NesConstants.Ntsc.ScreenWidth
            );
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_reset", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void Reset() {
        LogEntrypoint();
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_get_region", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static nuint GetRegion() {
        return RETRO_REGION_PAL;
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
    public static int Serialize(nint size, void* data) {
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_unserialize", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int Unserialize(nint size, void* data) {
        return 0;
    }

    // Cheats
    [UnmanagedCallersOnly(EntryPoint = "retro_cheat_reset", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void CheatReset() {
    }

    [UnmanagedCallersOnly(EntryPoint = "retro_cheat_set", CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void CheatSet(char* code, nuint index, int enabled) {
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
