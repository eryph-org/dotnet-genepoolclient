using System;
using System.IO;

namespace Eryph.GenePool.Client;

public class ProgressStream(Stream input, IProgress<long> progress) : Stream
{
    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return input.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var n = input.Read(buffer, offset, count);
        progress.Report(n);
        return n;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead => input.CanRead;
    public override bool CanSeek => input.CanSeek;
    public override bool CanWrite => false;
    public override long Length => input.Length;
    public override long Position
    {
        get => input.Position;
        set => throw new NotSupportedException();
    }
}