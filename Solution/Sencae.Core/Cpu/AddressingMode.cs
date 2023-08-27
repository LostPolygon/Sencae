namespace Sencae;

public enum AddressingMode : byte {
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
