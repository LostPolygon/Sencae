using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Sencae.Desktop;

public class SettingsStorage<TDto>(string filePath)
    where TDto : new() {
    public string FilePath { get; } = filePath;

    public TDto Value { get; private set; } = default!;

    public void Load() {
        TDto value;
        if (File.Exists(FilePath)) {
            value = JsonSerializer.Deserialize(
                File.ReadAllText(FilePath).AsSpan(),
                GetSerializationTypeInfo()
            )!;
        } else {
            value = new();
        }

        Value = value;
    }

    public void Save() {
        byte[] jsonUtf8 = JsonSerializer.SerializeToUtf8Bytes(Value, GetSerializationTypeInfo());
        File.WriteAllBytes(FilePath, jsonUtf8);
    }

    private static JsonTypeInfo<TDto> GetSerializationTypeInfo() {
        JsonTypeInfo<TDto>? jsonTypeInfo = 
            JsonSerializationSourceGenerationContext.Default.GetTypeInfo(typeof(TDto)) as JsonTypeInfo<TDto>;
        Debug.Assert(jsonTypeInfo != null, "jsonTypeInfo != null");

        return jsonTypeInfo;
    }
}
