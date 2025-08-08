using Joveler.Compression.XZ;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eryph.GenePool.Compression;

public static class CompressionStreamFactory
{
    private static bool _isXzInitialized;

    public static Stream CreateCompressionStream(Stream targetStream, string format)
    {
        return format switch
        {
            "plain" => targetStream,
            // TODO should we set leave open to false
            "gz" => new GZipStream(targetStream, CompressionLevel.Fastest, false),
            "xz" => CreateXzCompressionStream(targetStream),
            _ => throw new ArgumentException($"The compression format {format} is not supported.", nameof(format))
        };
    }

    public static Stream CreateDecompressionStream(Stream sourceStream, string format)
    {
        return format switch
        {
            "plain" => sourceStream,
            "gz" => new GZipStream(sourceStream, CompressionMode.Decompress),
            "xz" => CreateXzDecompressionStream(sourceStream),
            _ => throw new ArgumentException($"The compression format {format} is not supported.", nameof(format))
        };
    }

    private static Stream CreateXzCompressionStream(Stream targetStream)
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

    private static Stream CreateXzDecompressionStream(Stream sourceStream)
    {
        InitializeNativeLibrary();

        var threadOpts = new XZThreadedDecompressOptions
        {
            Threads = Environment.ProcessorCount switch
            {
                >= 8 => Environment.ProcessorCount - 2,
                > 2 => Environment.ProcessorCount - 1,
                _ => 1,
            },
            MemlimitThreading = XZHardware.PhysMem() / 4
        };

        return new XZStream(sourceStream, new XZDecompressOptions(), threadOpts);
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
