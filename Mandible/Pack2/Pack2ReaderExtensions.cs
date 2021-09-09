using Mandible.Util;
using Microsoft.Win32.SafeHandles;

namespace Mandible.Pack2
{
    public static class Pack2ReaderExtensions
    {
        /// <summary>
        /// Exports each asset in a pack.
        /// </summary>
        /// <param name="reader">The pack reader.</param>
        /// <param name="outputPath">The path to export the assets to.</param>
        /// <param name="hashedNamePairs">A mapping of CRC-64 hashes to their original file name strings, so the assets can be exported with sane file names.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchonous operation.</returns>
        public static async Task ExportAllAsync(this Pack2Reader reader, string outputPath, Dictionary<ulong, string> hashedNamePairs, CancellationToken ct = default)
        {
            if (!Directory.Exists(outputPath))
                throw new DirectoryNotFoundException(outputPath);

            IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

            foreach (Asset2Header assetHeader in assetHeaders)
            {
                string fileName = hashedNamePairs.ContainsKey(assetHeader.NameHash) ? hashedNamePairs[assetHeader.NameHash] : assetHeader.NameHash.ToString();

                using SafeFileHandle outputHandle = File.OpenHandle(
                    Path.Combine(outputPath, fileName),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read,
                    FileOptions.Asynchronous
                );

                ReadOnlyMemory<byte> assetData = await reader.ReadAssetDataAsync(assetHeader, ct).ConfigureAwait(false);
                await RandomAccess.WriteAsync(outputHandle, assetData, 0, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Exports each asset in a pack.
        /// </summary>
        /// <param name="reader">The pack reader.</param>
        /// <param name="outputPath">The path to export the assets to.</param>
        /// <param name="hashedNamePairs">A mapping of CRC-64 hashes to their original file name strings, so the assets can be exported with sane file names.</param>
        public static void ExportAll(this Pack2Reader reader, string outputPath, Dictionary<ulong, string> hashedNamePairs)
        {
            if (!Directory.Exists(outputPath))
                throw new DirectoryNotFoundException(outputPath);

            foreach (Asset2Header assetHeader in reader.ReadAssetHeaders())
            {
                string fileName = hashedNamePairs.ContainsKey(assetHeader.NameHash) ? hashedNamePairs[assetHeader.NameHash] : assetHeader.NameHash.ToString();

                using SafeFileHandle outputHandle = File.OpenHandle(
                    Path.Combine(outputPath, fileName),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read,
                    FileOptions.Asynchronous
                );

                ReadOnlySpan<byte> assetData = reader.ReadAssetData(assetHeader);
                RandomAccess.Write(outputHandle, assetData, 0);
            }
        }

        /// <summary>
        /// Exports each asset in a pack.
        /// </summary>
        /// <param name="reader">The pack reader.</param>
        /// <param name="outputPath">The path to export the assets to.</param>
        /// <param name="nameList">An optional namelist so the assets can be exported with sane file names.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchonous operation.</returns>
        public static async Task ExportAllAsync(this Pack2Reader reader, string outputPath, IEnumerable<string>? nameList = null, CancellationToken ct = default)
        {
            Dictionary<ulong, string> hashedNamePairs = nameList is null ? new() : PackCrc64.HashStrings(nameList);
            await ExportAllAsync(reader, outputPath, hashedNamePairs, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Exports each asset in a pack.
        /// </summary>
        /// <param name="reader">The pack reader.</param>
        /// <param name="outputPath">The path to export the assets to.</param>
        /// <param name="nameList">An optional namelist so the assets can be exported with sane file names.</param>
        public static void ExportAll(this Pack2Reader reader, string outputPath, IEnumerable<string>? nameList = null)
        {
            Dictionary<ulong, string> hashedNamePairs = nameList is null ? new() : PackCrc64.HashStrings(nameList);
            ExportAll(reader, outputPath, hashedNamePairs);
        }

        /// <summary>
        /// Exports assets in a pack, only if their file name is present in the provided name hash list.
        /// </summary>
        /// <param name="reader">The pack reader.</param>
        /// <param name="outputPath">The path to export the assets to.</param>
        /// <param name="hashedNamePairs">A mapping of CRC-64 hashes to the original file name strings.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchonous operation.</returns>
        public static async Task ExportNamedAsync(this Pack2Reader reader, string outputPath, Dictionary<ulong, string> hashedNamePairs, CancellationToken ct = default)
        {
            if (!Directory.Exists(outputPath))
                throw new DirectoryNotFoundException(outputPath);

            IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

            foreach (Asset2Header assetHeader in assetHeaders.Where(h => hashedNamePairs.ContainsKey(h.NameHash)))
            {
                using SafeFileHandle outputHandle = File.OpenHandle(
                    Path.Combine(outputPath, hashedNamePairs[assetHeader.NameHash]),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read,
                    FileOptions.Asynchronous
                );

                ReadOnlyMemory<byte> assetData = await reader.ReadAssetDataAsync(assetHeader, ct).ConfigureAwait(false);
                await RandomAccess.WriteAsync(outputHandle, assetData, 0, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Exports assets in a pack, only if their file name is present in the provided name hash list.
        /// </summary>
        /// <param name="reader">The pack reader.</param>
        /// <param name="outputPath">The path to export the assets to.</param>
        /// <param name="hashedNamePairs">A mapping of CRC-64 hashes to the original file name strings.</param>
        public static void ExportNamed(this Pack2Reader reader, string outputPath, Dictionary<ulong, string> hashedNamePairs)
        {
            if (!Directory.Exists(outputPath))
                throw new DirectoryNotFoundException(outputPath);

            IReadOnlyList<Asset2Header> assetHeaders = reader.ReadAssetHeaders();

            foreach (Asset2Header assetHeader in assetHeaders.Where(h => hashedNamePairs.ContainsKey(h.NameHash)))
            {
                using SafeFileHandle outputHandle = File.OpenHandle(
                    Path.Combine(outputPath, hashedNamePairs[assetHeader.NameHash]),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read,
                    FileOptions.Asynchronous
                );

                ReadOnlySpan<byte> assetData = reader.ReadAssetData(assetHeader);
                RandomAccess.Write(outputHandle, assetData, 0);
            }
        }

        /// <summary>
        /// Exports assets in a pack, only if their file name is present in the provided name list.
        /// </summary>
        /// <param name="reader">The pack reader.</param>
        /// <param name="outputPath">The path to export the assets to.</param>
        /// <param name="nameList">A list of the original file names.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchonous operation.</returns>
        public static async Task ExportNamedAsync(this Pack2Reader reader, string outputPath, IEnumerable<string> nameList, CancellationToken ct = default)
        {
            Dictionary<ulong, string> hashedNamePairs = PackCrc64.HashStrings(nameList);
            await ExportNamedAsync(reader, outputPath, hashedNamePairs, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Exports assets in a pack, only if their file name is present in the provided name list.
        /// </summary>
        /// <param name="reader">The pack reader.</param>
        /// <param name="outputPath">The path to export the assets to.</param>
        /// <param name="nameList">A list of the original file names.</param>
        public static void ExportNamed(this Pack2Reader reader, string outputPath, IEnumerable<string> nameList)
        {
            Dictionary<ulong, string> hashedNamePairs = PackCrc64.HashStrings(nameList);
            ExportNamed(reader, outputPath, hashedNamePairs);
        }
    }
}
