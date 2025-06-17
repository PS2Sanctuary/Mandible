using ConsoleAppFramework;
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

/// <summary>
/// Unpacks pack/pack2 file/s.
/// </summary>
public class UnpackCommands
{
    private readonly IAnsiConsole _console;

    public UnpackCommands(IAnsiConsole console)
    {
        _console = console;
    }

    /// <summary>
    /// Unpacks pack/pack2 file/s.
    /// </summary>
    /// <param name="inputPath">A path to a single pack/pack2 file, or a directory containing multiple.</param>
    /// <param name="outputDirectory">
    /// The directory to output the packed content to. Contents will be nested in directories matching the name of the
    /// pack they originated from.
    /// </param>
    /// <param name="namelistPath">-n|--namelist, A path to a namelist file</param>
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
            namelist = await CommandUtils.BuildNamelistAsync(_console, namelistPath, ct);

        if (packFiles.Count > 0)
            await ExportPackAssetsAsync(packFiles, outputDirectory, ct);

        if (pack2Files.Count > 0)
            await ExportPack2AssetsAsync(pack2Files, outputDirectory, namelist, ct);

        _console.Markup("[green]Unpacking Complete![/]");
    }

    private async Task ExportPackAssetsAsync
    (
        IReadOnlyList<string> packFiles,
        string outputPath,
        CancellationToken ct
    )
    {
        await _console.Progress()
            .StartAsync(async ctx =>
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

                    await reader.ExportAllAsync(myOutputPath, ct);

                    exportTask.Increment(increment);
                }
            });
    }

    private async Task ExportPack2AssetsAsync
    (
        List<string> pack2Files,
        string outputPath,
        Namelist? namelist,
        CancellationToken ct
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

                        await reader.ExportAllAsync(myOutputPath, namelist, ct);

                        exportTask.Increment(increment);
                    }
                }
            );
}
