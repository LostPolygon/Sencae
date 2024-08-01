using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace Sencae.Nes20Db.Xml2BinConverter;

internal class Program {
    private static void Main(string[] args) {
        string inputFile =
            args.Length >= 2 && !String.IsNullOrWhiteSpace(args[1]) ?
                args[1] :
                "Resources/nes20db.xml";

        XmlSerializer xmlSerializer = new(typeof(Nes20DbDto));
        xmlSerializer.LogAndFailOnError();

        Nes20DbDto? deserializedDto = (Nes20DbDto?) xmlSerializer.Deserialize(new StreamReader(inputFile));
        //deserializedDto.Games = deserializedDto.Games[0..2];

        string json = JsonSerializer.Serialize(deserializedDto, JsonSerializerUtility.JsonSerializerOptions);

        // Ugly hack to fixup comments
        json = json.Replace("*/," + Environment.NewLine, "*/" + Environment.NewLine);

        JsonDocument jsonDocument = JsonDocument.Parse(json, new JsonDocumentOptions {
            CommentHandling = JsonCommentHandling.Skip,
        });

        using MemoryStream jsonStream = new();
        Utf8JsonWriter writer = new(jsonStream, new JsonWriterOptions { Indented = true });
        jsonDocument.WriteTo(writer);
        writer.Flush();

        string jsonWithoutComments = Encoding.UTF8.GetString(jsonStream.ToArray());

        Console.WriteLine(jsonWithoutComments);
        Console.WriteLine("Done!");
    }
}
