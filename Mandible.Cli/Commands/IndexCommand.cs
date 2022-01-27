using CommandDotNet;
using Mandible.Cli.Objects;
using Mandible.Cli.Util;
using Mandible.Pack2;
using Mandible.Pack2.Names;
using Mandible.Services;
using Spectre.Console;
using System;
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

        [Operand(Description = "The path to output the index to.")]
        string outputPath,

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

        string pack2OutputPath = Path.ChangeExtension(outputPath, ".pack2.json");
        if (File.Exists(pack2OutputPath) && !_console.Confirm("The output file already exists. Would you like to override it?"))
            return;

        string packOutputPath = Path.ChangeExtension(outputPath, ".pack.json");
        if (File.Exists(packOutputPath) && !_console.Confirm("The output file already exists. Would you like to override it?"))
            return;

        Namelist namelist = await CommandUtils.BuildNamelistAsync(_console, namelistPath, _ct).ConfigureAwait(false);
        Objects.Index index2 = await BuildIndex2Async(pack2Files, namelist).ConfigureAwait(false);

        JsonSerializerOptions jsonOptions = new();
        jsonOptions.WriteIndented = !noPrettyPrint;

        _console.WriteLine($"Saving pack2 index to {pack2OutputPath}...");
        await using FileStream fs = File.Open(pack2OutputPath, FileMode.Create);
        await JsonSerializer.SerializeAsync(fs, index2, jsonOptions, _ct).ConfigureAwait(false);
        _console.Markup("[green]Pack2 Index Complete![/]");
    }

    // TODO: Build diff command

    private async Task<Objects.Index> BuildIndex2Async(IReadOnlyList<string> pack2Files, Namelist namelist)
        => await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask indexTask = ctx.AddTask("Building pack2 index...");

                    List<Objects.Index.Index2Pack> packIndexes = new();
                    double increment = indexTask.MaxValue / pack2Files.Count;

                    foreach (string file in pack2Files)
                    {
                        using RandomAccessDataReaderService dataReader = new(file);
                        using Pack2Reader reader = new(dataReader);

                        Pack2Header header = await reader.ReadHeaderAsync(_ct).ConfigureAwait(false);
                        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(_ct).ConfigureAwait(false);

                        IEnumerable<Index.IndexAsset> assets = assetHeaders
                            .Select(s => Index.IndexAsset.FromAsset2Header(s, namelist))
                            .OrderBy(a => a.Name);

                        packIndexes.Add
                        (
                            new Objects.Index.Index2Pack
                            (
                                Path.GetFullPath(file),
                                header,
                                Enumerable.ToList<Objects.Index.IndexAsset>(assets)
                            )
                        );

                        indexTask.Increment(increment);
                    }

                    return new Index2(DateTimeOffset.UtcNow, packIndexes);
                }
            )
            .ConfigureAwait(false);
}
