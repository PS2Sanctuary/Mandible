using Mandible.Pack2;
using Mandible.Pack2.Names;
using Mandible.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZlibNGSharpMinimal;

namespace Mandible.Cli;

public static class Program
{
    public static async Task Main(string[] args)
    {
        CancellationTokenSource cts = new();
        CancellationToken ct = cts.Token;
        Console.CancelKeyPress += (_, __) => cts.Cancel();

        Console.WriteLine("zlib-ng version: {0}", Zng.Version);

        if (args.Length != 3)
        {
            Console.WriteLine("Usage: <packFolderPath> <outputFolderPath> <namelistPath>");
            return;
        }

        if (!Directory.Exists(args[0]))
            throw new DirectoryNotFoundException("Pack folder directory does not exist: " + args[0]);

        if (!Directory.Exists(args[1]))
            throw new DirectoryNotFoundException("Output directory does not exist: " + args[1]);

        Stopwatch stopwatch = new();
        stopwatch.Start();

        Namelist namelist = await Namelist.FromFileAsync(args[2], ct).ConfigureAwait(false);
        //await ExtractNewNamelist
        //(
        //    namelist,
        //    args[0],
        //    Path.Combine(Path.GetDirectoryName(args[2])!, "extracted-namelist.txt"),
        //    ct
        //).ConfigureAwait(false);

        stopwatch.Stop();
        Console.WriteLine("Generated namelist in {0}", stopwatch.Elapsed);

        // return;

        IEnumerable<string> packFiles = Directory.EnumerateFiles(args[0], "*.pack2", SearchOption.TopDirectoryOnly);

        stopwatch.Reset();
        stopwatch.Start();

        foreach (string file in packFiles)
        {
            await ExportPackAssetsAsync
            (
                file,
                args[1],
                namelist,
                ct
            ).ConfigureAwait(false);
        }

        stopwatch.Stop();
        Console.WriteLine("Wrote all assets in {0}", stopwatch.Elapsed);
    }

    private static async Task ExportPackAssetsAsync
    (
        string packFilePath,
        string outputPath,
        Namelist namelist,
        CancellationToken ct
    )
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        Console.WriteLine("Exporting {0}", packFilePath);

        using RandomAccessDataReaderService dataReader = new(packFilePath);
        using Pack2Reader reader = new(dataReader);
        outputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(packFilePath));

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        await reader.ExportAllAsync(outputPath, namelist, ct).ConfigureAwait(false);

        stopwatch.Stop();
        Console.WriteLine("Completed exporting in {0}", stopwatch.Elapsed);
    }

    private static async Task ExtractNewNamelist
    (
        Namelist existing,
        string packDirectoryPath,
        string outputPath,
        CancellationToken ct
    )
    {
        Namelist extractedNamelist = await NameExtractor.ExtractAsync(packDirectoryPath, true, ct: ct).ConfigureAwait(false);
        existing.Append(extractedNamelist);

        await using FileStream nlOut = new(outputPath, FileMode.Create);
        await existing.WriteAsync(nlOut, ct).ConfigureAwait(false);
    }

    private static async Task WriteAmerishLod2TileAssets(Pack2Reader reader, string outputPath, CancellationToken ct = default)
    {
        Namelist namelist = GenerateTileNames("Amerish", "LOD2");
        await reader.ExportNamedAsync(outputPath, namelist, ct).ConfigureAwait(false);
    }

    private static Namelist GenerateTileNames(string continent, string lod)
    {
        static string GetTileCoordinateString(int number)
            => number < 0 ? number.ToString("d2") : number.ToString("d3");

        Namelist namelist = new();

        for (int i = -64; i < 64; i += 16)
        {
            for (int j = -64; j < 64; j += 16)
            {
                string name = $"{continent}_Tile_{GetTileCoordinateString(i)}_{GetTileCoordinateString(j)}_{lod.ToUpper()}.dds";
                namelist.Append(name);
            }
        }

        return namelist;
    }
}
