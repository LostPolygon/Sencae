using FastEnumToString;

namespace Sencae;

[ToString(ToStringDefault.First)]
public enum InstructionType : byte {
    // Transfer Instructions

    LDA, // load accumulator
    LDX, // load X
    LDY, // load Y
    STA, // store accumulator
    STX, // store X
    STY, // store Y
    TAX, // transfer accumulator to X
    TAY, // transfer accumulator to Y
    TSX, // transfer stack pointer to X
    TXA, // transfer X to accumulator
    TXS, // transfer X to stack pointer
    TYA, // transfer Y to accumulator

    // Stack Instructions

    PHA, // push accumulator
    PHP, // push processor status register (with break flag set)
    PLA, // pull accumulator
    PLP, // pull processor status register 

    // Decrements & Increments

    DEC, // decrement (memory)
    DEX, // decrement X
    DEY, // decrement Y
    INC, // increment (memory)
    INX, // increment X
    INY, // increment Y

    // Arithmetic Operations

    ADC, // add with carry (prepare by CLC)
    SBC, // subtract with carry (prepare by SEC)

    // Logical Operations

    AND, // and (with accumulator)
    EOR, // exclusive or (with accumulator)
    ORA, // (inclusive) or with accumulator

    // Shift & Rotate Instructions

    ASL, // arithmetic shift left (shifts in a zero bit on the right)
    LSR, // logical shift right (shifts in a zero bit on the left)
    ROL, // rotate left (shifts in carry bit on the right)
    ROR, // rotate right (shifts in zero bit on the left)

    // Flag Instructions

    CLC, // clear carry
    CLD, // clear decimal (BCD arithmetics disabled)
    CLI, // clear interrupt disable
    CLV, // clear overflow
    SEC, // set carry
    SED, // set decimal (BCD arithmetics enabled)
    SEI, // set interrupt disable

    // Comparisons
    CMP, // compare (with accumulator)
    CPX, // compare with X
    CPY, // compare with Y 

    // Conditional Branch Instructions

    BCS, // branch on carry set
    BCC, // branch on carry clear
    BEQ, // branch on equal (zero set)
    BNE, // branch on not equal (zero clear)
    BMI, // branch on minus (negative set)
    BPL, // branch on plus (negative clear)
    BVS, // branch on overflow set 
    BVC, // branch on overflow clear

    // Jumps & Subroutines

    JMP, // jump
    JSR, // jump subroutine
    RTS, // return from subroutine 

    // Interrupts

    BRK, // break / software interrupt
    //SpecialNmiHandler,
    RTI, // return from interrupt 

    // Other

    BIT, // bit test (accumulator & memory)
    NOP, // no operation
    
    // Unofficial
    
    STP,
    SLO,
    ANC,
    RLA,
    SRE,
    ALR,
    RRA,
    SHY,
    SHX,
    ARR,
    SAX,
    XAA,
    AHX,
    TAS,
    LAX,
    LAS,
    DCP,
    AXS,
    ISC
}
