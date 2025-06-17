using ConsoleAppFramework;
using Mandible.Pack2;
using Mandible.Services;
using Spectre.Console;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

public class PackCommands
{
    private readonly IAnsiConsole _console;

    public PackCommands(IAnsiConsole console)
    {
        _console = console;
    }

    /// <summary>
    /// Creates a pack2 archive from files in the given input directory.
    /// </summary>
    /// <param name="inputDirectory">The directory containing files to pack.</param>
    /// <param name="outputPath">The path to write the generated pack2 file to.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("create-pack-2")]
    public async Task CreatePack2
    (
        [Argument] string inputDirectory,
        [Argument] string outputPath,
        CancellationToken ct = default
    )
    {
        if (File.Exists(outputPath))
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        using RandomAccessDataWriterService ioWriter = new(outputPath, FileMode.Create);
        await using Pack2Writer writer = new(ioWriter);
    }
}
