using ConsoleAppFramework;
using Mandible.Cli.Util;
using Mandible.Pack2.Names;
using Spectre.Console;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

/// <summary>
/// Provides subcommands to help build namelists.
/// </summary>
public class NamelistCommands
{
    private readonly IAnsiConsole _console;

    public NamelistCommands(IAnsiConsole console)
    {
        _console = console;
    }

    /// <summary>
    /// Extracts names from a collection of pack2 files.
    /// </summary>
    /// <param name="pack2Directory">The directory containing the pack2 files to extract names from.</param>
    /// <param name="output">The path to output the namelist file to.</param>
    /// <param name="deepSearch">
    /// -d|--deep, Performs a deep search for names. This takes considerably longer but captures ~3% more names.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("extract")]
    public async Task ExtractAsync
    (
        [Argument] string pack2Directory,
        [Argument] string output,
        bool deepSearch = false,
        CancellationToken ct = default
    )
    {
        if (!CommandUtils.TryFindPacksFromPath(_console, pack2Directory, out _, out _))
            return;

        output = Path.ChangeExtension(output, ".txt");
        if (File.Exists(output))
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        await _console.Status()
            .StartAsync
            (
                $"Extracting namelist with deep search {(deepSearch ? "on" : "off")}...",
                async _ =>
                {
                    Namelist extractedNamelist = await NameExtractor.ExtractAsync(pack2Directory, deepSearch, ct);
                    extractedNamelist.ToUpperCaseNames();

                    await using FileStream nlOut = new(output, FileMode.Create);
                    await extractedNamelist.WriteAsync(nlOut, ct);
                }
            );

        _console.MarkupLine($"Namelist extracted to [cyan]{output}[/]");
        _console.MarkupLine("[green]Extraction complete![/]");
    }

    /// <summary>
    /// Merges namelist files together.
    /// </summary>
    /// <param name="output">The path to output the merged namelist to.</param>
    /// <param name="namelistPaths">The namelist files to merge.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("merge")]
    public async Task MergeAsync
    (
        [Argument] string output,
        CancellationToken ct = default,
        [Argument] params string[] namelistPaths
    )
    {
        output = Path.ChangeExtension(output, ".txt");
        if (File.Exists(output))
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        Namelist mergedNamelist = new();
        List<string> namelistPathsList = namelistPaths.ToList();

        await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask mergeTask = ctx.AddTask("Merging...");
                    double increment = mergeTask.MaxValue / namelistPathsList.Count;

                    foreach (string path in namelistPathsList)
                    {
                        if (!File.Exists(path))
                            continue;

                        Namelist nl = await Namelist.FromFileAsync(path, ct);
                        mergedNamelist.Append(nl);

                        mergeTask.Increment(increment);
                    }
                }
            );

        await _console.Status()
            .StartAsync
            (
                "Saving...",
                async _ =>
                {
                    await using FileStream nlOut = new(output, FileMode.Create);
                    await mergedNamelist.WriteAsync(nlOut, ct);
                }
            );

        _console.MarkupLine("[green]Merge Complete![/]");
    }

    /// <summary>
    /// Converts a namelist to a format compatible with PS2LS2.
    /// </summary>
    /// <param name="namelist">The namelist to convert.</param>
    /// <param name="output">The output namelist file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("ps2ls2-convert")]
    public async Task PS2LS2ConvertAsync
    (
        [Argument] string namelist,
        [Argument] string output,
        CancellationToken ct = default
    )
    {
        output = Path.ChangeExtension(output, ".txt");
        if (File.Exists(output))
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        if (!File.Exists(namelist))
        {
            _console.Markup("[red]The input namelist does not exist.[/]");
            return;
        }

        await _console.Status()
            .StartAsync
            (
                "Converting...",
                async _ =>
                {
                    Namelist nl = await Namelist.FromFileAsync(namelist, ct);

                    await using FileStream fsOut = new(output, FileMode.Create, FileAccess.Write);
                    await using StreamWriter sw = new(fsOut);

                    foreach ((ulong hash, string name) in nl.Map)
                        await sw.WriteLineAsync($"{hash}:{name}");
                }
            );

        _console.MarkupLine("[green]Conversion Complete![/]");
    }
}
