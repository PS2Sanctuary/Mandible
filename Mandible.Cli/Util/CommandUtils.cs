using Mandible.Pack2.Names;
using Spectre.Console;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Util;

public static class CommandUtils
{
    /// <summary>
    /// Ensures that an output directory can be written to.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="directoryPath">The directory path.</param>
    /// <returns>A value indicating whether the directory can be written to.</returns>
    public static bool CheckOutputDirectory(IAnsiConsole console, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            if (!console.Confirm("The output directory does not exist. Would you like to create it?"))
                return false;

            Directory.CreateDirectory(directoryPath);
        }

        return true;
    }

    /// <summary>
    /// Attempts to find any pack/pack2 files on the given path.
    /// </summary>
    /// <param name="console">The console to log error messages to.</param>
    /// <param name="path">The path to search.</param>
    /// <param name="packPaths">A list to append any discovered pack file paths to.</param>
    /// <param name="pack2Paths">A list to append any discovered pack2 file paths to.</param>
    /// <returns>A value indicating whether any pack/pack2 files were found.</returns>
    public static bool TryFindPacksFromPath(IAnsiConsole console, string path, out List<string> packPaths, out List<string> pack2Paths)
    {
        packPaths = new List<string>();
        pack2Paths = new List<string>();

        if (!File.Exists(path) && !Directory.Exists(path))
        {
            console.MarkupLine("[red]The input path does not exist.[/]");
            return false;
        }

        if (File.Exists(path))
        {
            switch (Path.GetExtension(path))
            {
                case ".pack":
                    packPaths.Add(path);
                    break;
                case ".pack2":
                    pack2Paths.Add(path);
                    break;
                default:
                    console.MarkupLine("[red]The input file was not a pack/pack2.[/]");
                    return false;
            }
        }

        if (Directory.Exists(path))
        {
            foreach (string pack in Directory.EnumerateFiles(path, "*.pack"))
                packPaths.Add(pack);

            foreach (string pack in Directory.EnumerateFiles(path, "*.pack2"))
                pack2Paths.Add(pack);

            if (packPaths.Count == 0 && pack2Paths.Count == 0)
            {
                console.MarkupLine("[red]No pack/pack2 files were found in the input directory.[/]");
                return false;
            }
        }

        packPaths.Sort();
        pack2Paths.Sort();

        return true;
    }

    /// <summary>
    /// Builds a namelist and prints a status line to the console.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="namelistPath">The path to the namelist file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The built namelist.</returns>
    public static async Task<Namelist> BuildNamelistAsync(IAnsiConsole console, string namelistPath, CancellationToken ct)
    {
        Namelist nl = await console.Status()
            .StartAsync
            (
                "Building namelist",
                async _ => await Namelist.FromFileAsync(namelistPath, ct).ConfigureAwait(false)
            )
            .ConfigureAwait(false);

        console.MarkupLine("[green]Namelist build complete![/]");
        return nl;
    }
}
