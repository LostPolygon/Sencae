using NUnit.Framework;

namespace Sencae.Tests;

public class CartridgeTests {
    [Test]
    public void Bus() {
        byte[] programRom = new byte[16 * 1024];

        programRom[^4] = 0x69;
        programRom[^3] = 0x42;

        byte[] characterRom = new byte[8 * 1024];

        MapperDevice mapperDevice = MapperDeviceFactory.Create(new CartridgeData(0, programRom, null, characterRom));
        using Machine machine = new(mapperDevice);

        Assert.AreEqual(0x69, machine.CpuMemoryBus.Read(CpuConstants.ResetVector));
        Assert.AreEqual(0x42, machine.CpuMemoryBus.Read(CpuConstants.ResetVector + 1));
    }
}
