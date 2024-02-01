namespace Eryph.GenePool.Packing.Tests
{
    public sealed class GenePackerStreamTests : IDisposable
    {
        private readonly string testPath;

        public GenePackerStreamTests()
        {
            testPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(testPath);
        }

        [Fact]
        public async Task WritingExactMultipleOfChunkSize_CreatesCorrectChunks()
        {
            const int chunkSize = 100;
            var data = new byte[chunkSize * 2];
            Random.Shared.NextBytes(data);

            await using var stream = new GenePackerStream(new DirectoryInfo(testPath), chunkSize);
            await stream.WriteAsync(data);
            await stream.DisposeAsync();

            var chunks = stream.GetChunks();
            chunks.Should().HaveCount(2);
            var chunkPaths = Directory.EnumerateFileSystemEntries(testPath)
                .Should().HaveCount(2);

            var actualData = await ReadChunks(chunks);
            actualData.Should().Equal(data);
        }

        [Fact]
        public async Task WritingZeroLengthData_ChunkIsNotCreated()
        {
            await using var stream = new GenePackerStream(new DirectoryInfo(testPath), 1024);
            await stream.WriteAsync(Array.Empty<byte>());
            await stream.DisposeAsync();

            stream.GetChunks().Should().BeEmpty();
            Directory.EnumerateFileSystemEntries(testPath).Should().BeEmpty();
        }

        private async Task<byte[]> ReadChunks(IEnumerable<string> chunks)
        {
            IEnumerable<byte> result = Array.Empty<byte>();
            
            foreach (var chunk in chunks)
            {
                var chunkPath = Path.Combine(testPath, $"{chunk.Replace("sha1:","")}.part");
                File.Exists(chunkPath).Should().BeTrue();
                var chunkBytes = await File.ReadAllBytesAsync(chunkPath);
                result = result.Concat(chunkBytes);
            }
            
            return result.ToArray();
        }

        public void Dispose()
        {
            if (Directory.Exists(testPath))
                Directory.Delete(testPath, true);
        }
    }
}