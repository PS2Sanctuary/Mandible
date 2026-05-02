using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Services;
using Mandible.Common;
using Mandible.Dds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Gnf;

/// <summary>
/// Converts GNF images into different formats.
/// </summary>
public static class GnfConverter
{
    /// <summary>
    /// Converts a single texture from a GNF image into a DDS texture file.
    /// </summary>
    /// <param name="gnf">The GNF image data.</param>
    /// <param name="textureIndex">The index of the texture to convert.</param>
    /// <param name="ddsOutput">The sink to write the DDS data into.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    public static async ValueTask ToDds
    (
        GnfImage gnf,
        int textureIndex,
        IDataWriterService ddsOutput,
        CancellationToken ct = default
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(textureIndex, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(textureIndex, gnf.Textures.Count);

        GnfTextureHeader tex = gnf.Textures[textureIndex];

        DdsHeaderFlags headerFlags = DdsHeaderFlags.DDS_HEADER_FLAGS_TEXTURE;
        DdsSurfaceFlags surfaceFlags = DdsSurfaceFlags.DDS_SURFACE_FLAGS_TEXTURE;

        if (tex.MipmapCount > 1)
        {
            headerFlags |= DdsHeaderFlags.DDS_HEADER_FLAGS_MIPMAP;
            surfaceFlags |= DdsSurfaceFlags.DDS_SURFACE_FLAGS_MIPMAP;
        }
        if (tex.Depth > 1)
            headerFlags |= DdsHeaderFlags.DDS_HEADER_FLAGS_VOLUME;

        DdsPixelFormat pixelFormat = tex.DataFormat switch
        {
            GnmImageDataFormat.FORMAT_BC1 => DdsPixelFormat.DXT1,
            GnmImageDataFormat.FORMAT_BC2 => DdsPixelFormat.DXT2,
            GnmImageDataFormat.FORMAT_BC3 => DdsPixelFormat.DXT5,
            _ => throw new NotSupportedException($"The GNM image data format {tex.DataFormat} is not supported.")
        };

        uint pitch = DdsSizeHelper.CalculatePitch
        (
            tex.Width,
            tex.Height,
            tex.Depth,
            pixelFormat,
            out DdsHeaderFlags pitchFlag
        );
        headerFlags |= pitchFlag;

        DdsHeader ddsHeader = new()
        {
            Flags = headerFlags,
            Caps = surfaceFlags,
            Depth = tex.Depth,
            Height = tex.Height,
            Width = tex.Width,
            MipMapCount = (uint)tex.MipmapCount,
            PixelFormat = pixelFormat,
            PitchOrLinearSize = pitch
        };

        // Write the magic
        ddsOutput.Write(FileIdentifiers.Magics[FileType.DdsImage].Span, 0);
        long outputOffset = FileIdentifiers.Magics[FileType.DdsImage].Length;

        // Write the DDS header
        using MemoryOwner<byte> headerBuf = MemoryOwner<byte>.Allocate(DdsHeader.SIZE);
        int headerLen = ddsHeader.Write(headerBuf.Span);
        ddsOutput.Write(headerBuf.Span, outputOffset);
        outputOffset += headerLen;

        // Unswizzle and write each mipmap
        using MemoryOwner<byte> texData = await gnf.ReadTextureData(textureIndex, ct);
        (int StartOffset, int Length)[] mips = GnfMipmapHelper.GetMipmapOffsets(tex);
        for (int mipIndex = 0; mipIndex < mips.Length; mipIndex++)
        {
            (int start, int len) = mips[mipIndex];
            (int mipWidth, int mipHeight) = GnfMipmapHelper.GetMipmapSize2D(tex.Width, tex.Height, mipIndex);

            ReadOnlyMemory<byte> mipSpan = texData.Memory.Slice(start, len);
            using MemoryOwner<byte> unswizzledSpan = MemoryOwner<byte>.Allocate(mipSpan.Length);
            int blockSize = GnfSizeHelper.GetBlockSize(tex);
            Ps4Swizzler.PerformSwizzle(mipSpan.Span, unswizzledSpan.Span, mipWidth, mipHeight, blockSize, true);

            await ddsOutput.WriteAsync(unswizzledSpan.Memory, outputOffset, ct);
            outputOffset += unswizzledSpan.Length;
        }
    }
}
