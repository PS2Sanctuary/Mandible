using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Common;
using Mandible.Pack2.Names;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2;

/// <summary>
/// Contains extension methods for the <see cref="IPack2Reader"/> interface.
/// </summary>
public static class IPack2ReaderExtensions
{
    /// <summary>
    /// Exports each asset in a pack.
    /// </summary>
    /// <param name="reader">The pack reader.</param>
    /// <param name="outputPath">The path to export the assets to.</param>
    /// <param name="namelist">The namelist.</param>
    /// <param name="inferFileExtension">
    /// When an asset could not be resolved to a name, infer the file type using magic data and apply an appropriate
    /// file extension.
    /// </param>
    /// <param name="excludeUnnamed">
    /// Whether to exclude files that are not named in the <paramref name="namelist"/>.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task ExportAllAsync
    (
        this IPack2Reader reader,
        string outputPath,
        Namelist? namelist,
        bool inferFileExtension = true,
        bool excludeUnnamed = false,
        CancellationToken ct = default
    )
    {
        if (!Directory.Exists(outputPath))
            throw new DirectoryNotFoundException(outputPath);

        Debug.Assert
        (
            !(namelist is null && excludeUnnamed),
            "If excluding unnamed files, a name list must be provided"
        );

        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        foreach (Asset2Header assetHeader in assetHeaders)
        {
            string? fileName = null;
            bool? hasName = namelist?.TryGet(assetHeader.NameHash, out fileName);

            if (excludeUnnamed && hasName is false)
                continue;

            await ExportAsync
            (
                reader,
                assetHeader,
                Path.Combine(outputPath, fileName ?? assetHeader.NameHash.ToString()),
                inferFileExtension && fileName is null,
                ct
            ).ConfigureAwait(false);
        }
    }

    private static async Task ExportAsync
    (
        IPack2Reader reader,
        Asset2Header assetHeader,
        string outputPath,
        bool replaceFileExtension,
        CancellationToken ct
    )
    {
        using MemoryOwner<byte> data = await reader.ReadAssetDataAsync(assetHeader, false, ct);

        if (replaceFileExtension)
        {
            FileType discoveredType = FileType.Unknown;
            string? discoveredExt = null;

            foreach ((FileType type, ReadOnlyMemory<byte> magic) in FileIdentifiers.Magics)
            {
                if (data.Span.StartsWith(magic.Span))
                {
                    discoveredType = type;
                    break;
                }
            }

            bool valid = discoveredType is not FileType.Unknown
                && FileIdentifiers.Extensions.TryGetValue(discoveredType, out discoveredExt);
            if (valid)
                outputPath = Path.ChangeExtension(outputPath, discoveredExt);
        }

        using SafeFileHandle outputHandle = File.OpenHandle
        (
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            FileOptions.Asynchronous
        );
        await RandomAccess.WriteAsync(outputHandle, data.Memory, 0, ct);
    }
}
