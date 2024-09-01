using System.Runtime.InteropServices;
using System.Text;
using Sencae.Nes20Database;
using Sencae.Nes20Database.Binary;
using Sencae.Nes20File;

namespace Sencae.Nes20Db.Xml2BinConverter;

public unsafe class BinaryDatabaseWriter {
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;

    public BinaryDatabaseWriter(Stream stream) {
        _stream = stream;
        _writer = new BinaryWriter(stream, Encoding.UTF8, true);
    }

    public void Write(string databaseDate, Nes20DbDto.GameDtoWrapper[] games) {
        _writer.Write(Header.Magic);
        _writer.Write(Header.Version);
        WriteLengthPrefixedString(databaseDate);

        _writer.Write(games.Length);

        checked {
            foreach (Nes20DbDto.GameDtoWrapper game in games) {
                long startPosition = _stream.Position;

                //WriteLengthPrefixedString(game.Comment);

                GameRomMetadataElementPresence elementPresence =
                    (game.Value.ChrRom != null ? GameRomMetadataElementPresence.ChrRom : 0) |
                    (game.Value.Trainer != null ? GameRomMetadataElementPresence.Trainer : 0) |
                    (game.Value.MiscRom != null ? GameRomMetadataElementPresence.MiscRom : 0) |
                    (game.Value.PrgRam != null ? GameRomMetadataElementPresence.PrgRam : 0) |
                    (game.Value.ChrRam != null ? GameRomMetadataElementPresence.ChrRam : 0) |
                    (game.Value.ChrNvram != null ? GameRomMetadataElementPresence.ChrNvram : 0) |
                    (game.Value.PrgNvram != null ? GameRomMetadataElementPresence.PrgNvram : 0) |
                    (game.Value.VsSystem != null ? GameRomMetadataElementPresence.VsSystem : 0);

                _writer.Write((byte) elementPresence);

                void WriteData<TSrc, T>(TSrc src, Func<TSrc, T> valueFactory, GameRomMetadataElementPresence presenceMarker = 0) where T : unmanaged {
                    if (presenceMarker != 0 && (elementPresence & presenceMarker) == 0)
                        return;

                    T value = valueFactory(src);
                    Span<byte> valueBytes = MemoryMarshal.Cast<T, byte>(new Span<T>(ref value));
                    _writer.Write(valueBytes);
                }

                void WriteRomData(Nes20DbDto.RomDto romDto, GameRomMetadataElementPresence presenceMarker = 0) {
                    WriteData(romDto, src => new GenericRomMetadata(src.Size, CreateSha1Hash(src.Sha1)), presenceMarker);
                }

                void WriteRamSizeData(Nes20DbDto.RamSizeDto ramSizeDto, GameRomMetadataElementPresence presenceMarker = 0) {
                    WriteData(ramSizeDto, src => new RamSizeMetadata(src.Size), presenceMarker);
                }

                Sha1Hash CreateSha1Hash(string sha1String) {
                    Span<byte> sha1Bytes = Convert.FromHexString(sha1String).AsSpan();
                    Span<Sha1Hash> sha1 = MemoryMarshal.Cast<byte, Sha1Hash>(sha1Bytes);
                    return sha1[0];
                }

                uint CreateCrc32(string crc32String) {
                    return BitConverter.ToUInt32(Convert.FromHexString(crc32String));
                }

                WriteRomData(game.Value.Rom);
                WriteRomData(game.Value.PrgRom);
                WriteData(game.Value.Pcb, src => new PcbMetadata(
                    (ushort) src.Mapper,
                    (ushort) src.Submapper,
                    src.Mirroring switch {
                        "H" => MirroringType.TwoScreenHorizontalMirroring,
                        "V" => MirroringType.TwoScreenVerticalMirroring,
                        "4" => MirroringType.FourScreen,
                        "0" => MirroringType.OneScreenBlk0,
                        "1" => MirroringType.OneScreenBlk1,
                        _ => throw new ArgumentOutOfRangeException(nameof(src.Mirroring), src.Mirroring, null)
                    },
                    src.Battery != 0
                ));
                WriteData(game.Value.Console, src => new ConsoleMetadata((ExtendedConsoleType) src.Type, (Nes20Header.Flags.CpuPpuTiming) src.Region));
                WriteData(game.Value.Expansion, src => new ExpansionMetadata((DefaultExpansionDevice) src.Type));

                WriteRomData(game.Value.ChrRom, GameRomMetadataElementPresence.ChrRom);
                WriteRomData(game.Value.Trainer, GameRomMetadataElementPresence.Trainer);
                WriteData(game.Value.MiscRom, src => new MiscRomMetadata(src.Size, CreateSha1Hash(src.Sha1), src.Number), GameRomMetadataElementPresence.MiscRom);

                WriteRamSizeData(game.Value.PrgRam, GameRomMetadataElementPresence.PrgRam);
                WriteRamSizeData(game.Value.ChrRam, GameRomMetadataElementPresence.ChrRam);
                WriteRamSizeData(game.Value.ChrNvram, GameRomMetadataElementPresence.ChrNvram);
                WriteRamSizeData(game.Value.PrgNvram, GameRomMetadataElementPresence.PrgNvram);

                WriteData(game.Value.VsSystem, src => new VsSystemMetadata((byte) src.Hardware, (byte) src.Ppu), GameRomMetadataElementPresence.VsSystem);

                //Console.WriteLine($"'{game.Comment}' length: {_stream.Position - startPosition}");
            }
        }
    }

    private void WriteLengthPrefixedString(string value) {
        _writer.Write((ushort) value.Length);
        _writer.Write(value.ToArray());
    }
}
