using System.Text.Json.Serialization;

namespace Sencae.Tests.TomHarteProcessorTestsNes6502;

public readonly record struct TestCaseDto {
    [JsonPropertyOrder(1)] public string Name { get; init; }
    [JsonPropertyOrder(2)] public CpuStateDto Initial { get; init; }
    [JsonPropertyOrder(3)] public CpuStateDto Final { get; init; }
    [JsonPropertyOrder(4)] public CyclesEntryDto[] Cycles { get; init; }

    public readonly record struct CpuStateDto {
        [JsonPropertyOrder(1)] public ushort PC { get; init; }
        [JsonPropertyOrder(2)] public byte S { get; init; }
        [JsonPropertyOrder(3)] public byte A { get; init; }
        [JsonPropertyOrder(4)] public byte X { get; init; }
        [JsonPropertyOrder(5)] public byte Y { get; init; }
        [JsonPropertyOrder(6)] public byte P { get; init; }
        [JsonPropertyOrder(7)] public RamEntryDto[] Ram { get; init; }
    }

    public readonly record struct RamEntryDto(
        [property: JsonPropertyOrder(1)] ushort Address,
        [property: JsonPropertyOrder(2)] byte Value
    );

    public readonly record struct CyclesEntryDto(
        [property: JsonPropertyOrder(1)] ushort Address,
        [property: JsonPropertyOrder(2)] byte BusData,
        [property: JsonPropertyOrder(3)] bool IsRead
    );
}
