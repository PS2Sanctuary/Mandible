using Mandible.Util;
using Mandible.Util.Zlib;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2;

public static class AssetZipHelper
{
    /// <summary>
    /// Gets the indicator placed in front of an asset data block to indicate that it has been compressed.
    /// </summary>
    public const uint AssetCompressionIndicator = 0xA1B2C3D4;

    /// <summary>
    /// Reads the decompressed length from compressed asset data.
    /// </summary>
    /// <param name="data">The raw, compressed asset data.</param>
    /// <returns>The decompressed length.</returns>
    public static int GetDecompressedLength(ReadOnlySpan<byte> data)
        => (int)BinaryPrimitives.ReadUInt32BigEndian(data[4..8]);

    /// <summary>
    /// Checks if the given <paramref name="data"/> starts with the <see cref="AssetCompressionIndicator"/>.
    /// </summary>
    /// <param name="data">The compressed data.</param>
    /// <returns><c>True</c> if the data has the compression indicator, otherwise <c>false</c>.</returns>
    public static bool HasCompressionIndicator(ReadOnlySpan<byte> data)
    {
        if (data.Length < sizeof(uint))
            return false;

        return BinaryPrimitives.ReadUInt32BigEndian(data) == AssetCompressionIndicator;
    }

    /// <summary>
    /// Unzips an asset.
    /// </summary>
    /// <param name="data">The compressed asset data.</param>
    /// <param name="outputBuffer">The output buffer.</param>
    /// <param name="inflater">The inflater to use.</param>
    /// <returns>The length of the decompressed data.</returns>
    /// <exception cref="InvalidDataException">Thrown if the compressed data was not in the expected format.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the output buffer was not long enough.</exception>
    public static int UnzipAssetData(ReadOnlySpan<byte> data, Span<byte> outputBuffer, ZlibInflater inflater)
    {
        ThrowIfNoCompressionIndicator(data);
        int decompressedLength = GetDecompressedLength(data);

        ArgumentOutOfRangeException.ThrowIfLessThan(outputBuffer.Length, decompressedLength, nameof(outputBuffer));
        ulong written = inflater.Inflate(data[8..], outputBuffer);
        inflater.Reset();

        return (int)written;
    }

    /// <summary>
    /// Unzips an asset.
    /// </summary>
    /// <param name="data">The compressed asset data.</param>
    /// <param name="output">The stream to write the decompressed data to.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    /// <returns>The length of the decompressed data.</returns>
    /// <exception cref="InvalidDataException">Thrown if the compressed data was not in the expected format.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the output buffer was not long enough.</exception>
    public static async Task<int> UnzipAssetData
    (
        ReadOnlyMemory<byte> data,
        Stream output,
        CancellationToken ct = default
    )
    {
        ThrowIfNoCompressionIndicator(data.Span);
        int decompressedLength = GetDecompressedLength(data.Span);

        // ReSharper disable twice UseAwaitUsing
        using ReadOnlyMemoryStream rms = new(data[8..]);
        using ZLibStream zs = new(rms, CompressionMode.Decompress, true);
        await zs.CopyToAsync(output, ct);

        return decompressedLength;
    }

    [StackTraceHidden]
    private static void ThrowIfNoCompressionIndicator(ReadOnlySpan<byte> data)
    {
        if (!HasCompressionIndicator(data))
            ThrowNoCompressionIndicator();
    }

    [DoesNotReturn, StackTraceHidden]
    private static void ThrowNoCompressionIndicator()
        => throw new InvalidDataException("The data does not start with the compression indicator");
}
