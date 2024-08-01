using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Sencae.Nes20Db.Xml2BinConverter;

[XmlRoot("nes20db")]
public class Nes20DbDto {
    [XmlAttribute("date")] [JsonPropertyOrder(-1)] public string Date;

    //[XmlArrayItem(ElementName = "game")]
    [XmlElement("game")]
    public GameDtoWrapper[] Games { get; set; }

    [JsonConverter(typeof(GameDtoWrapperJsonConverter))]
    public class GameDtoWrapper : IXmlSerializable {
        public string Comment;
        public GameDto Value;

        public XmlSchema? GetSchema() {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader) {
            string innerXml = reader.ReadOuterXml();

            reader = XmlReader.Create(new StringReader(innerXml));
            reader.Read();
            if (reader.LocalName != "game")
                throw new SerializationException("Excepted a node with a name 'game'");

            reader.Read();
            if (reader.NodeType != XmlNodeType.Comment)
                throw new SerializationException("Excepted a comment node");

            Comment = reader.Value.Trim();

            XmlSerializer xmlSerializer = new(typeof(GameDto));
            xmlSerializer.LogAndFailOnError();

            reader = XmlReader.Create(new StringReader(innerXml));
            Value = (GameDto) xmlSerializer.Deserialize(reader)!;
        }

        public void WriteXml(XmlWriter writer) {
            throw new NotImplementedException();
        }
    }

    public class GameDtoWrapperJsonConverter : JsonConverter<GameDtoWrapper> {
        public override GameDtoWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, GameDtoWrapper value, JsonSerializerOptions options) {
            writer.WriteRawValue(Environment.NewLine.AsSpan(), true);
            writer.WriteCommentValue($" {value.Comment} ");
            
            JsonSerializer.Serialize(writer, value.Value, JsonSerializerUtility.JsonSerializerOptions);
        }
    }

    [XmlRoot("game")]
    public class GameDto {
        [XmlElement("prgrom")] public PrgRomAndChrRomDto PrgRom;
        [XmlElement("chrrom")] public PrgRomAndChrRomDto ChrRom;
        [XmlElement("trainer")] public RomDto Trainer;
        [XmlElement("rom")] public RomDto Rom;
        [XmlElement("miscrom")] public MiscRomDto MiscRomSize;
        [XmlElement("prgram")] public RamSizeDto PrgRamSize;
        [XmlElement("chrram")] public RamSizeDto ChrRamSize;
        [XmlElement("chrnvram")] public RamSizeDto ChrNvramSize;
        [XmlElement("prgnvram")] public RamSizeDto PrgNvramSize;
        [XmlElement("pcb")] public PcbDto Pcb;
        [XmlElement("console")] public ConsoleDto Console;
        [XmlElement("expansion")] public ExpansionDto Expansion;
        [XmlElement("vs")] public VsSystemDto VsSystem;
    }

    public class RomDto {
        [XmlAttribute("size")] public int Size;
        [XmlAttribute("crc32")] [JsonConverter(typeof(LowerCaseStringJsonConverter))] public string Crc32;
        [XmlAttribute("sha1")] [JsonConverter(typeof(LowerCaseStringJsonConverter))] public string Sha1;
    }

    public class PrgRomAndChrRomDto : RomDto {
        [XmlAttribute("sum16")] [JsonPropertyOrder(10)] [JsonConverter(typeof(LowerCaseStringJsonConverter))] public string Sum16;
    }

    public class MiscRomDto : RomDto {
        [XmlAttribute("number")] public int Number;
    }

    public class PcbDto {
        [XmlAttribute("mapper")] public int Mapper;
        [XmlAttribute("submapper")] public int Submapper;
        [XmlAttribute("mirroring")] public string Mirroring;
        [XmlAttribute("battery")] public int Battery;
    }

    public class VsSystemDto {
        [XmlAttribute("hardware")] public int Hardware;
        [XmlAttribute("ppu")] public int Ppu;
    }

    public class ConsoleDto {
        [XmlAttribute("type")] public int Type;
        [XmlAttribute("region")] public int Region;
    }

    public class RamSizeDto {
        [XmlAttribute("size")] public int Size;
    }

    public class ExpansionDto {
        [XmlAttribute("type")] public int Type;
    }
}
