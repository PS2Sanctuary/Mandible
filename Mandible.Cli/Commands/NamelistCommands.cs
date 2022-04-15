using CommandDotNet;
using Mandible.Cli.Util;
using Mandible.Pack2.Names;
using Spectre.Console;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

[Command("namelist", Description = "Provides subcommands to help build namelists.")]
public class NamelistCommands
{
    private readonly IAnsiConsole _console;
    private readonly CancellationToken _ct;

    public NamelistCommands(IAnsiConsole console, CancellationToken ct)
    {
        _console = console;
        _ct = ct;
    }

    [Command("extract", Description = "Extracts names from a collection of pack2 files.")]
    public async Task ExtractAsync
    (
        [Operand(Description = "The directory containing the pack2 files to extract names from.")]
        string pack2Directory,

        [Operand(Description = "The path to output the namelist file to.")]
        string output,

        [Option('d', Description = "Performs a deep search for names. This takes considerably longer but captures ~3% more names.")]
        bool deepSearch = false
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
                    Namelist extractedNamelist = await NameExtractor.ExtractAsync(pack2Directory, deepSearch, _ct).ConfigureAwait(false);

                    await using FileStream nlOut = new(output, FileMode.Create);
                    await extractedNamelist.WriteAsync(nlOut, _ct).ConfigureAwait(false);
                }
            );

        _console.MarkupLine($"Namelist extracted to [cyan]{output}[/]");
        _console.MarkupLine("[green]Extraction complete![/]");
    }

    [Command("merge", Description = "Merges namelist files together.")]
    public async Task MergeAsync
    (
        [Operand(Description = "The path to output the merged namelist to.")]
        string output,

        [Operand(Description = "The namelist files to merge.")]
        IEnumerable<string> namelistPaths
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

                        Namelist nl = await Namelist.FromFileAsync(path, _ct).ConfigureAwait(false);
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
                    await mergedNamelist.WriteAsync(nlOut, _ct).ConfigureAwait(false);
                }
            );

        _console.MarkupLine("[green]Merge Complete![/]");
    }

    [Command("ps2ls2-convert", Description = "Converts a namelist to a format compatible with PS2LS2")]
    public async Task PS2LS2ConvertAsync
    (
        [Operand(Description = "The namelist to convert")]
        string namelist,

        [Operand(Description = "The output namelist file")]
        string output
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
                    Namelist nl = await Namelist.FromFileAsync(namelist, _ct);

                    await using FileStream fsOut = new(output, FileMode.Create, FileAccess.Write);
                    await using StreamWriter sw = new(fsOut);

                    foreach ((ulong hash, string name) in nl.Map)
                        await sw.WriteLineAsync($"{hash}:{name}");
                }
            );

        _console.MarkupLine("[green]Conversion Complete![/]");
    }
}
