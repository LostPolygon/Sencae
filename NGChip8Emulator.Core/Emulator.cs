using System;
using System.Runtime.CompilerServices;

namespace NGChip8Emulator {
    public unsafe class Emulator {
        private readonly Machine _machine;

        public Emulator(Machine machine) {
            _machine = machine;
        }

        public Machine Machine => _machine;

        public ref MachineState GetState() {
            return ref _machine.State;
        }

        public virtual void Reset() {
            _machine.Reset();
        }

        public void LoadProgram(byte[] program) {
            //program.AsSpan().CopyTo(GetProgramMemorySpan());
        }

        public virtual void UpdateTimers() {
        }

        public void Update(int instructionCount) {
            /*bool mustDrawAccumulative = false;
            for (int i = 0; i < instructionCount; i++) {
                EmulateCycle(, out bool mustDraw);
                mustDrawAccumulative |= mustDraw;
                if (machineState.WaitingForAnyKey)
                    break;
            }

            machineState.ExecutedInstructionCount += (ulong) instructionCount;

            if (mustDrawAccumulative) {
                Render();
            }*/
        }

        public virtual void Clock() {
            ref CpuState cpu = ref _machine.State.Cpu;

            if (!cpu.Internal.Read) {
                _machine.CpuBus.WriteU8(cpu.Internal.AddressBus.Value, cpu.Internal.DataBus);
                //cpu.PC.Value++;
                //if (cpu.Internal.State == CpuInternalState.ExecutionState.FinishingOldInstruction) {
                //cpu.Internal.Read = true;
                //}
            } else {
                cpu.Internal.DataLatch = cpu.Internal.DataBus;
                cpu.Internal.DataBus = _machine.CpuBus.GetU8(cpu.Internal.AddressBus.Value);

            }

            /*if (cpu.Internal.State == CpuInternalState.ExecutionState.FinishingOldInstruction) {
                ExecuteState();
            }*/

            ExecuteState();
            


            _machine.State.Cycle++;
        }

        private void ExecuteState() {
            ref CpuState cpu = ref _machine.State.Cpu;

            switch (cpu.Internal.State) {
                case CpuInternalState.ExecutionState.FetchNextInstructionOpcode: {
                    InstructionDefinition instructionDefinition = InstructionDefinitions.Data[cpu.Internal.IR];
                    bool completed = InstructionCycle(instructionDefinition, true);
                    
                    cpu.Internal.Read = true;
                    //cpu.Internal.OldIR = cpu.Internal.IR;
                    //cpu.Internal.IR = cpu.Internal.DataBus;
                    cpu.PC.Value++;
                    cpu.Internal.AddressBus = cpu.PC;
                    
                    cpu.Internal.State = CpuInternalState.ExecutionState.InterpretOpcode;

                    break;
                }
                case CpuInternalState.ExecutionState.InterpretOpcode: {
                    cpu.Internal.OldIR = cpu.Internal.IR;
                    cpu.Internal.IR = cpu.Internal.DataBus;
                    //cpu.PC.Value++;
                    //cpu.Internal.AddressBus = cpu.PC;
                    
                    cpu.Internal.State = CpuInternalState.ExecutionState.ExecutingAddressing;
                    cpu.Internal.OperationCycle = 0;
                    ExecuteState();
                    break;
                } 
                case CpuInternalState.ExecutionState.ExecutingAddressing: {
                    InstructionDefinition instructionDefinition = InstructionDefinitions.Data[cpu.Internal.IR];
                    bool completed =
                        (instructionDefinition.Flags & InstructionFlags.CustomAddressing) != 0 ||
                        LoadEffectiveAddress(instructionDefinition.Type, instructionDefinition.AddressMode);
                    if (completed) {
                        cpu.Internal.OperationCycle = -1;
                        cpu.Internal.State = CpuInternalState.ExecutionState.ExecutingInstruction;

                        ExecuteState();
                    }

                    break;
                }
                case CpuInternalState.ExecutionState.ExecutingInstruction: {
                    InstructionDefinition instructionDefinition = InstructionDefinitions.Data[cpu.Internal.IR];
                    bool completed = InstructionCycle(instructionDefinition, false);
                    if (completed) {
                        cpu.Internal.State = CpuInternalState.ExecutionState.FetchNextInstructionOpcode;
                        cpu.Internal.OperationCycle = 0;

                        //cpu.Internal.AddressBus = cpu.PC;
                        //  ExecuteState();
                    }

                    break;
                }
                /*case CpuInternalState.ExecutionState.FinishingOldInstruction: {
                    InstructionDefinition instructionDefinition = InstructionDefinitions.Data[cpu.Internal.OldIR];
                    InstructionCycle(instructionDefinition, true);
                    //cpu.Internal.OldIR = cpu.Internal.IR;
                    //cpu.Internal.IR = cpu.Internal.DataBus;
                    cpu.Internal.State = CpuInternalState.ExecutionState.FetchNextInstructionOpcode;
                    cpu.Internal.OperationCycle = 0;
                    //cpu.Internal.AddressBus = cpu.PC;

                    if (_machine.State.Cycle != 0) {
                        //cpu.PC.Value++;
                        //cpu.Internal.AddressBus = cpu.PC;
                    }

                    //ExecuteState();

                    break;
                }*/
            }
        }

