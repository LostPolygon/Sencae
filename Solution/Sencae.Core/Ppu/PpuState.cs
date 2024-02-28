using System.Runtime.CompilerServices;

namespace Sencae.Ppu;

public sealed class PpuState {
    public byte[] Memory = new byte[2048];
    public byte[] PaletteMemory = new byte[32];
    public byte[] OamMemory = new byte[256];

    public PpuControlRegisterFlags PPUCTRL;
    public PpuMaskRegisterFlags PPUMASK;
    public PpuStatusRegisterFlags PPUSTATUS;

    public byte OAMADDR;

    public PpuInternalState Internal;

    public bool IsRenderingDisabled {
        [MethodImpl(AggressiveInlining)]
        get => (PPUMASK & (PpuMaskRegisterFlags.ShowBackground | PpuMaskRegisterFlags.ShowSprites)) == 0;
    }

    /*
    PPUCTRL 	$2000 	VPHB SINN 	NMI enable (V), PPU master/slave (P), sprite height (H), background tile select (B), sprite tile select (S), increment mode (I), nametable select (NN)
    PPUMASK 	$2001 	BGRs bMmG 	color emphasis (BGR), sprite enable (s), background enable (b), sprite left column enable (M), background left column enable (m), greyscale (G)
    PPUSTATUS 	$2002 	VSO- ---- 	vblank (V), sprite 0 hit (S), sprite overflow (O); read resets write pair for $2005/$2006
    OAMADDR 	$2003 	aaaa aaaa 	OAM read/write address
    OAMDATA 	$2004 	dddd dddd 	OAM data read/write
    PPUSCROLL 	$2005 	xxxx xxxx 	fine scroll position (two writes: X scroll, Y scroll)
    PPUADDR 	$2006 	aaaa aaaa 	PPU read/write address (two writes: most significant byte, least significant byte)
    PPUDATA 	$2007 	dddd dddd 	PPU data read/write
    */

    public struct PpuInternalState {
        public UInt16Bytes Address;
        public byte AddressHighTemp;
        public bool PpuAddressRegisterLatch; // false: no writes yet, true: 1 write was done

        public bool PpuScrollRegisterLatch; // false: no writes yet, true: 1 write was done

        public byte FineScrollX;
        public byte FineScrollY;

        public byte DataBus;

        public byte DataBusLatch;

        //public bool BusLatchToggle;

        public int Scanline;
        public int CyclesInScanline;

        //public bool IsOddFrame;
        public int NextFrameLastScanlineLastCycle;
        
        public ulong Frames;
        public ulong LastPPUSTATUS_ReadCycle;

        public bool Reset;
        
        public ulong Cycles;
    }
}
