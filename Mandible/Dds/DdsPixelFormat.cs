using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mandible.Dds;

/// <summary>
/// Surface pixel format.
/// </summary>
/// <remarks>
/// <seealso href="https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-pixelformat"/>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct DdsPixelFormat
{
    public static readonly DdsPixelFormat DXT1 = FromFourCC("DXT1"u8);
    public static readonly DdsPixelFormat DXT2 = FromFourCC("DXT2"u8);
    public static readonly DdsPixelFormat DXT3 = FromFourCC("DXT3"u8);
    public static readonly DdsPixelFormat DXT4 = FromFourCC("DXT4"u8);
    public static readonly DdsPixelFormat DXT5 = FromFourCC("DXT5"u8);

    /// <summary>
    /// Structure size.
    /// </summary>
    public uint Size;

    /// <summary>
    /// Values which indicate what type of data is in the surface.
    /// </summary>
    public DdsPixelFormatFlags Flags;

    /// <summary>
    /// Four-character codes for specifying compressed or custom formats. Possible values include: DXT1, DXT2, DXT3,
    /// DXT4, or DXT5. A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, and the
    /// dxgiFormat member of that structure indicates the true format. When using a four-character code,
    /// <see cref="Flags"/> must include <see cref="DdsPixelFormatFlags.DDS_FOURCC"/>.
    /// </summary>
    public uint FourCC;

    /// <summary>
    /// Number of bits in an RGB (possibly including alpha) format. Valid when <see cref="Flags"/> includes
    /// <see cref="DdsPixelFormatFlags.DDS_RGB"/>, <see cref="DdsPixelFormatFlags.DDS_LUMINANCE"/>, or DDPF_YUV.
    /// </summary>
    public uint RgbBitCount;

    /// <summary>
    /// Red (or luminance or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would
    /// be 0x00ff0000.
    /// </summary>
    public uint RBitMask;

    /// <summary>
    /// Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be
    /// 0x0000ff00.
    /// </summary>
    public uint GBitMask;

    /// <summary>
    /// Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be
    /// 0x000000ff.
    /// </summary>
    public uint BBitMask;

    /// <summary>
    /// Alpha mask for reading alpha data. <see cref="Flags"/> must include
    /// <see cref="DdsPixelFormatFlags.DDS_ALPHAPIXELS"/> or <see cref="DdsPixelFormatFlags.DDS_ALPHA"/>. For instance,
    /// given the A8R8G8B8 format, the alpha mask would be 0xff000000.
    /// </summary>
    public uint ABitMask;

    private static uint TextToFourCC(ReadOnlySpan<byte> text)
    {
        Debug.Assert(text.Length == 4);
        fixed (byte* ptr = text)
            return *(uint*)ptr;
    }

    private static DdsPixelFormat FromFourCC(ReadOnlySpan<byte> fourCC)
        => new()
        {
            Size = (uint)sizeof(DdsPixelFormat),
            Flags = DdsPixelFormatFlags.DDS_FOURCC,
            FourCC = TextToFourCC(fourCC),
            RgbBitCount = 0,
            RBitMask = 0,
            GBitMask = 0,
            BBitMask = 0,
            ABitMask = 0
        };
}
