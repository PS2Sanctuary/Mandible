using ConsoleAppFramework;
using Mandible.Cli.Objects;
using Mandible.Cli.Util;
using Mandible.Pack;
using Mandible.Pack2;
using Mandible.Pack2.Names;
using Mandible.Services;
using Spectre.Console;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

/// <summary>
/// Builds JSON-structured indexes of the given pack/pack2 file/s.
/// </summary>
public class IndexCommands
{
    private readonly IAnsiConsole _console;

    public IndexCommands(IAnsiConsole console)
    {
        _console = console;
    }

    /// <summary>
    /// Builds JSON-structured indexes of the given pack/pack2 file/s.
    /// </summary>
    /// <param name="inputPath">A path to the pack/pack2 file to index, or a directory containing multiple.</param>
    /// <param name="outputDirectory">The directory to output the index files to.</param>
    /// <param name="namelistPath">-n|--namelist, A path to a namelist file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("")]
    public async Task ExecuteAsync
    (
        [Argument] string inputPath,
        [Argument] string outputDirectory,
        string? namelistPath,
        CancellationToken ct = default
    )
    {
        if (!CommandUtils.TryFindPacksFromPath(_console, inputPath, out List<string> packFiles, out List<string> pack2Files))
            return;

        if (!CommandUtils.CheckOutputDirectory(_console, outputDirectory))
            return;

        if (Directory.EnumerateFiles(outputDirectory).Any())
        {
            if (!_console.Confirm("The output directory already contains files. [red]Existing indexes in the directory may be overwritten.[/] Are you sure you want to continue?"))
                return;
        }

        Namelist? namelist = null;
        if (namelistPath is not null)
            namelist = await CommandUtils.BuildNamelistAsync(_console, namelistPath, ct);

        if (packFiles.Count > 0)
        {
            List<PackIndex> packIndexes = await BuildIndexAsync(packFiles, ct);
            await SaveIndexes(packIndexes, "pack", outputDirectory, ct);
        }

        if (pack2Files.Count > 0)
        {
            List<PackIndex> pack2Indexes = await BuildIndex2Async(pack2Files, namelist, ct);
            await SaveIndexes(pack2Indexes, "pack2", outputDirectory, ct);
        }

        _console.Markup("[green]Indexing Complete![/]");
    }

    private async Task<List<PackIndex>> BuildIndexAsync(IReadOnlyList<string> packFiles, CancellationToken ct)
        => await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask indexTask = ctx.AddTask("Building pack indexes...");

                    List<PackIndex> packIndexes = new();
                    double increment = indexTask.MaxValue / packFiles.Count;

                    foreach (string file in packFiles)
                    {
                        using RandomAccessDataReaderService dataReader = new(file);
                        PackReader reader = new(dataReader);

                        IReadOnlyList<AssetHeader> assetHeaders = await reader.ReadAssetHeadersAsync(ct);

                        IEnumerable<PackIndex.IndexAsset> assets = assetHeaders
                            .Select(s => PackIndex.IndexAsset.FromAssetHeader(s))
                            .OrderBy(a => a.Name);

                        packIndexes.Add
                        (
                            new PackIndex
                            (
                                Path.GetFullPath(file),
                                (uint)assetHeaders.Count,
                                (ulong)dataReader.GetLength(),
                                assets.ToList()
                            )
                        );

                        indexTask.Increment(increment);
                    }

                    return packIndexes;
                }
            );

    private async Task<List<PackIndex>> BuildIndex2Async
    (
        IReadOnlyList<string> pack2Files,
        Namelist? namelist,
        CancellationToken ct
    )
    {
        return await _console.Progress()
            .StartAsync(async ctx =>
            {
                ProgressTask indexTask = ctx.AddTask("Building pack2 indexes...");
                namelist ??= new Namelist();

                List<PackIndex> packIndexes = [];
                double increment = indexTask.MaxValue / pack2Files.Count;

                foreach (string file in pack2Files)
                {
                    using RandomAccessDataReaderService dataReader = new(file);
                    using Pack2Reader reader = new(dataReader);

                    Pack2Header header = await reader.ReadHeaderAsync(ct);
                    IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct);

                    IEnumerable<PackIndex.IndexAsset> assets = assetHeaders
                        .Select(s => PackIndex.IndexAsset.FromAsset2Header(s, namelist))
                        .OrderBy(a => a.Name);

                    packIndexes.Add
                    (
                        new PackIndex
                        (
                            Path.GetFullPath(file),
                            header.AssetCount,
                            header.Length,
                            assets.ToList()
                        )
                    );

                    indexTask.Increment(increment);
                }

                return packIndexes;
            });
    }

    private async Task SaveIndexes
    (
        IReadOnlyCollection<PackIndex> indexes,
        string suffix,
        string outputDirectory,
        CancellationToken ct
    )
    {
        await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask saveTask = ctx.AddTask($"Saving {suffix} indexes");
                    double increment = saveTask.MaxValue / (indexes.Count + 1);

                    await using FileStream metadataStream = File.Open
                    (
                        Path.Combine(outputDirectory, $"_Metadata-{suffix}.json"),
                        FileMode.Create
                    );

                    IndexMetadata metadata = IndexMetadata.FromIndexList(indexes);
                    await JsonSerializer.SerializeAsync
                    (
                        metadataStream,
                        metadata,
                        CliJsonContext.Default.IndexMetadata,
                        ct
                    );
                    saveTask.Increment(increment);

                    foreach (PackIndex index2 in indexes)
                    {
                        string fileName = Path.GetFileName(index2.Path);
                        await using FileStream index2Stream = File.Open
                        (
                            Path.Combine(outputDirectory, fileName + ".json"),
                            FileMode.Create
                        );

                        await JsonSerializer.SerializeAsync
                        (
                            index2Stream,
                            index2,
                            CliJsonContext.Default.PackIndex,
                            ct
                        );
                        saveTask.Increment(increment);
                    }
                }
            );
    }
}
