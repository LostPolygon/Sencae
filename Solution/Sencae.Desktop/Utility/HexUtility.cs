namespace Sencae.Desktop;

// https://gist.github.com/crozone/06c4aa41e13be89def1352ba0d378b0f
public static class HexUtility {
    private static ReadOnlySpan<byte> HexAlphabetUpperCase => "0123456789ABCDEF"u8;

    private static ReadOnlySpan<byte> HexAlphabetLowerCase => "0123456789abcdef"u8;

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
