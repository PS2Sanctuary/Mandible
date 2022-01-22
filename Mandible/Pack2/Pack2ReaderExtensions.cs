using Mandible.Util;
using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2;

public static class Pack2ReaderExtensions
{
    /// <summary>
    /// Exports each asset in a pack.
    /// </summary>
    /// <param name="reader">The pack reader.</param>
    /// <param name="outputPath">The path to export the assets to.</param>
    /// <param name="hashedNamePairs">
    /// A mapping of CRC-64 hashes to their original file name strings, so the assets can be exported with sane file names.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchonous operation.</returns>
    public static async Task ExportAllAsync
    (
        this Pack2Reader reader,
        string outputPath,
        IReadOnlyDictionary<ulong, string> hashedNamePairs,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(outputPath))
            throw new DirectoryNotFoundException(outputPath);

        Pack2Header header = await reader.ReadHeaderAsync(ct).ConfigureAwait(false);
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(header, ct).ConfigureAwait(false);

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

            (IMemoryOwner<byte> data, int length) = await reader.ReadAssetDataAsync(assetHeader, ct).ConfigureAwait(false);
            await RandomAccess.WriteAsync(outputHandle, data.Memory[..length], 0, ct).ConfigureAwait(false);
            data.Dispose();
        }
    }

    /// <summary>
    /// Exports each asset in a pack.
    /// </summary>
    /// <param name="reader">The pack reader.</param>
    /// <param name="outputPath">The path to export the assets to.</param>
    /// <param name="hashedNamePairs">
    /// A mapping of CRC-64 hashes to their original file name strings, so the assets can be exported with sane file names.
    /// </param>
    public static void ExportAll
    (
        this Pack2Reader reader,
        string outputPath,
        IReadOnlyDictionary<ulong, string> hashedNamePairs
    )
    {
        if (!Directory.Exists(outputPath))
            throw new DirectoryNotFoundException(outputPath);

        foreach (Asset2Header assetHeader in reader.ReadAssetHeaders(reader.ReadHeader()))
        {
            string fileName = hashedNamePairs.ContainsKey(assetHeader.NameHash) ? hashedNamePairs[assetHeader.NameHash] : assetHeader.NameHash.ToString();

            using SafeFileHandle outputHandle = File.OpenHandle(
                Path.Combine(outputPath, fileName),
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                FileOptions.Asynchronous
            );

            (IMemoryOwner<byte> data, int length) = reader.ReadAssetData(assetHeader);
            RandomAccess.Write(outputHandle, data.Memory.Span[..length], 0);
            data.Dispose();
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
    public static async Task ExportAllAsync
    (
        this Pack2Reader reader,
        string outputPath,
        IEnumerable<string>? nameList = null,
        CancellationToken ct = default
    )
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
    public static void ExportAll
    (
        this Pack2Reader reader,
        string outputPath,
        IEnumerable<string>? nameList = null
    )
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
    public static async Task ExportNamedAsync
    (
        this Pack2Reader reader,
        string outputPath,
        IReadOnlyDictionary<ulong, string> hashedNamePairs,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(outputPath))
            throw new DirectoryNotFoundException(outputPath);

        Pack2Header header = await reader.ReadHeaderAsync(ct).ConfigureAwait(false);
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(header, ct).ConfigureAwait(false);

        foreach (Asset2Header assetHeader in assetHeaders.Where(h => hashedNamePairs.ContainsKey(h.NameHash)))
        {
            using SafeFileHandle outputHandle = File.OpenHandle(
                Path.Combine(outputPath, hashedNamePairs[assetHeader.NameHash]),
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                FileOptions.Asynchronous
            );

            (IMemoryOwner<byte> data, int length) = await reader.ReadAssetDataAsync(assetHeader, ct).ConfigureAwait(false);
            await RandomAccess.WriteAsync(outputHandle, data.Memory[..length], 0, ct).ConfigureAwait(false);
            data.Dispose();
        }
    }

    /// <summary>
    /// Exports assets in a pack, only if their file name is present in the provided name hash list.
    /// </summary>
    /// <param name="reader">The pack reader.</param>
    /// <param name="outputPath">The path to export the assets to.</param>
    /// <param name="hashedNamePairs">A mapping of CRC-64 hashes to the original file name strings.</param>
    public static void ExportNamed
    (
        this Pack2Reader reader,
        string outputPath,
        IReadOnlyDictionary<ulong, string> hashedNamePairs
    )
    {
        if (!Directory.Exists(outputPath))
            throw new DirectoryNotFoundException(outputPath);

        IReadOnlyList<Asset2Header> assetHeaders = reader.ReadAssetHeaders(reader.ReadHeader());

        foreach (Asset2Header assetHeader in assetHeaders.Where(h => hashedNamePairs.ContainsKey(h.NameHash)))
        {
            using SafeFileHandle outputHandle = File.OpenHandle(
                Path.Combine(outputPath, hashedNamePairs[assetHeader.NameHash]),
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                FileOptions.Asynchronous
            );

            (IMemoryOwner<byte> data, int length) = reader.ReadAssetData(assetHeader);
            RandomAccess.Write(outputHandle, data.Memory.Span[..length], 0);
            data.Dispose();
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
    public static async Task ExportNamedAsync
    (
        this Pack2Reader reader,
        string outputPath,
        IEnumerable<string> nameList,
        CancellationToken ct = default
    )
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
    public static void ExportNamed
    (
        this Pack2Reader reader,
        string outputPath,
        IEnumerable<string> nameList
    )
    {
        Dictionary<ulong, string> hashedNamePairs = PackCrc64.HashStrings(nameList);
        ExportNamed(reader, outputPath, hashedNamePairs);
    }
}
