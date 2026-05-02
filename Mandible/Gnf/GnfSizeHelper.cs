using System;

namespace Mandible.Gnf;

public static class GnfSizeHelper
{
    /// <summary>
    /// Gets the block size of a GNF texture.
    /// </summary>
    /// <remarks>
    /// This algorithm only works on texels that are 4x4 pixels. This will need revising if we need to support more
    /// image data formats than below.
    /// </remarks>
    /// <param name="header">The header to determine the block size of.</param>
    /// <returns>The number of bytes consumed by a single block.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if the header indicates an unsupported image data format.
    /// </exception>
    public static int GetBlockSize(GnfTextureHeader header)
        => header.DataFormat switch
        {
            GnmImageDataFormat.FORMAT_BC1
                or GnmImageDataFormat.FORMAT_BC4 => 8,
            GnmImageDataFormat.FORMAT_BC2
                or GnmImageDataFormat.FORMAT_BC3
                or GnmImageDataFormat.FORMAT_BC5
                or GnmImageDataFormat.FORMAT_BC6
                or GnmImageDataFormat.FORMAT_BC7 => 16,
            _ => throw new NotSupportedException($"Cannot calculate block size for the data format {header.DataFormat}")
        };
}
