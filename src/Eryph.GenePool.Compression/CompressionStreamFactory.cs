using System.IO.Compression;
using System.Runtime.InteropServices;
using Joveler.Compression.XZ;

namespace Eryph.GenePool.Compression;

/// <summary>
/// Contains helper methods for creating (de)compression streams for the different
/// compression formats which are supported for genes.
/// </summary>
public static class CompressionStreamFactory
{
    private static bool _isXzInitialized;

    public static Stream CreateCompressionStream(Stream targetStream, string format, bool leaveOpen = false)
    {
        return format switch
        {
            "plain" => targetStream,
            "gz" => new GZipStream(targetStream, CompressionLevel.Fastest, leaveOpen),
            "xz" => CreateXzCompressionStream(targetStream, leaveOpen),
            _ => throw new ArgumentException($"The compression format {format} is not supported.", nameof(format))
        };
    }

    public static Stream CreateDecompressionStream(Stream sourceStream, string format, bool leaveOpen = false)
    {
        return format switch
        {
            "plain" => sourceStream,
            "gz" => new GZipStream(sourceStream, CompressionMode.Decompress, leaveOpen),
            "xz" => CreateXzDecompressionStream(sourceStream, leaveOpen),
            _ => throw new ArgumentException($"The compression format {format} is not supported.", nameof(format))
        };
    }

    private static Stream CreateXzCompressionStream(Stream targetStream, bool leaveOpen)
    {
        InitializeNativeLibrary();

        return new XZStream(
            targetStream,
            new XZCompressOptions
            {
                Level = LzmaCompLevel.Default,
                ExtremeFlag = true,
                LeaveOpen = leaveOpen,
            },
            new XZThreadedCompressOptions
            {
                Threads = Environment.ProcessorCount > 8
                    ? Environment.ProcessorCount - 2
                    : Environment.ProcessorCount > 2 ? Environment.ProcessorCount - 1 : 1,
            });
    }

    private static Stream CreateXzDecompressionStream(Stream sourceStream, bool leaveOpen)
    {
        InitializeNativeLibrary();

        return new XZStream(
            sourceStream,
            new XZDecompressOptions
            {
                LeaveOpen = leaveOpen,
            },
            new XZThreadedDecompressOptions
            {
                Threads = Environment.ProcessorCount switch
                {
                    >= 8 => Environment.ProcessorCount - 2,
                    > 2 => Environment.ProcessorCount - 1,
                    _ => 1,
                },
                MemlimitThreading = XZHardware.PhysMem() / 4,
            });
    }

    private static void InitializeNativeLibrary()
    {
        if (_isXzInitialized)
            return;

        _isXzInitialized = true;

        var libDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runtimes");
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
