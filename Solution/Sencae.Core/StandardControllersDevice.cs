using System;

namespace Sencae;

public class StandardControllersDevice : IBusDevice {
    private int _controller1Shift;
    private int _controller2Shift;
    private StandardControllerInput _controller1Input;
    private StandardControllerInput _controller1InputLatch;
    private StandardControllerInput _controller2Input;

    public void SetController1Input(StandardControllerInput input) {
        _controller1Input = input;
    }

    public bool HandleBus(ushort address, MemoryAccessMode accessMode, ref byte bus) {
        //return false; // disabled for self-discipline
        switch (address) {
            case 0x4016:
                switch (accessMode) {
                    case MemoryAccessMode.Read:
                        bus = (byte) ((byte) (_controller1InputLatch & (StandardControllerInput) (1 << _controller1Shift)) >> _controller1Shift);
                        _controller1Shift++;
                        break;
                    case MemoryAccessMode.Write:
                        if ((bus & 0b0000_0001) != 0) {
                            _controller1Shift = _controller2Shift = 0;
                            _controller1InputLatch = _controller1Input;
                        }

                        break;
                    case MemoryAccessMode.ReadObserve:
                        bus = (byte) _controller1InputLatch;
                        break;
                }

                return true;
                break;

            //case 0x4017:
            //    break;
        }

        return false;
    }
}
