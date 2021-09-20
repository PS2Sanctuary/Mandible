using Mandible.Pack2;
using Mandible.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new();
            CancellationToken ct = cts.Token;
            Console.CancelKeyPress += (_, __) => cts.Cancel();

            if (args.Length != 3)
            {
                Console.WriteLine("Usage: <packFolderPath> <outputFolderPath> <namelistPath>");
                return;
            }

            if (!Directory.Exists(args[0]))
                throw new DirectoryNotFoundException("Pack folder directory does not exist: " + args[0]);

            if (!Directory.Exists(args[1]))
                throw new DirectoryNotFoundException("Output directory does not exist: " + args[1]);

            if (!File.Exists(args[2]))
                throw new FileNotFoundException("Namelist file does not exist: " + args[2]);

            Stopwatch stopwatch = new();
            stopwatch.Start();

            string[] namelist = await File.ReadAllLinesAsync(args[2], ct).ConfigureAwait(false);
            Dictionary<ulong, string> hashedNamePairs = PackCrc.HashStrings64(namelist);

            stopwatch.Stop();
            Console.WriteLine("Generated name hashes in {0}", stopwatch.Elapsed);

            IEnumerable<string> packFiles = Directory.EnumerateFiles(args[0]);

            stopwatch.Reset();
            stopwatch.Start();

            foreach (string file in packFiles)
            {
                await ExportPackAssetsAsync(file, args[1], hashedNamePairs, ct).ConfigureAwait(false);
            }

            stopwatch.Stop();
            Console.WriteLine("Wrote all assets in {0}", stopwatch.Elapsed);
        }

        private static async ValueTask ExportPackAssetsAsync(string packFilePath, string outputPath, Dictionary<ulong, string> hashedNamePairs, CancellationToken ct = default)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            Console.WriteLine("Exporting {0}", packFilePath);

            using Pack2Reader reader = new(packFilePath);
            outputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(packFilePath));

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            // await reader.ExportAllAsync(outputPath, hashedNamePairs, ct).ConfigureAwait(false);
            reader.ExportAll(outputPath, hashedNamePairs); // TODO: Test async

            stopwatch.Stop();
            Console.WriteLine("Completed exporting in {0}", stopwatch.Elapsed);
        }

        private static async Task PrintPackHeader(Pack2Reader reader, CancellationToken ct = default)
        {
            Pack2Header header = await reader.ReadHeaderAsync(ct).ConfigureAwait(false);
            Console.WriteLine("Header: ");
            Console.WriteLine("\t- Asset Count: {0}", header.AssetCount);
            Console.WriteLine("\t- Packet Length: {0}", header.Length);
        }

        private static async Task WriteAmerishTileAssets(Pack2Reader reader, string outputPath, CancellationToken ct = default)
        {
            Dictionary<ulong, string> tileNames = GetTileNames();
            await reader.ExportNamedAsync(outputPath, tileNames, ct).ConfigureAwait(false);
        }

        private static Dictionary<ulong, string> GetTileNames()
        {
            Dictionary<ulong, string> tileNames = new();

            for (int i = -64; i < 64; i += 16)
            {
                for (int j = -64; j < 64; j += 16)
                {
                    string name = $"Amerish_Tile_{ GetNumberString(i) }_{ GetNumberString(j) }_LOD2.dds";
                    ulong hash = PackCrc.Calculate64(name);
                    tileNames.Add(hash, name);
                }
            }

            return tileNames;
        }

        private static string GetNumberString(int number)
            => number < 0 ? number.ToString("d2") : number.ToString("d3");
    }
}
