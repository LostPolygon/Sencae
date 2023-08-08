namespace NGChip8Emulator;

public enum AddressMode : byte {
    Invalid, 
    
    Implied,
    Immediate,

    Absolute,
    AbsoluteXIndexed,
    AbsoluteYIndexed,

    ZeroPage,
    ZeroPageXIndexed,
    ZeroPageYIndexed,

    AbsoluteIndirect,
    ZeroPageIndirectXPreIndexed,
    ZeroPageIndirectYPostIndexed,

    Relative,
}
