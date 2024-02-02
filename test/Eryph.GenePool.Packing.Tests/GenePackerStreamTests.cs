using System.Security.Cryptography;

namespace Eryph.GenePool.Packing.Tests;

public sealed class GenePackerStreamTests : IDisposable
{
    private readonly string _testPath;

    public GenePackerStreamTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testPath);
    }

    [Theory]
    [InlineData(100, 50, 35, 2)]
    [InlineData(142, 50, 35, 3)]
    [InlineData(142, 20, 40, 8)]
    [InlineData(142, 40, 20, 4)]
    public async Task WriteAsync_CreatesCorrectChunks(
        int dataSize,
        int chunkSize,
        int bufferSize,
        int expectedChunks)
    {
        var data = new byte[dataSize];
        Random.Shared.NextBytes(data);

        await using var stream = new GenePackerStream(new DirectoryInfo(_testPath), chunkSize);
        var memory = new ReadOnlyMemory<byte>(data);
        while (memory.Length > 0)
        {
            var chunk = memory[..Math.Min(bufferSize, memory.Length)];
            await stream.WriteAsync(chunk);
            memory = memory[chunk.Length..];
        }
        await stream.DisposeAsync();

        var chunks = stream.GetChunks();
        chunks.Should().HaveCount(expectedChunks);
        Directory.EnumerateFileSystemEntries(_testPath)
            .Should().HaveCount(expectedChunks);

        var actualData = ReadAndAssertChunks(chunks);
        actualData.Should().Equal(data);
    }

    [Fact]
    public async Task WriteAsync_ZeroLengthData_NoChunksAreCreated()
    {
        await using var stream = new GenePackerStream(new DirectoryInfo(_testPath), 1024);
        await stream.WriteAsync(Array.Empty<byte>());
        await stream.DisposeAsync();

        stream.GetChunks().Should().BeEmpty();
        Directory.EnumerateFileSystemEntries(_testPath).Should().BeEmpty();
    }

    [Theory]
    [InlineData(100, 50, 35, 2)]
    [InlineData(142, 50, 35, 3)]
    [InlineData(142, 20, 40, 8)]
    [InlineData(142, 40, 20, 4)]
    public void Write_CreatesCorrectChunks(
        int dataSize,
        int chunkSize,
        int bufferSize,
        int expectedChunks)
    {
        var data = new byte[dataSize];
        Random.Shared.NextBytes(data);

        using var stream = new GenePackerStream(new DirectoryInfo(_testPath), chunkSize);
        var span = new ReadOnlySpan<byte>(data);
        while (span.Length > 0)
        {
            var chunk = span[..Math.Min(bufferSize, span.Length)];
            stream.Write(chunk);
            span = span[chunk.Length..];
        }
        stream.Dispose();

        var chunks = stream.GetChunks();
        chunks.Should().HaveCount(expectedChunks);
        Directory.EnumerateFileSystemEntries(_testPath).Should().HaveCount(expectedChunks);

        var actualData = ReadAndAssertChunks(chunks);
        actualData.Should().Equal(data);
    }

    [Fact]
    public void Write_ZeroLengthData_NoChunksAreCreated()
    {
        using var stream = new GenePackerStream(new DirectoryInfo(_testPath), 1024);
        stream.Write(Array.Empty<byte>());
        stream.Dispose();

        stream.GetChunks().Should().BeEmpty();
        Directory.EnumerateFileSystemEntries(_testPath).Should().BeEmpty();
    }

    private byte[] ReadAndAssertChunks(IEnumerable<string> chunks)
    {
        IEnumerable<byte> result = Array.Empty<byte>();
        
        foreach (var chunk in chunks)
        {
            var chunkPath = Path.Combine(_testPath, $"{chunk.Replace("sha1:","")}.part");
            File.Exists(chunkPath).Should().BeTrue();
            var chunkBytes = File.ReadAllBytes(chunkPath);
            using var sha1 = SHA1.Create();
            Convert.ToHexString(sha1.ComputeHash(chunkBytes)).ToLowerInvariant()
                .Should().Be(chunk.Replace("sha1:", ""));
            result = result.Concat(chunkBytes);
        }
        
        return result.ToArray();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
            Directory.Delete(_testPath, true);
    }
}
