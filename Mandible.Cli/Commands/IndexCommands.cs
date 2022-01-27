using CommandDotNet;
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

[Command("index", Description = "Builds JSON-structured indexes of the given pack/pack2 file/s.")]
public class IndexCommands
{
    private readonly IAnsiConsole _console;
    private readonly CancellationToken _ct;

    public IndexCommands(IAnsiConsole console, CancellationToken ct)
    {
        _console = console;
        _ct = ct;
    }

    [DefaultCommand]
    public async Task ExecuteAsync
    (
        [Operand(Description = "A path to the pack/pack2 file to index, or a directory containing multiple.")]
        string inputPath,

        [Operand(Description = "The directory to output the index files to.")]
        string outputDirectory,

        [Option('n', Description = "A path to a namelist file.")]
        string? namelistPath,

        [Option('p', Description = "Disable pretty-printing of the JSON output.")]
        bool noPrettyPrint = false
    )
    {
        if (!CommandUtils.TryFindPacksFromPath(_console, inputPath, out List<string> packFiles, out List<string> pack2Files))
            return;

        if (!CommandUtils.CheckOutputDirectory(_console, outputDirectory))
            return;

        if (Directory.EnumerateFiles(outputDirectory).Any())
        {
            if (!_console.Confirm("The output directory already contains files. Existing indexes in the directory may be overwritten. Are you sure you want to continue?"))
                return;
        }

        Namelist? namelist = null;
        if (namelistPath is not null)
            namelist = await CommandUtils.BuildNamelistAsync(_console, namelistPath, _ct);

        List<PackIndex> packIndexes = await BuildIndexAsync(packFiles);
        List<PackIndex> pack2Indexes = await BuildIndex2Async(pack2Files, namelist);

        await SaveIndexes(packIndexes, "pack", !noPrettyPrint, outputDirectory);
        await SaveIndexes(pack2Indexes, "pack2", !noPrettyPrint, outputDirectory);

        _console.Markup("[green]Indexing Complete![/]");
    }

    private async Task<List<PackIndex>> BuildIndexAsync(IReadOnlyList<string> packFiles)
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

                        IReadOnlyList<AssetHeader> assetHeaders = await reader.ReadAssetHeadersAsync(_ct);

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

    private async Task<List<PackIndex>> BuildIndex2Async(IReadOnlyList<string> pack2Files, Namelist? namelist)
        => await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask indexTask = ctx.AddTask("Building pack2 indexes...");
                    if (namelist is null)
                        namelist = new Namelist();

                    List<PackIndex> packIndexes = new();
                    double increment = indexTask.MaxValue / pack2Files.Count;

                    foreach (string file in pack2Files)
                    {
                        using RandomAccessDataReaderService dataReader = new(file);
                        using Pack2Reader reader = new(dataReader);

                        Pack2Header header = await reader.ReadHeaderAsync(_ct).ConfigureAwait(false);
                        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(_ct).ConfigureAwait(false);

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
                }
            );

    private async Task SaveIndexes
    (
        IReadOnlyList<PackIndex> indexes,
        string suffix,
        bool prettyPrint,
        string outputDirectory
    )
    {
        JsonSerializerOptions jsonOptions = new();
        jsonOptions.WriteIndented = prettyPrint;

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
                    await JsonSerializer.SerializeAsync(metadataStream, metadata, jsonOptions, _ct).ConfigureAwait(false);
                    saveTask.Increment(increment);

                    foreach (PackIndex index2 in indexes)
                    {
                        string fileName = Path.GetFileName(index2.Path);
                        await using FileStream index2Stream = File.Open
                        (
                            Path.Combine(outputDirectory, fileName + ".json"),
                            FileMode.Create
                        );

                        await JsonSerializer.SerializeAsync(index2Stream, index2, jsonOptions, _ct).ConfigureAwait(false);
                        saveTask.Increment(increment);
                    }
                }
            );
    }
}
