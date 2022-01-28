using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack;

/// <summary>
/// Contains extension methods for the <see cref="IPackReader"/> interface.
/// </summary>
public static class PackReaderExtensions
{
    /// <summary>
    /// Exports each asset in a pack.
    /// </summary>
    /// <param name="reader">The pack reader.</param>
    /// <param name="outputPath">The path to export the assets to.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchonous operation.</returns>
    public static async Task ExportAllAsync
    (
        this IPackReader reader,
        string outputPath,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(outputPath))
            throw new DirectoryNotFoundException(outputPath);

        IReadOnlyList<AssetHeader> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        foreach (AssetHeader assetHeader in assetHeaders)
        {
            await ExportAsync
            (
                reader,
                assetHeader,
                Path.Combine(outputPath, assetHeader.Name),
                ct
            ).ConfigureAwait(false);
        }
    }

    private static async Task ExportAsync
    (
        IPackReader reader,
        AssetHeader assetHeader,
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

        using MemoryOwner<byte> data = await reader.ReadAssetDataAsync(assetHeader, ct).ConfigureAwait(false);
        await RandomAccess.WriteAsync(outputHandle, data.Memory, 0, ct).ConfigureAwait(false);
    }
}
