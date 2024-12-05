using CommandDotNet;
using Mandible.Cli.Util;
using Mandible.Pack;
using Mandible.Pack2;
using Mandible.Pack2.Names;
using Mandible.Services;
using Spectre.Console;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

[Command("unpack", Description = "Unpacks pack/pack2 file/s")]
public class UnpackCommands
{
    private readonly IAnsiConsole _console;
    private readonly CancellationToken _ct;

    public UnpackCommands(IAnsiConsole console, CancellationToken ct)
    {
        _console = console;
        _ct = ct;
    }

    [DefaultCommand]
    public async Task ExecuteAsync
    (
        [Operand(Description = "A path to a single pack/pack2 file, or a directory containing multiple.")]
        string inputPath,

        [Operand(Description = "The directory to output the packed content to. Contents will be nested in directories matching the name of the pack they originated from.")]
        string outputDirectory,

        [Option('n', Description = "A path to a namelist file.")]
        string? namelistPath
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

        Namelist? namelist = null;
        if (namelistPath is not null)
            namelist = await CommandUtils.BuildNamelistAsync(_console, namelistPath, _ct).ConfigureAwait(false);

        if (packFiles.Count > 0)
            await ExportPackAssetsAsync(packFiles, outputDirectory);

        if (pack2Files.Count > 0)
            await ExportPack2AssetsAsync(pack2Files, outputDirectory, namelist).ConfigureAwait(false);

        _console.Markup("[green]Unpacking Complete![/]");
    }

    private async Task ExportPackAssetsAsync
    (
        IReadOnlyList<string> packFiles,
        string outputPath
    )
        => await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask exportTask = ctx.AddTask("Exporting pack assets...");
                    double increment = exportTask.MaxValue / packFiles.Count;

                    foreach (string file in packFiles)
                    {
                        using RandomAccessDataReaderService dataReader = new(file);
                        PackReader reader = new(dataReader);
                        string myOutputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file));

                        if (!Directory.Exists(myOutputPath))
                            Directory.CreateDirectory(myOutputPath);

                        await reader.ExportAllAsync(myOutputPath, _ct).ConfigureAwait(false);

                        exportTask.Increment(increment);
                    }
                }
            );

    private async Task ExportPack2AssetsAsync
    (
        List<string> pack2Files,
        string outputPath,
        Namelist? namelist
    )
        => await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask exportTask = ctx.AddTask("Exporting pack2 assets...");
                    double increment = exportTask.MaxValue / pack2Files.Count;

                    foreach (string file in pack2Files)
                    {
                        using RandomAccessDataReaderService dataReader = new(file);
                        using Pack2Reader reader = new(dataReader);
                        string myOutputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file));

                        if (!Directory.Exists(myOutputPath))
                            Directory.CreateDirectory(myOutputPath);

                        await reader.ExportAllAsync(myOutputPath, namelist, _ct);

                        exportTask.Increment(increment);
                    }
                }
            );
}
