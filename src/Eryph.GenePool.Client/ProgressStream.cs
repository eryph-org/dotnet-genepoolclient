using System;
using System.IO;

namespace Eryph.GenePool.Client;

public class ProgressStream : Stream
{
    private readonly Stream _inputStream;
    private readonly IProgress<long> _progress;

    public ProgressStream(Stream input, IProgress<long> progress)
    {
        _inputStream = input;
        _progress = progress;
    }
    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _inputStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var n = _inputStream.Read(buffer, offset, count);
        _progress.Report(n);
        return n;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead => _inputStream.CanRead;
    public override bool CanSeek => _inputStream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _inputStream.Length;
    public override long Position
    {
        get => _inputStream.Position;
        set => throw new NotSupportedException();
    }
}