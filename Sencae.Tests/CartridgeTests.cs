using NUnit.Framework;

namespace Sencae.Tests; 

public class CartridgeTests {
    [Test]
    public void Bus() {
        byte[] programRom = new byte[16 * 1024];
        
        programRom[^4] = 0x69;
        programRom[^3] = 0x42;

        byte[] characterRom = new byte[8 * 1024];

        CartridgeDevice cartridgeDevice = new CartridgeDevice(0, programRom, characterRom);
        Machine machine = new Machine(cartridgeDevice);
        
        Assert.AreEqual(0x69, machine.CpuMemoryBus.Read(0xFFFC));
        Assert.AreEqual(0x42, machine.CpuMemoryBus.Read(0xFFFD));
    }
}
