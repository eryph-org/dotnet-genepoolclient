using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Eryph.GenePool.Packing;

/// <summary>
/// This special <see cref="Stream"/> implementation is used by the
/// <see cref="GenePacker"/>. It automatically splits the written
/// data into chunk files of a given size. Additionally, it computes
/// the SHA1 hash of each chunk file on the fly.
/// </summary>
internal sealed class GenePackerStream : Stream
{
    private readonly DirectoryInfo _chunksDirectory;
    private readonly long _chunkSize;
    private bool _isDisposed;
    private long _totalBytes;
    private readonly List<string> _chunks = [];
    private FileStream? _currentChunk;
    private readonly IncrementalHash _incrementalHash;

    public GenePackerStream(DirectoryInfo chunksDirectory, long chunkSize)
    {
        _chunksDirectory = chunksDirectory;
        _chunkSize = chunkSize;
        _incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        StartChunk();
    }

    public IList<string> GetChunks()
    {
        return _chunks;
    }

    public override void Flush()
    {
        _currentChunk?.Flush();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (_currentChunk is not null)
        {
            await _currentChunk.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Write(new ReadOnlySpan<byte>(buffer, offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);

        if (buffer.Length == 0)
            return;

        var currentBuffer = buffer;

        while ((_currentChunk?.Length ?? 0) + currentBuffer.Length > _chunkSize)
        {
            var bytesToWrite = (int)(_chunkSize - (_currentChunk?.Length ?? 0));
            WriteChunk(currentBuffer[..bytesToWrite]);
            EndChunk();
            StartChunk();
            currentBuffer = currentBuffer[bytesToWrite..];
        }

        WriteChunk(currentBuffer);
    }

    private void WriteChunk(ReadOnlySpan<byte> buffer)
    {
        if (_currentChunk is null)
        {
            StartChunk();
        }

        _currentChunk.Write(buffer);
        _incrementalHash.AppendData(buffer);

        _totalBytes += buffer.Length;
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);

        if (buffer.Length == 0)
            return;

        var currentBuffer = buffer;

        while ((_currentChunk?.Length ?? 0) + currentBuffer.Length > _chunkSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytesToWrite = (int)(_chunkSize - (_currentChunk?.Length ?? 0));
            await WriteChunkAsync(currentBuffer[..bytesToWrite], cancellationToken).ConfigureAwait(false);
            await EndChunkAsync().ConfigureAwait(false);
            StartChunk();
            currentBuffer = currentBuffer[bytesToWrite..];
        }

        cancellationToken.ThrowIfCancellationRequested();
        await WriteChunkAsync(currentBuffer, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask WriteChunkAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        if (_currentChunk is null)
            StartChunk();

        await _currentChunk.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        _incrementalHash.AppendData(buffer.Span);
        _totalBytes += buffer.Length;
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => _totalBytes;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    [MemberNotNull(nameof(_currentChunk))]
    private void StartChunk()
    {
        var chunkPath = Path.Combine(_chunksDirectory.FullName, $"{_chunks.Count}.part");
        _currentChunk = new FileStream(chunkPath, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    private async ValueTask EndChunkAsync()
    {
        if (_currentChunk is null)
            throw new InvalidOperationException("Chunk is not started");

        var chunkPath = _currentChunk.Name;
        await _currentChunk.DisposeAsync().ConfigureAwait(false);
        _currentChunk = null;

        EndChunk(chunkPath);
    }

    private void EndChunk()
    {
        if (_currentChunk is null)
            throw new InvalidOperationException("Chunk is not started");

        var chunkPath = _currentChunk.Name;
        _currentChunk.Dispose();
        _currentChunk = null!;

        EndChunk(chunkPath);
    }

    private void EndChunk(string chunkPath)
    {
        string hash;
        var hashBytes = ArrayPool<byte>.Shared.Rent(_incrementalHash.HashLengthInBytes);
        try
        {
            var hashBytesWritten = _incrementalHash.GetHashAndReset(new Span<byte>(hashBytes));
            hash = Convert.ToHexString(new ReadOnlySpan<byte>(hashBytes, 0, hashBytesWritten)).ToLowerInvariant();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(hashBytes);
        }

        var finalChunkPath = Path.Combine(_chunksDirectory.FullName, $"{hash}.part");
        File.Move(chunkPath, finalChunkPath);
        _chunks.Add($"sha1:{hash}");
    }

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                if (_currentChunk is not null)
                    EndChunk();

                _currentChunk?.Dispose();
                _currentChunk = null;

                _incrementalHash.Dispose();
            }

            _isDisposed = true;
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_currentChunk is not null)
            await EndChunkAsync().ConfigureAwait(false);

        if (_currentChunk is not null)
        {
            await _currentChunk.DisposeAsync().ConfigureAwait(false);
            _currentChunk = null;
        }

        _incrementalHash.Dispose();

        Dispose(false);
    }
}
