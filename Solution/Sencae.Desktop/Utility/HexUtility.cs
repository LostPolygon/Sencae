namespace Sencae.Desktop;

// https://gist.github.com/crozone/06c4aa41e13be89def1352ba0d378b0f
public static class HexUtility {
    private static ReadOnlySpan<byte> HexAlphabetUpperCase => new byte[16] {
        (byte) '0', (byte) '1', (byte) '2', (byte) '3', (byte) '4', (byte) '5', (byte) '6', (byte) '7', (byte) '8', (byte) '9',
        (byte) 'A', (byte) 'B', (byte) 'C', (byte) 'D', (byte) 'E', (byte) 'F'
    };

    private static ReadOnlySpan<byte> HexAlphabetLowerCase => new byte[16] {
        (byte) '0', (byte) '1', (byte) '2', (byte) '3', (byte) '4', (byte) '5', (byte) '6', (byte) '7', (byte) '8', (byte) '9',
        (byte) 'a', (byte) 'b', (byte) 'c', (byte) 'd', (byte) 'e', (byte) 'f'
    };

    public static void HexEncodeBytesUpperCase(ReadOnlySpan<byte> inputBytes, Span<byte> hexEncodedBytes) {
        HexEncodeBytes(inputBytes, hexEncodedBytes, HexAlphabetUpperCase);
    }

    public static void HexEncodeBytesLowerCase(ReadOnlySpan<byte> inputBytes, Span<byte> hexEncodedBytes) {
        HexEncodeBytes(inputBytes, hexEncodedBytes, HexAlphabetLowerCase);
    }

    private static void HexEncodeBytes(ReadOnlySpan<byte> inputBytes, Span<byte> hexEncodedBytes, ReadOnlySpan<byte> alphabet) {
        for (int i = 0; i < inputBytes.Length; i++) {
            hexEncodedBytes[i * 2] = alphabet[inputBytes[i] >> 4];
            hexEncodedBytes[i * 2 + 1] = alphabet[inputBytes[i] & 0xF];
        }
    }
}
