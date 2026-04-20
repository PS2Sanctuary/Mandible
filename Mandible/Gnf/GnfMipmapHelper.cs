using System;

namespace Mandible.Gnf;

public static class GnfMipmapHelper
{
    /// <summary>
    /// Counts the number of mipmaps that can be generated for a texture of the given dimensions.
    /// </summary>
    /// <param name="baseWidth">The width of the texture.</param>
    /// <param name="baseHeight">The height of the texture.</param>
    /// <returns></returns>
    public static int CalculateMipmapCount(int baseWidth, int baseHeight)
    {
        int count = 1;

        while (baseWidth > 1 || baseHeight > 1)
        {
            if (baseWidth > 1)
                baseWidth >>= 1;
            if (baseHeight > 1)
                baseHeight >>= 1;
            count++;
        }

        return count;
    }

    /// <summary>
    /// Gets the pixel size of a mipmap.
    /// </summary>
    /// <param name="header">The texture header.</param>
    public static (int Width, int Height)[] GetMipmapSizes(GnfTextureHeader header)
    {
        (int Width, int Height)[] ret = new (int Width, int Height)[header.MipmapCount];

        for (int mipLevel = 0; mipLevel < header.MipmapCount; mipLevel++)
        {
            int mipWidth = Math.Max(1, header.Width >> mipLevel);
            int mipHeight = Math.Max(1, header.Height >> mipLevel);
            ret[mipLevel] = (mipWidth, mipHeight);
        }

        return ret;
    }

    /// <summary>
    /// Gets the offset of a mipmap within a block of texture data, accounting for padding. Only works for 2D textures.
    /// </summary>
    /// <param name="header">The texture header.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown if mipmap offsets cannot be calculated for the texture's data format.
    /// </exception>
    public static (int StartOffset, int Length)[] GetMipmapOffsets(GnfTextureHeader header)
    {
        // Note - this algorithm only works on texels that are 4x4 pixels

        (int StartOffset, int Length)[] ret = new (int StartOffset, int Length)[header.MipmapCount];
        int lastOffset = 0;
        // Unsure if adding two is actually "correct" here, or just works by chance
        int alignment = 1 << (header.MipAlignmentShift + 2);

        int blockSize = header.DataFormat switch
        {
            GnmImageDataFormat.FORMAT_BC1
                or GnmImageDataFormat.FORMAT_BC4 => 8,
            GnmImageDataFormat.FORMAT_BC2
                or GnmImageDataFormat.FORMAT_BC3
                or GnmImageDataFormat.FORMAT_BC5
                or GnmImageDataFormat.FORMAT_BC6
                or GnmImageDataFormat.FORMAT_BC7 => 16,
            _ => throw new NotSupportedException($"Mipmap offsets cannot be calculated for the data format {header.DataFormat}")
        };

        for (int mipLevel = 0; mipLevel < header.MipmapCount; mipLevel++)
        {
            int mipWidth = Math.Max(1, header.Width >> mipLevel);
            int mipHeight = Math.Max(1, header.Height >> mipLevel);

            // Adding the (denominator - 1) to the numerator before dividing is the same as Math.Ceiling() given
            // integer rounding
            // Note restriction to 4x4 texels here
            mipWidth = Math.Max(1, (mipWidth + 3) / 4);
            mipHeight = Math.Max(1, (mipHeight + 3) / 4);

            int mipByteLength = mipWidth * mipHeight * blockSize;
            ret[mipLevel] = (lastOffset, mipByteLength);

            lastOffset += mipByteLength;
            // Pad to alignment
            if (lastOffset % alignment is not 0)
                lastOffset += alignment - lastOffset % alignment;
        }

        return ret;
    }
}
