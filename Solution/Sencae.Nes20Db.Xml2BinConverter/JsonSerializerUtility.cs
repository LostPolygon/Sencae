using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sencae.Nes20Db.Xml2BinConverter;

public static class JsonSerializerUtility {
    public static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web) {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true,
        IgnoreReadOnlyProperties = false,
        IncludeFields = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
