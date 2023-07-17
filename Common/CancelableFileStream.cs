using System;
using System.IO;
using System.Threading;

namespace RNGExperiments;

public class CancelableFileStream : Stream, IDisposable
{
    FileStream _stream;

    CancellationToken _token;

    public CancelableFileStream(FileStream stream, CancellationToken token) {
        _stream = stream;
        _token = token;
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position { get => _stream.Position; set => _stream.Position = value; }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _token.ThrowIfCancellationRequested();
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        _token.ThrowIfCancellationRequested();
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _token.ThrowIfCancellationRequested();
        _stream.Write(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _stream.Dispose();
    }
}