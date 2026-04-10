using ConsoleAppFramework;
using Mandible.Cli.Util;
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
    /// <param name="output">The path to output the namelist file to.</param>
    /// <param name="existingNamelistPath">
    /// -n|--namelist, A path to an existing namelist file. It will be appended to the output, and used to speed up the
    /// extraction by preventing the need to extract files to determine their type (and hence ignore them).
    /// </param>
    /// <param name="force">-f, Force overwrite of the output file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    /// <param name="pack2Paths">At least one path to a pack2 file or a directory containing pack2 files.</param>
    [Command("extract")]
    public async Task ExtractAsync
    (
        [Argument] string output,
        string? existingNamelistPath = null,
        bool force = false,
        CancellationToken ct = default,
        [Argument] params string[] pack2Paths
    )
    {
        if (!CommandUtils.TryFindPacksFromPaths(_console, pack2Paths, out _, out List<string> pack2Files))
        {
            _console.MarkupLine("[red]No pack2 files were found, cannot extract a namelist![/]");
            return;
        }

        if (File.Exists(output) && !force)
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        Namelist? existingNl = string.IsNullOrEmpty(existingNamelistPath)
            ? null
            : await CommandUtils.TryBuildNamelist(_console, existingNamelistPath, ct);

        await _console.Status()
            .StartAsync
            (
                "Extracting namelist...",
                async _ =>
                {
                    Namelist extractedNamelist = await NameExtractor.ExtractAsync(pack2Files, existingNl, ct);
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
    /// <param name="force">-f, Force overwrite of the output file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("merge")]
    public async Task MergeAsync
    (
        [Argument] string output,
        bool force = false,
        CancellationToken ct = default,
        [Argument] params string[] namelistPaths
    )
    {
        if (File.Exists(output) && !force)
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
    /// <param name="force">-f, Force overwrite of the output file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("ps2ls2-convert")]
    public async Task PS2LS2ConvertAsync
    (
        [Argument] string namelist,
        [Argument] string output,
        bool force = false,
        CancellationToken ct = default
    )
    {
        if (File.Exists(output) && !force)
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        Namelist? nl = await CommandUtils.TryBuildNamelist(_console, namelist, ct);
        if (nl is null)
            return;

        await _console.Status()
            .StartAsync
            (
                "Converting...",
                async _ =>
                {
                    await using FileStream fsOut = new(output, FileMode.Create, FileAccess.Write);
                    await using StreamWriter sw = new(fsOut);

                    foreach ((ulong hash, string name) in nl.Map)
                        await sw.WriteLineAsync($"{hash}:{name}");
                }
            );

        _console.MarkupLine("[green]Conversion Complete![/]");
    }

    /// <summary>
    /// Trims a namelist by removing any names of assets which do not exist in the provided pack data.
    /// </summary>
    /// <param name="namelistPath">A path to the namelist to trim.</param>
    /// <param name="output">The path to output the namelist file to.</param>
    /// <param name="force">-f, Force overwrite of the output file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    /// <param name="pack2Paths">At least one path to a pack2 file or a directory containing pack2 files.</param>
    [Command("trim")]
    public async Task Trim
    (
        [Argument] string namelistPath,
        [Argument] string output,
        bool force = false,
        CancellationToken ct = default,
        [Argument] params string[] pack2Paths
    )
    {
        if (!CommandUtils.TryFindPacksFromPaths(_console, pack2Paths, out _, out List<string> pack2Files))
        {
            _console.MarkupLine("[red]No pack2 files were found, cannot trim![/]");
            return;
        }

        if (File.Exists(output) && !force)
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        Namelist? nl = await CommandUtils.TryBuildNamelist(_console, namelistPath, ct);
        if (nl is null)
            return;

        HashSet<ulong> knownHashes = [];
        Namelist outputNl = new();

        await _console.Status()
            .StartAsync
            (
                "Trimming namelist...",
                async _ =>
                {
                    // Retrieve all known hashes
                    foreach (string path in pack2Files)
                    {
                        using RandomAccessDataReaderService dr = new(path);
                        using Pack2Reader pr = new(dr);

                        IReadOnlyList<Asset2Header> headers = await pr.ReadAssetHeadersAsync(ct);
                        foreach (Asset2Header element in headers)
                            knownHashes.Add(element.NameHash);
                    }

                    // Check all hashes in the existing namelist against the known hashes
                    foreach ((ulong hash, string name) in nl.Map)
                    {
                        if (knownHashes.Contains(hash))
                            outputNl.Append(hash, name);
                    }

                    await using FileStream nlOut = new(output, FileMode.Create);
                    await outputNl.WriteAsync(nlOut, ct);
                }
            );

        _console.MarkupLine($"Namelist trimmed to [cyan]{output}[/]");
        _console.MarkupLine("[green]Trimming complete![/]");
    }
}
