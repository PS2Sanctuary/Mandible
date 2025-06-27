using CommunityToolkit.HighPerformance.Buffers;
using ConsoleAppFramework;
using Mandible.Pack2;
using Mandible.Services;
using Mandible.Util;
using Microsoft.Win32.SafeHandles;
using Spectre.Console;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mandible.Util.Zlib;
using ZlibNGSharpMinimal;
using ZlibNGSharpMinimal.Exceptions;

namespace Mandible.Cli.Commands;

public class PackCommands
{
    private readonly IAnsiConsole _console;

    public PackCommands(IAnsiConsole console)
    {
        _console = console;
    }

    /// <summary>
    /// Creates a pack2 archive from files in the given input directory.
    /// </summary>
    /// <param name="inputDirectory">The directory containing files to pack.</param>
    /// <param name="outputPath">The path to write the generated pack2 file to.</param>
    /// <param name="verbose">-v, Enable verbose output.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("create-pack-2")]
    public async Task CreatePack2
    (
        [Argument] string inputDirectory,
        [Argument] string outputPath,
        bool verbose = true,
        CancellationToken ct = default
    )
    {
        if (File.Exists(outputPath))
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        if (!Directory.Exists(inputDirectory))
        {
            _console.MarkupLine("[red]The input directory does not exist[/]");
            return;
        }

        using RandomAccessDataWriterService ioWriter = new(outputPath, FileMode.Create);
        await using Pack2Writer writer = new(ioWriter);
        string[] filesToPack = Directory.GetFiles(inputDirectory);

        await _console.Progress()
            .StartAsync(async ctx =>
            {
                ProgressTask exportTask = ctx.AddTask("Exporting pack assets...");
                exportTask.MaxValue = filesToPack.Length;

                await WriteFilesToPack2(exportTask, verbose, filesToPack, writer, ct);
            });

        _console.MarkupLine("[green]Packing complete![/]");
    }

    private async Task WriteFilesToPack2
    (
        ProgressTask taskCtx,
        bool verboseOutput,
        string[] files,
        Pack2Writer writer,
        CancellationToken ct
    )
    {
        const int deflatePreferredTolerance = 1024;
        using ZlibDeflator deflator = new(ZlibCompressionLevel.BestCompression, includeZlibHeader: true);

        // Sort the filenames in ascending order of name hash
        Array.Sort
        (
            files,
            (x1, x2) => PackCrc64.Calculate(Path.GetFileName(x1)).CompareTo(PackCrc64.Calculate(Path.GetFileName(x2)))
        );

        foreach (string file in files)
        {
            using MemoryOwner<byte> assetData = LoadFileData(file);
            // +2 to Add size of the zlib header
            using MemoryOwner<byte> deflatedBuffer = MemoryOwner<byte>.Allocate(assetData.Length + 2);

            // These file types are never compressed
            bool mayCompress = Path.GetExtension(file).ToLower() is not (".cnk4" or ".cnk5" or ".def" or ".gfx")
                && assetData.Length > 0;

            int deflatedLength = int.MaxValue - deflatePreferredTolerance;
            try
            {
                if (mayCompress)
                    deflatedLength = (int)deflator.Deflate(assetData.Span, deflatedBuffer.Span);
            }
            catch (ZngCompressionException zce) when (zce.ErrorCode is CompressionResult.OK)
            {
                // This is fine, almost certainly the buffer deflated to a larger size so no point in compressing
            }
            deflator.Reset();

            Memory<byte> bufferToWrite = assetData.Memory;
            bool isCompressed = false;
            // Only use the deflated buffer if its length is smaller than the uncompressed length by more than the tolerance
            if (mayCompress && deflatedLength + deflatePreferredTolerance < assetData.Length)
            {
                bufferToWrite = deflatedBuffer.Memory[..deflatedLength];
                isCompressed = true;
            }

            await writer.WriteAssetAsync
            (
                Path.GetFileName(file),
                bufferToWrite,
                isCompressed ? Asset2ZipDefinition.Zipped : Asset2ZipDefinition.Unzipped,
                null,
                true,
                ct
            );

            if (verboseOutput)
                _console.WriteLine($"Packed {file}");
            taskCtx.Increment(1);
        }
    }

    private static MemoryOwner<byte> LoadFileData(string filePath)
    {
        using SafeFileHandle handle = File.OpenHandle(filePath);

        long fileLen = RandomAccess.GetLength(handle);
        MemoryOwner<byte> buffer = MemoryOwner<byte>.Allocate((int)fileLen);
        RandomAccess.Read(handle, buffer.Span, 0);

        return buffer;
    }
}
