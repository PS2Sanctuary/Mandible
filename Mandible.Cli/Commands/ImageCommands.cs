using ConsoleAppFramework;
using Mandible.Cli.Util;
using Mandible.Gnf;
using Mandible.Services;
using Spectre.Console;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

/// <summary>
/// Commands for manipulating image files.
/// </summary>
public class ImageCommands
{
    private readonly IAnsiConsole _console;

    public ImageCommands(IAnsiConsole console)
    {
        _console = console;
    }

    /// <summary>
    /// Converts each texture in a GNF image to a DDS file.
    /// </summary>
    /// <param name="inputPath">A path to the GNF file to convert.</param>
    /// <param name="outputPath">
    /// -o|--output, The name of the output files. If not specified, the name of the GNF file will be used.
    /// </param>
    /// <param name="force">-f, Force overwrite of any existing index files.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("gnf convert")]
    public async Task GnfConvert
    (
        [Argument] string inputPath,
        string? outputPath = null,
        bool force = false,
        CancellationToken ct = default
    )
    {
        if (!File.Exists(inputPath))
        {
            _console.MarkupLine($"[red]The input file does not exist ({inputPath})[/]");
            return;
        }

        outputPath ??= inputPath;
        CommandUtils.CheckOutputDirectory(_console, Path.GetDirectoryName(outputPath)!);

        using RandomAccessDataReaderService radrs = new(inputPath);
        GnfImage gnf = new(radrs);

        for (int i = 0; i < gnf.Textures.Count; i++)
        {
            // Don't include the _Texture suffix if there's only one texture
            string outPath = gnf.Textures.Count == 1
                ? Path.ChangeExtension(outputPath, "dds")
                : $"{Path.ChangeExtension(outputPath, null)}_Texture{i}.dds";

            using RandomAccessDataWriterService radws = new(outPath, force ? FileMode.Create : FileMode.CreateNew);
            await GnfConverter.ToDds(gnf, i, radws, ct);

            _console.MarkupLine($"[green]Output GNF texture to {outPath}[/]");
        }
    }
}
