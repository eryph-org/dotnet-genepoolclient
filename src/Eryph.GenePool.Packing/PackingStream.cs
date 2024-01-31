using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Eryph.GenePool.Packing
{
    internal class PackingStream : Stream
    {
        private readonly DirectoryInfo _chunksDirectory;
        private readonly long _chunkSize;
        private bool _isDisposed;

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
                StartNewChunk();
                currentOffset += bytesToWrite;
                currentCount -= bytesToWrite;
            }

            WriteChunk(buffer, currentOffset, currentCount);
        }

        private void WriteChunk(byte[] buffer, int offset, int count)
        {
            if (_currentChunk is null)
            {
                StartNewChunk();
            }

            if(_hashAlgorithm is null)
                throw new InvalidOperationException("Chunk was not properly started");

            _currentChunk.Write(buffer, offset, count);
            _hashAlgorithm.TransformBlock(buffer, offset, count, null, 0);
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;
        
        public override bool CanWrite => true;
        
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        [MemberNotNull(nameof(_currentChunk), nameof(_hashAlgorithm))]
        private void StartNewChunk()
        {
            if (_currentChunk is not null)
            {
                EndChunk();
            }
            
            StartChunk();
        }

        [MemberNotNull(nameof(_currentChunk), nameof(_hashAlgorithm))]
        private void StartChunk()
        {
            var chunkPath = Path.Combine(_chunksDirectory.FullName, $"{_chunks.Count}.part");
            _currentChunk = new FileStream(chunkPath, FileMode.Create, FileAccess.Write, FileShare.None);
            _hashAlgorithm = SHA1.Create();
        }

        private void EndChunk()
        {
            if (_currentChunk is null || _hashAlgorithm is null)
                throw new InvalidOperationException("Chunk is not started");

            _hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            var hashString = BitConverter.ToString(_hashAlgorithm.Hash!).Replace("-", string.Empty).ToLowerInvariant();
            _hashAlgorithm.Dispose();
            _hashAlgorithm = null!;

            var tempFileName = _currentChunk.Name;
            _currentChunk.Dispose();
            _currentChunk = null!;

            var fullPath = Path.Combine(_chunksDirectory.FullName, $"{hashString}.part");
            File.Move(tempFileName, fullPath);
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
    }
}
