using System.Text;

namespace Sencae.Tests; 

public static class TextUtility {
    // Reads a C-style 0-terminated string from Stream
    public static string? ReadCString(Stream stream) {
        List<byte> stringData = new(16);
        do {
            int currentByte = stream.ReadByte();
            if (currentByte == -1)
                return null;

            if (currentByte == 0)
                break;

            stringData.Add((byte) currentByte);
        } while (true);

        return Encoding.ASCII.GetString(stringData.ToArray());
    }
}
