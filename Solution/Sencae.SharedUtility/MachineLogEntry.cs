using System.Runtime.InteropServices;

namespace Sencae.SharedUtility;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct MachineLogEntry(
    MachineLogEntry.CpuState Cpu,
    MachineLogEntry.PpuState Ppu
) {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly record struct CpuState(
        ushort AddressBus,
        byte A,
        byte X,
        byte Y,
        CpuStatusRegisterPackedFlags P,
        byte SP,
        InstructionDescriptor Instruction,
        UInt16Bytes OpcodeData,
        ulong CpuCycles,
        byte TargetData,
        ushort EffectiveAddress,
        ushort EffectiveAddressAddress,
        ushort EffectiveAddressIndexed
    );

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly record struct PpuState(
        short Scanline,
        short ScanlineCycle
    );
}
