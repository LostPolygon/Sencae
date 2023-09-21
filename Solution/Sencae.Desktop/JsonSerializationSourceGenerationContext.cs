using System.Text.Json.Serialization;
using Sencae.Desktop;

namespace Sencae;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    AllowTrailingCommas = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString
)]
[JsonSerializable(typeof(UIPersistentSettings))]
internal partial class JsonSerializationSourceGenerationContext : JsonSerializerContext {
}
