using System;

namespace Mandible.Gnf;

public static class GnfTextureDataHelper
{
    /// <summary>
    /// Gets the offset of a mipmap within a block of texture data.
    /// </summary>
    /// <param name="header">The texture header.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown if mipmap offsets cannot be calculated for the texture's data format.
    /// </exception>
    public static (int StartOffset, int Length)[] GetMipmapOffsets(GnfTextureHeader header)
    {
        (int StartOffset, int Length)[] ret = new (int StartOffset, int Length)[header.MipmapCount];
        int lastOffset = 0;
        int alignment = 1 << header.MipAlignmentShift;

        for (int mipLevel = 0; mipLevel < header.MipmapCount; mipLevel++)
        {
            int mipWidth = Math.Max(1, header.Width >> mipLevel);
            int mipHeight = Math.Max(1, header.Height >> mipLevel);

            int blockSizeX;
            int blockSizeY;

            switch (header.DataFormat)
            {
                case GnmImageDataFormat.FORMAT_BC3:
                    blockSizeX = blockSizeY = 4;
                    break;
                default:
                    throw new NotSupportedException($"Mipmap offsets cannot be calculated for the data format {header.DataFormat}");
            }

            // Adding the (denominator - 1) to the numerator before dividing is the same as Math.Ceiling() given
            // integer rounding
            mipWidth = (mipWidth + (blockSizeX - 1)) / blockSizeX;
            mipHeight = (mipHeight + (blockSizeY - 1)) / blockSizeY;

            int mipByteLength = mipWidth * mipHeight * blockSizeX * blockSizeY;
            ret[mipLevel] = (lastOffset, mipByteLength);

            lastOffset += mipByteLength;
            // Pad to alignment
            if (lastOffset % alignment is not 0)
                lastOffset += alignment - lastOffset % alignment;
        }

        return ret;
    }
}
