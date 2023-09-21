using System.Runtime.CompilerServices;
using ImGuiNET;

namespace Sencae.Desktop;

public static unsafe class ImGuiEx {
    public static bool InputScalar<T>(
        string label,
        ref T p_data,
        T? p_step,
        T? p_step_fast,
        string format,
        ImGuiInputTextFlags flags
    ) where T : unmanaged {
        return ImGui.InputScalar(
            label,
            TypeToImGuiDataType<T>(),
            (IntPtr) Unsafe.AsPointer(ref p_data),
            p_step != null ? (IntPtr) (&p_step) : IntPtr.Zero,
            p_step_fast != null ? (IntPtr) (&p_step_fast) : IntPtr.Zero,
            format,
            flags
        );
    }
    
    public static bool SliderScalar<T>(
        string label,
        ref T p_data,
        T p_min,
        T p_max,
        string format,
        ImGuiSliderFlags flags
    ) where T : unmanaged {
        return ImGui.SliderScalar(
            label,
            TypeToImGuiDataType<T>(),
            (IntPtr) Unsafe.AsPointer(ref p_data),
            (IntPtr) (&p_min),
            (IntPtr) (&p_max),
            format,
            flags
        );
    }

    private static ImGuiDataType TypeToImGuiDataType<T>() where T : unmanaged =>
        Type.GetTypeCode(typeof(T)) switch {
            TypeCode.SByte => ImGuiDataType.S8,
            TypeCode.Byte => ImGuiDataType.U8,
            TypeCode.Int16 => ImGuiDataType.S16,
            TypeCode.UInt16 => ImGuiDataType.U16,
            TypeCode.Int32 => ImGuiDataType.S32,
            TypeCode.UInt32 => ImGuiDataType.U32,
            TypeCode.Int64 => ImGuiDataType.S64,
            TypeCode.UInt64 => ImGuiDataType.U64,
            TypeCode.Single => ImGuiDataType.Float,
            TypeCode.Double => ImGuiDataType.Double,
            _ => throw new ArgumentOutOfRangeException(nameof(T), Type.GetTypeCode(typeof(T)), null)
        };
}
