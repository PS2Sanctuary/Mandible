using ConsoleAppFramework;
using Mandible.Cli.Util;
using Mandible.Pack;
using Mandible.Pack2;
using Mandible.Pack2.Names;
using Mandible.Services;
using Spectre.Console;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// <param name="filter">
    /// -f|--search-pattern, A Windows file search pattern to use when enumerating packs should a directory path be provided.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("")]
    public async Task ExecuteAsync
    (
        [Argument] string inputPath,
        [Argument] string outputDirectory,
        string? namelistPath,
        string filter = "*",
        CancellationToken ct = default
    )
    {
        bool packsDiscovered = CommandUtils.TryFindPacksFromPath
        (
            _console,
            inputPath,
            out List<string> packFiles,
            out List<string> pack2Files,
            filter
        );

        if (!packsDiscovered)
            return;

        if (!CommandUtils.CheckOutputDirectory(_console, outputDirectory))
            return;

        Namelist? namelist = null;
        if (!string.IsNullOrEmpty(namelistPath) && pack2Files.Count > 0)
            namelist = await CommandUtils.BuildNamelistAsync(_console, namelistPath, ct);

        await ExportPackAssetsAsync(packFiles, pack2Files, outputDirectory, namelist, ct);

        _console.Markup("[green]Unpacking Complete![/]");
    }

    private async Task ExportPackAssetsAsync
    (
        List<string> packFiles,
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
                    ProgressTask exportTask = ctx.AddTask("Exporting pack assets");
                    double increment = exportTask.MaxValue / (packFiles.Count + pack2Files.Count);

                    foreach (string file in packFiles.Concat(pack2Files))
                    {
                        exportTask.Description = $"Exporting [cyan]{Path.GetFileName(file)}[/]";

                        using RandomAccessDataReaderService dataReader = new(file);
                        string myOutputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file));

                        if (!Directory.Exists(myOutputPath))
                            Directory.CreateDirectory(myOutputPath);

                        if (file.EndsWith(".pack2"))
                        {
                            using Pack2Reader reader = new(dataReader);
                            await reader.ExportAllAsync
                            (
                                myOutputPath,
                                namelist,
                                inferFileExtension: true,
                                excludeUnnamed: false,
                                ct: ct
                            );
                        }
                        else
                        {
                            PackReader reader = new(dataReader);
                            await reader.ExportAllAsync(myOutputPath, ct);
                        }

                        exportTask.Increment(increment);
                    }
                }
            );
}
