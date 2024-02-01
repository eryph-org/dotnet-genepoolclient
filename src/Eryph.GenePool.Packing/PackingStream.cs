using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace Eryph.GenePool.Packing
{
    internal sealed class PackingStream : Stream
    {
        private readonly DirectoryInfo _chunksDirectory;
        private readonly long _chunkSize;
        private bool _isDisposed;
        private long _totalBytes;

        public PackingStream(DirectoryInfo chunksDirectory, long chunkSize)
        {
            _chunksDirectory = chunksDirectory;
            _chunkSize = chunkSize;
        }

        private readonly List<string> _chunks = new();
        private FileStream? _currentChunk;
        private HashAlgorithm? _hashAlgorithm;

        public IEnumerable<string> GetChunks()
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
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            if (count == 0)
                return;

            var currentOffset = offset;
            var currentCount = count;

            while ((_currentChunk?.Length ?? 0) + currentCount > _chunkSize)
            {
                var bytesToWrite = (int)(_chunkSize - (_currentChunk?.Length ?? 0));
                WriteChunk(buffer, currentOffset, bytesToWrite);
                EndChunk();
                StartChunk();
                currentOffset += bytesToWrite;
                currentCount -= bytesToWrite;
            }

            WriteChunk(buffer, currentOffset, currentCount);

            _totalBytes += count;
        }

        private void WriteChunk(byte[] buffer, int offset, int count)
        {
            if (_currentChunk is null)
            {
                StartChunk();
            }

            if (_hashAlgorithm is null)
                throw new InvalidOperationException("Chunk was not properly started");

            _currentChunk.Write(buffer, offset, count);
            _totalBytes += buffer.Length;

            _hashAlgorithm.TransformBlock(buffer, offset, count, null, 0);
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
            {
                StartChunk();
            }

            if (_hashAlgorithm is null)
                throw new InvalidOperationException("Chunk was not properly started");

            await _currentChunk.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            _totalBytes += buffer.Length;
            
            if (MemoryMarshal.TryGetArray(buffer, out var segment))
            {
                _hashAlgorithm.TransformBlock(segment.Array!, segment.Offset, segment.Count, null, 0);
                return;
            }

            var array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(array);
                _hashAlgorithm.TransformBlock(segment.Array!, segment.Offset, segment.Count, null, 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
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

        [MemberNotNull(nameof(_currentChunk), nameof(_hashAlgorithm))]
        private void StartChunk()
        {
            var chunkPath = Path.Combine(_chunksDirectory.FullName, $"{_chunks.Count}.part");
            _currentChunk = new FileStream(chunkPath, FileMode.Create, FileAccess.Write, FileShare.None);
            _hashAlgorithm = SHA1.Create();
        }

        private async ValueTask EndChunkAsync()
        {
            if (_currentChunk is null || _hashAlgorithm is null)
                throw new InvalidOperationException("Chunk is not started");

            var chunkPath = _currentChunk.Name;
            await _currentChunk.DisposeAsync().ConfigureAwait(false);
            _currentChunk = null;

            EndChunk(chunkPath);
        }

        private void EndChunk()
        {
            if (_currentChunk is null || _hashAlgorithm is null)
                throw new InvalidOperationException("Chunk is not started");

            var chunkPath = _currentChunk.Name;
            _currentChunk.Dispose();
            _currentChunk = null!;
            
            EndChunk(chunkPath);
        }

        private void EndChunk(string chunkPath)
        {
            if (_hashAlgorithm is null)
                throw new InvalidOperationException("Chunk is not started");
            
            _hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            var hashString = BitConverter.ToString(_hashAlgorithm.Hash!).Replace("-", string.Empty).ToLowerInvariant();
            _hashAlgorithm.Dispose();
            _hashAlgorithm = null!;

            var finalChunkPath = Path.Combine(_chunksDirectory.FullName, $"{hashString}.part");
            File.Move(chunkPath, finalChunkPath);
            _chunks.Add($"sha1:{hashString}");
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if(_currentChunk is not null && _hashAlgorithm is not null)
                        EndChunk();

                    _currentChunk?.Dispose();
                    _currentChunk = null!;
                    _hashAlgorithm?.Dispose();
                    _hashAlgorithm = null!;
                }
                
                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            if (_currentChunk is not null && _hashAlgorithm is not null)
                await EndChunkAsync().ConfigureAwait(false);

            if (_currentChunk is not null)
            {
                await _currentChunk.DisposeAsync().ConfigureAwait(false);
                _currentChunk = null;
            }

            if (_hashAlgorithm is not null)
            {
                _hashAlgorithm.Dispose();
                _hashAlgorithm = null;
            }
                
            Dispose(false);
        }
    }
}
