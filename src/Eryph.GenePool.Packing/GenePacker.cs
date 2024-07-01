using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Eryph.GenePool.Model;
using Joveler.Compression.XZ;

namespace Eryph.GenePool.Packing;

public static class GenePacker
{
    private const int ChunkSize = 1024 * 1024 * 80;
    private const int BufferSize = 1024 * 1024 * 1;

    private static bool _isNativeInitialized;

    public static async Task<string> CreateGene(
        PackableFile file,
        string genesetDir,
        IProgress<GenePackerProgress>? progress = default,
        CancellationToken token = default)
    {
        var originalSize = new FileInfo(file.FullPath).Length;

        progress?.Report(new GenePackerProgress(0, originalSize));

        var tempDir = Path.Combine(genesetDir, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var targetStream = new GenePackerStream(new DirectoryInfo(tempDir), ChunkSize);
        try
        {
            await CompressAsync(targetStream, file.FullPath, file.ExtremeCompression, progress, token);
            await targetStream.DisposeAsync();

            progress?.Report(new GenePackerProgress(originalSize, originalSize));

            // the original content is optional and can be added to the gene, however it will not affect the hash
            var yamlFileName = !string.IsNullOrWhiteSpace(file.YamlContent) ? $"{file.GeneName}.yaml" : null;
            long? yamlSize = !string.IsNullOrWhiteSpace(file.YamlContent) ? Encoding.Unicode.GetByteCount(file.YamlContent) : null;

            var manifestData = new GeneManifestData
            {
                FileName = file.FileName,
                YamlFileName = yamlFileName,
                YamlSize = yamlSize,
                Name = file.GeneName,
                Size = targetStream.Length,
                OriginalSize = originalSize,
                Format = file.ExtremeCompression ? "xz" : "gz",
                Parts = targetStream.GetChunks().ToArray(),
                Type = file.GeneType.ToString().ToLowerInvariant()
            };

            var jsonString = JsonSerializer.Serialize(manifestData, GeneModelDefaults.SerializerOptions);

            using var sha256 = SHA256.Create();
            var manifestHash = GetHashString(sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonString)));
            await File.WriteAllTextAsync(Path.Combine(tempDir, "gene.json"), jsonString, token);

            if(!string.IsNullOrWhiteSpace(yamlFileName))
                await File.WriteAllTextAsync(Path.Combine(tempDir, yamlFileName), jsonString, token);


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
        string filePath,
        bool extremeCompression,
        IProgress<GenePackerProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (extremeCompression)
        {
            InitializeNativeLibrary();
        }

        await using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        await using var compressionStream = extremeCompression
            ? CreateXZStream(targetStream)
            : new GZipStream(targetStream, CompressionLevel.Fastest, false);

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

    static string GetHashString(byte[] hashBytes)
    {
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
    }

    private static Stream CreateXZStream(Stream targetStream)
    {
        InitializeNativeLibrary();

        var compOpts = new XZCompressOptions
        {
            Level = LzmaCompLevel.Default,
            ExtremeFlag = true,
            LeaveOpen = true,
        };

        var threadOpts = new XZThreadedCompressOptions
        {
            Threads = Environment.ProcessorCount > 8
                ? Environment.ProcessorCount - 2
                : Environment.ProcessorCount > 2 ? Environment.ProcessorCount - 1 : 1,
        };

        return new XZStream(targetStream, compOpts, threadOpts);
    }

    private static void InitializeNativeLibrary()
    {
        if (_isNativeInitialized)
            return;

        _isNativeInitialized = true;

        string libDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            libDir = Path.Combine(libDir, "win-");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            libDir = Path.Combine(libDir, "linux-");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            libDir = Path.Combine(libDir, "osx-");

        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86:
                libDir += "x86";
                break;
            case Architecture.X64:
                libDir += "x64";
                break;
            case Architecture.Arm:
                libDir += "arm";
                break;
            case Architecture.Arm64:
                libDir += "arm64";
                break;
        }
        libDir = Path.Combine(libDir, "native");
        if (!Directory.Exists(libDir))
            libDir = AppDomain.CurrentDomain.BaseDirectory;

        string? libPath = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            libPath = Path.Combine(libDir, "liblzma.dll");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            libPath = Path.Combine(libDir, "liblzma.so");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            libPath = Path.Combine(libDir, "liblzma.dylib");

        if (libPath == null)
            throw new PlatformNotSupportedException($"Unable to find native library.");
        if (!File.Exists(libPath))
            throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

        XZInit.GlobalInit(libPath);
    }
}
