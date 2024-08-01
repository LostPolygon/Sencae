using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Sencae.Nes20Db.Xml2BinConverter;

internal static class XmlSerializerUtility {
    public static void LogAndFailOnError(this XmlSerializer xmlSerializer) {
        string FormatError(object obj) {
            JsonSerializerOptions errorJsonSerializerOptions = new(JsonSerializerDefaults.General) {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true,
                IgnoreReadOnlyProperties = false
            };

            return JsonSerializer.Serialize(obj, errorJsonSerializerOptions);
        }
        xmlSerializer.UnknownAttribute += (sender, eventArgs) =>
            throw new SerializationException($"Unknown attribute:\n{FormatError(eventArgs)}");
        xmlSerializer.UnknownElement += (sender, eventArgs) =>
            throw new SerializationException($"Unknown element:\n{FormatError(eventArgs)}");
        xmlSerializer.UnknownNode += (sender, eventArgs) =>
            throw new SerializationException($"Unknown node:\n{FormatError(eventArgs)}");
        xmlSerializer.UnreferencedObject += (sender, eventArgs) =>
            throw new SerializationException($"Unreferenced object:\n{FormatError(eventArgs)}");
    }
}
