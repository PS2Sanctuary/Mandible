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
    /// <param name="filter">
    /// A Windows file search pattern to use when enumerating packs should a directory <paramref name="path"/> be
    /// provided.
    /// </param>
    /// <returns>A value indicating whether any pack/pack2 files were found.</returns>
    public static bool TryFindPacksFromPath
    (
        IAnsiConsole console,
        string path,
        out List<string> packPaths,
        out List<string> pack2Paths,
        string filter = "*"
    )
    {
        packPaths = new List<string>();
        pack2Paths = new List<string>();

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
                    console.MarkupLine($"[red]The file was not a pack/pack2[/] ({path})");
                    return false;
            }
        }
        else if (Directory.Exists(path))
        {
            console.MarkupLine($"Enumerating files matching the pattern [cyan]{filter}.pack(2)[/]");
            packPaths.AddRange(Directory.EnumerateFiles(path, $"{filter}.pack"));
            pack2Paths.AddRange(Directory.EnumerateFiles(path, $"{filter}.pack2"));

            if (packPaths.Count == 0 && pack2Paths.Count == 0)
            {
                console.MarkupLine($"[red]No pack/pack2 files were found in the input directory[/] ({path})");
                return false;
            }
        }
        else
        {
            console.MarkupLine("[red]Invalid search path.[/]");
            return false;
        }

        packPaths.Sort();
        pack2Paths.Sort();

        return true;
    }

    /// <summary>
    /// Attempts to find any pack/pack2 files on the given paths.
    /// </summary>
    /// <param name="console">The console to log error messages to.</param>
    /// <param name="paths">The paths to search.</param>
    /// <param name="packPaths">A list to append any discovered pack file paths to.</param>
    /// <param name="pack2Paths">A list to append any discovered pack2 file paths to.</param>
    /// <param name="filter">
    /// A Windows file search pattern to use when enumerating packs should a directory path be provided.
    /// </param>
    /// <returns>A value indicating whether any pack/pack2 files were found.</returns>
    public static bool TryFindPacksFromPaths
    (
        IAnsiConsole console,
        IEnumerable<string> paths,
        out List<string> packPaths,
        out List<string> pack2Paths,
        string filter = "*"
    )
    {
        packPaths = [];
        pack2Paths = [];

        foreach (string dir in paths)
        {
            if (!TryFindPacksFromPath(console, dir, out List<string> tempPaths, out List<string> temp2Paths, filter))
                continue;

            packPaths.AddRange(tempPaths);
            pack2Paths.AddRange(temp2Paths);
        }

        packPaths.Sort();
        pack2Paths.Sort();

        return packPaths.Count is not 0 || pack2Paths.Count is not 0;
    }

    /// <summary>
    /// Builds a namelist and prints a status line to the console.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="namelistPath">The path to the namelist file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The built namelist.</returns>
    public static async Task<Namelist> BuildNamelistAsync
    (
        IAnsiConsole console,
        string namelistPath,
        CancellationToken ct
    )
    {
        Namelist nl = await console.Status()
            .StartAsync
            (
                "Building namelist",
                async _ => await Namelist.FromFileAsync(namelistPath, ct)
            )
            .ConfigureAwait(false);

        console.MarkupLine("[green]Namelist build complete![/]");
        return nl;
    }

    /// <summary>
    /// Tries to build a namelist from the given path. If a file does not existing at the path, the user is notified
    /// and a null value is returned.
    /// </summary>
    /// <param name="console">The console to write to.</param>
    /// <param name="namelistPath">The path to a namelist file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    /// <returns>The built namelist, or <c>null</c> if the <paramref name="namelistPath"/> was invalid.</returns>
    public static async Task<Namelist?> TryBuildNamelist
    (
        IAnsiConsole console,
        string namelistPath,
        CancellationToken ct
    )
    {
        if (File.Exists(namelistPath))
        {
            return await BuildNamelistAsync(console, namelistPath, ct);
        }
        else
        {
            console.MarkupLine($"[red]The namelist path is invalid[/] ({namelistPath})");
            return null;
        }
    }
}
