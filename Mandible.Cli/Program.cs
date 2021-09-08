using Mandible.Pack2;
using Mandible.Util;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Linq;

namespace Mandible.Cli
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new();
            CancellationToken ct = cts.Token;
            Console.CancelKeyPress += (_, __) => cts.Cancel();

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: <packFilePath> <outputFolder>");
                return;
            }

            using Pack2Reader reader = new(args[0]);

            await PrintPackHeader(reader, ct).ConfigureAwait(false);

            Stopwatch stopwatch = new();
            stopwatch.Start();

            await WriteAssets(reader, args[1], ct).ConfigureAwait(false);

            stopwatch.Stop();
            Console.WriteLine("Wrote assets in {0}", stopwatch.Elapsed);

            Console.WriteLine("Done!");
        }

        private static async Task PrintPackHeader(Pack2Reader reader, CancellationToken ct = default)
        {
            Pack2Header header = await reader.ReadHeaderAsync(ct).ConfigureAwait(false);
            Console.WriteLine("Header: ");
            Console.WriteLine("\t- Asset Count: {0}", header.AssetCount);
            Console.WriteLine("\t- Packet Length: {0}", header.Length);
        }

        private static async Task WriteAssets(Pack2Reader reader, string outputPath, CancellationToken ct = default)
        {
            Dictionary<ulong, string> tileNames = GetTileNames();
            await reader.ExportNamed(outputPath, tileNames, ct).ConfigureAwait(false);
        }

        private static Dictionary<ulong, string> GetTileNames()
        {
            Dictionary<ulong, string> tileNames = new();

            for (int i = -64; i < 64; i += 16)
            {
                for (int j = -64; j < 64; j += 16)
                {
                    string name = $"Amerish_Tile_{ GetNumberString(i) }_{ GetNumberString(j) }_LOD2.dds";
                    ulong hash = PackCrc64.Calculate(name);
                    tileNames.Add(hash, name);
                }
            }

            return tileNames;
        }

        private static string GetNumberString(int number)
            => number < 0 ? number.ToString("d2") : number.ToString("d3");
    }
}