        protected virtual bool InstructionCycle(
            in InstructionDefinition instructionDefinition,
            bool finishing
        ) {
            ref CpuState cpu = ref _machine.State.Cpu;
            int operationCycle = finishing ? 100 : cpu.Internal.OperationCycle;

            bool completed = false;
            switch (instructionDefinition.Type) {
                case InstructionType.BRK:
                    if (_machine.State.Cycle > 3) {
                        cpu.Halted = true;
                    }

                    completed = true;
                    break;
                case InstructionType.STA:
                    switch (operationCycle) {
                        case -1:
                            cpu.Internal.Read = false;
                            cpu.Internal.DataBus = cpu.A;
                            //cpu.Internal.AddressBus = cpu.PC;

                            //cpu.PC.Value++;
                            completed = true;
                            break;
                        case 100:
                            cpu.Internal.Read = true;
                            cpu.Internal.AddressBus = cpu.PC;
                            break;
                    }

                    break;
                case InstructionType.LDA:
                    switch (operationCycle) {
                        case -1:
                            cpu.Internal.Read = true;
                            //cpu.PC.Value++;
                            //cpu.Internal.AddressBus = cpu.PC;
                            //cpu.A = cpu.Internal.DataBus;
                            completed = true;
                            break;
                        case 100:
                            cpu.A = cpu.Internal.DataBus;
                            //completed = true;
                            break;
                    }

                    break;
                case InstructionType.JSR:
                    ushort stackAddress = (ushort) (0x0100 + cpu.SP);
                    switch (operationCycle) {
                        case 0:
                            cpu.Internal.Read = true;
                            cpu.Internal.T = cpu.Internal.DataBus;
                            cpu.Internal.AddressBus.Value = stackAddress;
                            cpu.PC.Value++;
                            break;
                        case 1:
                            cpu.Internal.Read = true;
                            cpu.Internal.AddressBus.Value = stackAddress;
                            break;
                        case 2:
                            cpu.Internal.Read = false;
                            cpu.Internal.DataBus = cpu.PC.High;
                            cpu.SP--;
                            break;
                        case 3:
                            cpu.Internal.Read = false;
                            cpu.Internal.AddressBus.Value = stackAddress;
                            cpu.Internal.DataBus = cpu.PC.Low;

                            //cpu.PC.Value += 1;
                            cpu.SP--;
                            break;
                        case 4:
                            cpu.Internal.Read = true;
                            cpu.Internal.AddressBus = cpu.PC;

                            UInt16Bytes u = new() {
                                High = cpu.Internal.DataBus,
                                Low = (byte) cpu.Internal.T
                            };
                            cpu.PC = u;
                            cpu.Internal.AddressBus = cpu.PC;
                            completed = true;
                            break;
                    }

                    break;
                default:
                    completed = true;
                    break;
            }

            cpu.Internal.OperationCycle++;

            return completed;
        }

        private bool LoadEffectiveAddress(InstructionType instructionDefinitionType, AddressMode addressMode) {
            ref CpuState cpu = ref _machine.State.Cpu;

            bool completed = false;
            switch (addressMode) {
                case AddressMode.Implied:
                    completed = true;
                    break;
                case AddressMode.Immediate:
                    switch (cpu.Internal.OperationCycle) {
                        case 0:
                            cpu.PC.Value++;
                            cpu.Internal.AddressBus = cpu.PC;
                            completed = true;
                            break;
                    }

                    break;
                case AddressMode.Absolute:
                    switch (cpu.Internal.OperationCycle) {
                        case 0:
                            //cpu.Internal.AddressBus.Low = cpu.Internal.DataBus;
                            cpu.PC.Value++;
                            cpu.Internal.AddressBus = cpu.PC;
                            break;
                        case 1:
                            cpu.Internal.T = cpu.Internal.DataBus;
                            cpu.PC.Value++;
                            cpu.Internal.AddressBus = cpu.PC;
                            break;
                        case 2:
                            cpu.Internal.AddressBus.Low = (byte) cpu.Internal.T;
                            cpu.Internal.AddressBus.High = cpu.Internal.DataBus;
                            //cpu.PC.Value++;
                            completed = true;
                            break;
                    }

                    if (instructionDefinitionType == InstructionType.JSR) {
                    } else {
                    }

                    break;
                /*case AddressMode.AbsoluteXIndexed:
                    operandAddress = ReadU16AtPCAndAdvancePC() + cpu.X;
                    break;
                case AddressMode.AbsoluteYIndexed:
                    operandAddress = ReadU16AtPCAndAdvancePC() + cpu.Y;
                    break;
                case AddressMode.ZeroPage:
                    operandAddress = ReadU8AtPCAndAdvancePC();
                    break;
                case AddressMode.ZeroPageXIndexed:
                    operandAddress = ReadU8AtPCAndAdvancePC() + cpu.X;
                    break;
                case AddressMode.ZeroPageYIndexed:
                    operandAddress = ReadU8AtPCAndAdvancePC() + cpu.Y;
                    break;
                case AddressMode.AbsoluteIndirect:
                    operandAddress = _machine.CpuBus.GetU16(ReadU16AtPCAndAdvancePC());
                    break;
                case AddressMode.ZeroPageIndirectXPreIndexed:
                    operandAddress = _machine.CpuBus.GetU8(_machine.CpuBus.GetU8((ushort) (cpu.PC + cpu.X)));
                    cpu.PC += 1;
                    break;
                case AddressMode.ZeroPageIndirectYPostIndexed:
                    operandAddress = _machine.CpuBus.GetU8((ushort) (_machine.CpuBus.GetU8(cpu.PC) + cpu.Y));
                    cpu.PC += 1;
                    break;
                case AddressMode.Relative:
                    sbyte offset = (sbyte) ReadU8AtPCAndAdvancePC();
                    operandAddress = cpu.PC + offset;
                    break;*/
            }

            cpu.Internal.OperationCycle++;

            return completed;
        }

        protected virtual void Render() {
        }

        protected virtual void DoSound() {
        }

        /*private ref struct InstructionExecutionResult {
            public bool MustDraw;
            public bool WaitingForAnyKey;
        }*/
    }
}
