using System.Diagnostics;

namespace Sencae;

public static partial class Instructions {
    public static readonly InstructionDescriptor[] DescriptorByOpcode;
    public static readonly InstructionDescriptorSlim[] DescriptorSlimByOpcode;
    public static readonly AddressingMode[] AddressModeByOpcode;
    public static readonly InstructionType[] InstructionTypeByOpcode;

    static Instructions() {
        Debug.Assert(Map.Count == 256);
        DescriptorByOpcode = new InstructionDescriptor[Map.Count];
        DescriptorSlimByOpcode = new InstructionDescriptorSlim[Map.Count];
        AddressModeByOpcode = new AddressingMode[Map.Count];
        InstructionTypeByOpcode = new InstructionType[Map.Count];

        foreach ((byte opcode, InstructionDescriptor descriptor) in Map) {
            DescriptorByOpcode[opcode] = descriptor;
            AddressModeByOpcode[opcode] = descriptor.AddressingMode;
            InstructionTypeByOpcode[opcode] = descriptor.Type;

            DescriptorSlimByOpcode[opcode] = new InstructionDescriptorSlim(HandlerMap[opcode], descriptor);
        }
    }
}
