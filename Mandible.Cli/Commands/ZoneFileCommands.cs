using ConsoleAppFramework;
using Spectre.Console;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

public class ZoneFileCommands
{
    private readonly IAnsiConsole _console;

    public ZoneFileCommands(IAnsiConsole console)
    {
        _console = console;
    }

    /// <summary>
    /// Retrieves basic information from a Zone file.
    /// </summary>
    /// <param name="zoneFilePath">A path to the zone file.</param>
    [Command("info")]
    public void GetInfo([Argument] string zoneFilePath)
    {
        if (!File.Exists(zoneFilePath))
        {
            _console.MarkupLine("[red]The zone file does not exist.[/]");
            return;
        }

        byte[] data = File.ReadAllBytes(zoneFilePath);
        Zone.Zone zone = Zone.Zone.Read(data, out _);
        
        _console.WriteLine($"Zone Info: {Path.GetFileName(zoneFilePath)}");
        _console.WriteLine("---");
        _console.WriteLine($"Version: {zone.Version}");
        _console.WriteLine($"Tile Quad Count: {zone.TileInfo.QuadCount}");
        _console.WriteLine($"Tile Width: {zone.TileInfo.Width}");
        _console.WriteLine($"Tile Height: {zone.TileInfo.Height}");
        _console.WriteLine($"Tile Vertex Count: {zone.TileInfo.VertexCount}");
        _console.WriteLine($"Chunk Tile Count: {zone.ChunkInfo.TileCount}");
        _console.WriteLine($"Chunk Start X Coordinate: {zone.ChunkInfo.StartX}");
        _console.WriteLine($"Chunk Start Y Coordinate: {zone.ChunkInfo.StartY}");
        _console.WriteLine($"Chunk Count X: {zone.ChunkInfo.CountX}");
        _console.WriteLine($"Chunk Count Y: {zone.ChunkInfo.CountY}");
        _console.WriteLine($"Eco Count: {zone.Ecos.Count}");
        _console.WriteLine($"Flora Count: {zone.Florae.Count}");
        _console.WriteLine($"Invisible Wall Count: {zone.InvisibleWalls.Count}");
        _console.WriteLine($"Object Count: {zone.Objects.Count}");
        _console.WriteLine($"Light Count: {zone.Lights.Count}");
    }

    /// <summary>
    /// Exports the contents of a Zone file to JSON.
    /// </summary>
    /// <param name="zoneFilePath">A path to the zone file.</param>
    /// <param name="outputPath">The file to export the JSON data into.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("json")]
    public async Task ExportAsJson
    (
        [Argument] string zoneFilePath,
        [Argument] string outputPath,
        CancellationToken ct = default
    )
    {
        if (!File.Exists(zoneFilePath))
        {
            _console.MarkupLine("[red]The zone file does not exist.[/]");
            return;
        }

        if (File.Exists(outputPath))
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        byte[] data = await File.ReadAllBytesAsync(zoneFilePath, ct);
        Zone.Zone zone = Zone.Zone.Read(data, out _);

        await using FileStream fs = new(outputPath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(fs, zone, AppJsonContext.Default.Zone, ct);
        
        _console.MarkupLine($"Zone file exported to [cyan]{outputPath}[/]");
        _console.MarkupLine("[green]Export complete![/]");
    }
}
