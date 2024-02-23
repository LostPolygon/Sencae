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
        byte InstructionOpcode,
        UInt16Bytes OpcodeData,
        ulong Cycles,
        byte TargetData,
        ushort EffectiveAddress,
        ushort EffectiveAddressAddress,
        ushort EffectiveAddressIndexed
    ) {
        public InstructionDescriptor Instruction => Instructions.DescriptorByOpcode[InstructionOpcode];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly record struct PpuState(
        ulong Cycles,
        short Scanline,
        short ScanlineCycle
    );
}
