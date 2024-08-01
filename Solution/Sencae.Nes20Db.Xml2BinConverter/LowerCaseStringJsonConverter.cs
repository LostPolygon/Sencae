using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sencae.Nes20Db.Xml2BinConverter;

public class LowerCaseStringJsonConverter : JsonConverter<string> {
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.GetString()?.ToLowerInvariant();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToLowerInvariant());
    }
}
