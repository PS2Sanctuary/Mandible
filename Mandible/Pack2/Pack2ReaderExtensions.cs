using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Pack2.Names;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.IO;
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
    /// <param name="namelist">The namelist.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchonous operation.</returns>
    public static async Task ExportAllAsync
    (
        this IPack2Reader reader,
        string outputPath,
        Namelist? namelist,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(outputPath))
            throw new DirectoryNotFoundException(outputPath);

        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        foreach (Asset2Header assetHeader in assetHeaders)
        {
            string fileName = namelist?.Get(assetHeader.NameHash) ?? assetHeader.NameHash.ToString();

            await ExportAsync
            (
                reader,
                assetHeader,
                Path.Combine(outputPath, fileName),
                ct
            ).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Exports assets in a pack, only if their file name is present in the provided name hash list.
    /// </summary>
    /// <param name="reader">The pack reader.</param>
    /// <param name="outputPath">The path to export the assets to.</param>
    /// <param name="namelist">The namelist.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchonous operation.</returns>
    public static async Task ExportNamedAsync
    (
        this IPack2Reader reader,
        string outputPath,
        Namelist namelist,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(outputPath))
            throw new DirectoryNotFoundException(outputPath);

        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        foreach (Asset2Header assetHeader in assetHeaders)
        {
            string? fileName = namelist.Get(assetHeader.NameHash);
            if (fileName is null)
                continue;

            await ExportAsync
            (
                reader,
                assetHeader,
                Path.Combine(outputPath, fileName),
                ct
            ).ConfigureAwait(false);
        }
    }

    private static async Task ExportAsync
    (
        IPack2Reader reader,
        Asset2Header assetHeader,
        string outputPath,
        CancellationToken ct
    )
    {
        using SafeFileHandle outputHandle = File.OpenHandle
        (
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            FileOptions.Asynchronous
        );

        int assetLength = await reader.GetAssetLengthAsync(assetHeader, ct).ConfigureAwait(false);
        using MemoryOwner<byte> data = MemoryOwner<byte>.Allocate(assetLength);

        await reader.ReadAssetDataAsync(assetHeader, data.Memory, ct).ConfigureAwait(false);
        await RandomAccess.WriteAsync(outputHandle, data.Memory, 0, ct).ConfigureAwait(false);
    }
}
