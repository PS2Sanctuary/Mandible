using CommandDotNet;
using Mandible.Cli.Objects;
using Mandible.Cli.Util;
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

[Command(
    "index",
    Description = "Builds a JSON-structured index of the given pack/pack2 file/s."
)]
public class IndexCommand
{
    private readonly IAnsiConsole _console;
    private readonly CancellationToken _ct;

    public IndexCommand(IAnsiConsole console, CancellationToken ct)
    {
        _console = console;
        _ct = ct;
    }

    // TODO: Add pack support
    [DefaultCommand]
    public async Task ExecuteAsync
    (
        [Operand(Description = "A path to the pack/pack2 file to index, or a directory containing multiple.")]
        string inputPath,

        [Operand(Description = "The directory to output the index files to.")]
        string outputDirectory,

        [Operand(Description = "A path to a namelist file.")]
        string namelistPath,

        [Option('p')]
        bool noPrettyPrint = false
    )
    {
        bool packsDiscovered = CommandUtils.TryFindPacksFromPath
        (
            _console,
            inputPath,
            out List<string> packFiles,
            out List<string> pack2Files
        );

        if (!packsDiscovered)
            return;

        if (!CommandUtils.CheckOutputDirectory(_console, outputDirectory))
            return;

        if (Directory.EnumerateFiles(outputDirectory).Any())
        {
            if (!_console.Confirm("The output directory already contains files. Existing indexes in the directory may be overwritten. Are you sure you want to continue?"))
                return;
        }

        Namelist namelist = await CommandUtils.BuildNamelistAsync(_console, namelistPath, _ct).ConfigureAwait(false);

        List<Objects.PackIndex> pack2Indexes = await BuildIndex2Async(pack2Files, namelist).ConfigureAwait(false);
        IndexMetadata pack2Metadata = IndexMetadata.FromIndexList(pack2Indexes);

        JsonSerializerOptions jsonOptions = new();
        jsonOptions.WriteIndented = !noPrettyPrint;

        _console.WriteLine("Saving indexes...");

        await using FileStream metadata2Stream = File.Open
        (
            Path.Combine(outputDirectory, "_Metadata.pack2.json"),
            FileMode.Create
        );
        await JsonSerializer.SerializeAsync(metadata2Stream, pack2Metadata, jsonOptions, _ct).ConfigureAwait(false);

        foreach (PackIndex index2 in pack2Indexes)
        {
            string fileName = Path.GetFileName(index2.Path);
            await using FileStream index2Stream = File.Open
            (
                Path.Combine(outputDirectory, fileName + ".json"),
                FileMode.Create
            );
            await JsonSerializer.SerializeAsync(index2Stream, index2, jsonOptions, _ct).ConfigureAwait(false);
        }

        _console.Markup("[green]Indexing Complete![/]");
    }

    private async Task<List<PackIndex>> BuildIndex2Async(IReadOnlyList<string> pack2Files, Namelist namelist)
        => await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask indexTask = ctx.AddTask("Building pack2 indexes...");

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
            )
            .ConfigureAwait(false);
}
