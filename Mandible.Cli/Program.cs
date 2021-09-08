using Mandible.Pack2;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;

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
            IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

            foreach (Asset2Header assetHeader in assetHeaders)
            {
                using SafeFileHandle outputHandle = File.OpenHandle(
                    Path.Combine(outputPath, assetHeader.NameHash.ToString()),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read,
                    FileOptions.Asynchronous
                );

                Console.WriteLine("Reading asset data for {0}...", assetHeader.NameHash);
                ReadOnlyMemory<byte> assetData = await reader.ReadAssetData(assetHeader, ct).ConfigureAwait(false);

                Console.WriteLine("Writing asset data for {0}...", assetHeader.NameHash);
                await RandomAccess.WriteAsync(outputHandle, assetData, 0, ct).ConfigureAwait(false);
            }
        }
    }
}
