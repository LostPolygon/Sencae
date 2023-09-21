using NUnit.Framework;

namespace Sencae.Tests;

public class CartridgeTests {
    [Test]
    public void Bus() {
        byte[] programRom = new byte[16 * 1024];

        programRom[^4] = 0x69;
        programRom[^3] = 0x42;

        byte[] characterRom = new byte[8 * 1024];

        CartridgeDevice cartridgeDevice = CartridgeDeviceFactory.Create(new CartridgeData(0, programRom, characterRom, true));
        Machine machine = new(cartridgeDevice);

        Assert.AreEqual(0x69, machine.CpuMemoryBus.Read(CpuConstants.ResetVector));
        Assert.AreEqual(0x42, machine.CpuMemoryBus.Read(CpuConstants.ResetVector + 1));
    }
}
