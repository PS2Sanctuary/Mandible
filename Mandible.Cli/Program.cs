using Mandible.Pack2;
using Microsoft.Win32.SafeHandles;

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

            Pack2Header header = await reader.ReadHeaderAsync(ct).ConfigureAwait(false);
            Console.WriteLine("Header: ");
            Console.WriteLine("\t- Asset Count: {0}", header.AssetCount);
            Console.WriteLine("\t- Packet Length: {0}", header.Length);

            IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

            Asset2Header unzippedAsset = assetHeaders.First(h => h.ZipStatus is AssetZipDefinition.Unzipped or AssetZipDefinition.UnzippedAlternate);
            Asset2Header zippedAsset = assetHeaders.First(h => h.ZipStatus is AssetZipDefinition.Zipped or AssetZipDefinition.ZippedAlternate);

            using SafeFileHandle unzippedHandle = File.OpenHandle(
                Path.Combine(args[1], unzippedAsset.NameHash.ToString()),
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                FileOptions.Asynchronous
            );

            using SafeFileHandle zippedHandle = File.OpenHandle(
                Path.Combine(args[1],
                zippedAsset.NameHash.ToString()),
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                FileOptions.Asynchronous
            );

            Console.WriteLine("Reading assets...");
            ReadOnlyMemory<byte> unzippedData = await reader.ReadAssetData(unzippedAsset, ct).ConfigureAwait(false);
            ReadOnlyMemory<byte> zippedData = await reader.ReadAssetData(zippedAsset, ct).ConfigureAwait(false);

            Console.WriteLine("Writing assets...");
            await RandomAccess.WriteAsync(unzippedHandle, unzippedData, 0, ct).ConfigureAwait(false);
            await RandomAccess.WriteAsync(zippedHandle, zippedData, 0, ct).ConfigureAwait(false);
        }
    }
}
