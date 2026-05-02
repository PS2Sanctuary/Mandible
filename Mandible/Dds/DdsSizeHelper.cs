using System;

namespace Mandible.Dds;

public static class DdsSizeHelper
{
    public static int GetBlockSize( DdsPixelFormat format )
    {
        if (format.FourCC == DdsPixelFormat.DXT1.FourCC)
            return 8;

        bool is16Byte = format.FourCC == DdsPixelFormat.DXT2.FourCC
            || format.FourCC == DdsPixelFormat.DXT3.FourCC
            || format.FourCC == DdsPixelFormat.DXT4.FourCC
            || format.FourCC == DdsPixelFormat.DXT5.FourCC;
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (is16Byte)
            return 16;

        throw new ArgumentException("Unsupported pixel format");
    }

    public static int GetBitsPerPixel(DdsPixelFormat format)
    {
        if (format.FourCC == DdsPixelFormat.DXT1.FourCC)
            return 4;

        bool isEightBit = format.FourCC == DdsPixelFormat.DXT2.FourCC
            || format.FourCC == DdsPixelFormat.DXT3.FourCC
            || format.FourCC == DdsPixelFormat.DXT4.FourCC
            || format.FourCC == DdsPixelFormat.DXT5.FourCC;
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (isEightBit)
            return 8;

        throw new ArgumentException("Unsupported pixel format");
    }

    public static uint CalculatePitch
    (
        ushort width,
        ushort height,
        ushort depth,
        DdsPixelFormat format,
        out DdsHeaderFlags pitchFlag
    )
    {
        // We don't bother calculating pitch here, as all of our current supported pixel formats are block-compressed,
        // and although documentation varies, it does seem that the "correct" thing to do for compressed textures is
        // to provide the linear size

        pitchFlag = DdsHeaderFlags.DDS_HEADER_FLAGS_LINEARSIZE;
        return (uint)CalculateLinearSize(width, height, depth, format);
    }

    private static int CalculateLinearSize(ushort width, ushort height, ushort depth, DdsPixelFormat format)
    {
        int val = width * height * GetBitsPerPixel(format) / 8;

        if (depth > 1)
            val *= depth;

        return val;
    }
}
