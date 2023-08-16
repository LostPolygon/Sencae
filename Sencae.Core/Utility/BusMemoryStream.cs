using System;
using System.IO;

namespace Sencae;

public class BusMemoryStream : Stream {
    public IBusDevice BusDevice { get; }

    public BusMemoryStream(IBusDevice busDevice) {
        BusDevice = busDevice;
    }

    private long _position;

    public override void Flush() {
    }

    public override int Read(byte[] buffer, int offset, int count) {
        int resultCount = 0;
        for (int i = 0; i < count; i++) {
            if (_position == UInt16.MaxValue)
                break;

            byte data = 0;
            BusDevice.HandleBus((ushort) _position, false, ref data);
            buffer[offset + i] = data;
            Position++;
            resultCount++;
        }

        return resultCount;
    }

    public override void Write(byte[] buffer, int offset, int count) {
        for (int i = 0; i < count; i++) {
            if (_position == UInt16.MaxValue)
                break;

            byte data = buffer[offset + i];
            BusDevice.HandleBus((ushort) _position, true, ref data);
            Position++;
        }
    }

    public override long Seek(long offset, SeekOrigin origin) {
        switch (origin) {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = UInt16.MaxValue - offset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }

        return Position;
    }

    public override void SetLength(long value) {
        throw new NotSupportedException();
    }

    public override bool CanRead { get; } = true;
    public override bool CanSeek { get; } = true;
    public override bool CanWrite { get; } = true;
    public override long Length { get; } = UInt16.MaxValue;

    public override long Position {
        get => _position;
        set {
            if (value is < 0 or > UInt16.MaxValue)
                throw new IndexOutOfRangeException($"Position must be within [0x0000-0xFFFF], got {value}");

            _position = value;
        }
    }
}
