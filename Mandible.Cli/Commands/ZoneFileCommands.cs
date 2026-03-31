using ConsoleAppFramework;
using Spectre.Console;
using System;
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

        await ExportAsJsonInternal(zoneFilePath, outputPath, ct);

        _console.MarkupLine($"Zone file exported to [cyan]{outputPath}[/]");
        _console.MarkupLine("[green]Export complete![/]");
    }

    /// <summary>
    /// Loads a JSON representation of a Zone file and writes it to the binary format.
    /// </summary>
    /// <param name="inputJsonPath">A path to the input JSON file.</param>
    /// <param name="outputZonePath">A path to the output zone file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("write")]
    public async Task WriteFromJson
    (
        [Argument] string inputJsonPath,
        [Argument] string outputZonePath,
        CancellationToken ct = default
    )
    {
        if (!File.Exists(inputJsonPath))
        {
            _console.MarkupLine("[red]The input JSON file does not exist.[/]");
            return;
        }

        if (File.Exists(outputZonePath))
        {
            if (!_console.Confirm("[red]The output file already exists.[/] Would you like to overwrite it?"))
                return;
        }

        await WriteFromJsonInternal(inputJsonPath, outputZonePath, ct);

        _console.MarkupLine($"Zone file written to [cyan]{outputZonePath}[/]");
        _console.MarkupLine("[green]Complete![/]");
    }

    /// <summary>
    /// Loads a binary zone file, exports to JSON, and attempts to re-write to the binary format. Returns whether the
    /// written output is identical. This provides a means of round-trip testing the tool's understanding of the zone
    /// file format.
    /// </summary>
    /// <param name="zoneFilePath">A path to the binary zone file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("test")]
    public async Task RoundTripTest
    (
        [Argument] string zoneFilePath,
        CancellationToken ct = default
    )
    {
        string tempJson = Path.GetTempFileName();
        string tempZone = Path.GetTempFileName();

        try
        {
            await ExportAsJsonInternal(zoneFilePath, tempJson, ct);
            await WriteFromJsonInternal(tempJson, tempZone, ct);

            byte[] expected = await File.ReadAllBytesAsync(zoneFilePath, ct);
            byte[] actual = await File.ReadAllBytesAsync(tempZone, ct);

            if (expected.SequenceEqual(actual))
                _console.MarkupLine("[green]Success! The round-trip output matches the input zone file[/]");
            else
                _console.MarkupLine("[red]Failure! The round-trip output does not match the input zone file[/]");
        }
        finally
        {
            File.Delete(tempJson);
            File.Delete(tempZone);
        }
    }

    private static async Task ExportAsJsonInternal
    (
        [Argument] string zoneFilePath,
        [Argument] string outputPath,
        CancellationToken ct = default
    )
    {
        byte[] data = await File.ReadAllBytesAsync(zoneFilePath, ct);
        Zone.Zone zone = Zone.Zone.Read(data, out _);

        await using FileStream fs = new(outputPath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(fs, zone, AppJsonContext.Default.Zone, ct);
    }

    private async Task WriteFromJsonInternal
    (
        [Argument] string inputJsonPath,
        [Argument] string outputZonePath,
        CancellationToken ct = default
    )
    {
        await using FileStream fs = new(inputJsonPath, FileMode.Open, FileAccess.Read,  FileShare.ReadWrite);
        Zone.Zone? zone = await JsonSerializer.DeserializeAsync<Zone.Zone>(fs, AppJsonContext.Default.Zone, ct);

        if (zone is null)
        {
            _console.MarkupLine("[red]Failed to load Zone data from the JSON file.[/]");
            return;
        }

        byte[] buffer = new byte[zone.GetRequiredBufferSize()];
        zone.Write(buffer);
        await File.WriteAllBytesAsync(outputZonePath, buffer, ct);
    }
}
