using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

public static class TestCaseSerialization {
    private static readonly JsonSerializerOptions ReadJsonSerializerOptions;
    private static readonly JsonSerializerOptions WriteJsonSerializerOptions;

    static TestCaseSerialization() {
        WriteJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            WriteIndented = true
        };
        
        ReadJsonSerializerOptions = new JsonSerializerOptions(WriteJsonSerializerOptions) {
            Converters = {
                new RamEntryDtoConverter(),
                new CyclesEntryDtoConverter()
            }
        };
    }

    public static TestCaseDto[] DeserializeTestFile(Span<byte> jsonUtf8) {
        return JsonSerializer.Deserialize<TestCaseDto[]>(jsonUtf8, ReadJsonSerializerOptions)!;
    }

    public static string SerializeTestCase(TestCaseDto testCaseDto) {
        return JsonSerializer.Serialize(testCaseDto, WriteJsonSerializerOptions);
    }

    private class RamEntryDtoConverter : JsonConverter<TestCaseDto.RamEntryDto> {
        public override TestCaseDto.RamEntryDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Debug.Assert(reader.TokenType == JsonTokenType.StartArray);
            reader.Read();

            Debug.Assert(reader.TokenType == JsonTokenType.Number);
            ushort address = reader.GetUInt16();
            reader.Read();

            Debug.Assert(reader.TokenType == JsonTokenType.Number);
            byte value = reader.GetByte();
            reader.Read();

            Debug.Assert(reader.TokenType == JsonTokenType.EndArray);

            return new TestCaseDto.RamEntryDto(address, value);
        }

        public override void Write(Utf8JsonWriter writer, TestCaseDto.RamEntryDto value, JsonSerializerOptions options) {
            throw new NotSupportedException();
        }
    }

    private class CyclesEntryDtoConverter : JsonConverter<TestCaseDto.CyclesEntryDto> {
        public override TestCaseDto.CyclesEntryDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Debug.Assert(reader.TokenType == JsonTokenType.StartArray);
            reader.Read();

            Debug.Assert(reader.TokenType == JsonTokenType.Number);
            ushort address = reader.GetUInt16();
            reader.Read();

            Debug.Assert(reader.TokenType == JsonTokenType.Number);
            byte value = reader.GetByte();
            reader.Read();

            Debug.Assert(reader.TokenType == JsonTokenType.String);
            string busOpType = reader.GetString()!;
            reader.Read();

            Debug.Assert(reader.TokenType == JsonTokenType.EndArray);

            return new TestCaseDto.CyclesEntryDto(
                address,
                value,
                busOpType == "read"
            );
        }

        public override void Write(Utf8JsonWriter writer, TestCaseDto.CyclesEntryDto value, JsonSerializerOptions options) {
            throw new NotSupportedException();
        }
    }
}
