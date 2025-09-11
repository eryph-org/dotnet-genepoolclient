using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Eryph.GenePool.Compression;
using Eryph.GenePool.Model;

namespace Eryph.GenePool.Packing;

public static class GenePacker
{
    private const int ChunkSize = 1024 * 1024 * 80;
    private const int BufferSize = 1024 * 1024 * 1;

    public static async Task<string> CreateGene(
        PackableFile file,
        string genesetDir,
        IProgress<GenePackerProgress>? progress = null,
        CancellationToken token = default)
    {
        var originalSize = new FileInfo(file.FullPath).Length;

        progress?.Report(new GenePackerProgress(0, originalSize));

        var tempDir = Path.Combine(genesetDir, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var targetStream = new GenePackerStream(new DirectoryInfo(tempDir), ChunkSize);
        try
        {
            var format = file.ExtremeCompression switch
            {
                true => "xz",
                false when originalSize >= GeneModelDefaults.MinCompressionBytes => "gz",
                false => "plain"
            };

            await CompressAsync(targetStream, file.FullPath, format, progress, token);
            await targetStream.DisposeAsync();

            progress?.Report(new GenePackerProgress(originalSize, originalSize));

            var manifestData = new GeneManifestData
            {
                Version = GeneModelDefaults.LatestGeneManifestVersion.ToString(),
                FileName = file.FileName,
                Name = file.GeneName,
                Size = targetStream.Length,
                OriginalSize = originalSize,
                Format = format,
                Parts = targetStream.GetChunks().ToArray(),
                Type = file.GeneType.ToString().ToLowerInvariant(),
                Architecture = file.Architecture,
            };

            var jsonString = JsonSerializer.Serialize(manifestData, GeneModelDefaults.SerializerOptions);

            using var sha256 = SHA256.Create();
            var manifestHash = GetHashString(sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonString)));
            await File.WriteAllTextAsync(Path.Combine(tempDir, "gene.json"), jsonString, Encoding.UTF8, token);

            if(!string.IsNullOrWhiteSpace(file.YamlContent))
                await File.WriteAllTextAsync(Path.Combine(tempDir, "gene.yaml"), file.YamlContent, Encoding.UTF8, token);


            var destDir = Path.Combine(Path.GetDirectoryName(tempDir)!, manifestHash);
            if (Directory.Exists(destDir))
                Directory.Delete(destDir, true);

            Directory.Move(tempDir, destDir);

            return $"sha256:{manifestHash}";
        }
        finally
        {
            await targetStream.DisposeAsync();
        }
    }

    private static async Task CompressAsync(
        Stream targetStream,
        string sourcePath,
        string format,
        IProgress<GenePackerProgress>? progress,
        CancellationToken cancellationToken)
    {
        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using var compressionStream = CompressionStreamFactory.CreateCompressionStream(targetStream, format);

        var buffer = ArrayPool<byte>.Shared.Rent(Math.Min(BufferSize, (int)Math.Min(int.MaxValue, sourceStream.Length)));
        try
        {
            int read;
            long totalRead = 0;
            while ((read = await sourceStream.ReadAsync(new Memory<byte>(buffer), cancellationToken)) > 0)
            {
                await compressionStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, read), cancellationToken);
                totalRead += read;

                progress?.Report(new GenePackerProgress(totalRead, sourceStream.Length));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static string GetHashString(byte[] hashBytes)
    {
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
    }
}
