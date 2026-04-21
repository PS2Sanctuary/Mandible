using System.Runtime.InteropServices;

namespace Mandible.Dds;

/// <summary>
/// Describes a DDS file header.
/// </summary>
/// <remarks>
/// <seealso href="https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-header"/>.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct DdsHeader
{
    public const int SIZE = 124;

    /// <summary>
    /// The size of the structure.
    /// </summary>
    public uint Size = SIZE;

    /// <summary>
    /// Flags to indicate which members contain valid data.
    /// </summary>
    public DdsHeaderFlags Flags;

    /// <summary>
    /// Surface height (in pixels).
    /// </summary>
    public uint Height;

    /// <summary>
    /// Surface width (in pixels).
    /// </summary>
    public uint Width;

    /// <summary>
    /// The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top
    /// level texture for a compressed texture.
    /// </summary>
    /// <remarks>
    /// For information about how to compute the pitch, see the DDS File Layout section of the
    /// <see href="https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide#dds-file-layout">
    /// Programming Guide for DDS.</see>
    /// </remarks>
    public uint PitchOrLinearSize;

    /// <summary>
    /// Depth of a volume texture (in pixels), otherwise unused.
    /// </summary>
    /// <remarks>
    /// Only if <see cref="DdsHeaderFlags.DDS_HEADER_FLAGS_VOLUME"/> is set in <see cref="Flags"/>.
    /// </remarks>
    public uint Depth;

    /// <summary>
    /// Number of mipmap levels, otherwise unused.
    /// </summary>
    /// <remarks>
    /// Only if <see cref="DdsHeaderFlags.DDS_HEADER_FLAGS_MIPMAP"/> is set in <see cref="Flags"/>.
    /// </remarks>
    public uint MipMapCount;

    /// <summary>
    /// Unused.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
    public uint[] Reserved1;

    /// <summary>
    /// The pixel format.
    /// </summary>
    public DdsPixelFormat PixelFormat;

    /// <summary>
    /// Specifies the complexity of the surfaces stored.
    /// </summary>
    public DdsSurfaceFlags Caps;

    /// <summary>
    /// Additional detail about the surfaces stored.
    /// </summary>
    public DdsCubemapFlags Caps2;

    /// <summary>
    /// Unused.
    /// </summary>
    public uint Caps3;

    /// <summary>
    /// Unused.
    /// </summary>
    public uint Caps4;

    /// <summary>
    /// Unused.
    /// </summary>
    public uint Reserved2;

    public DdsHeader()
    {
        Size = SIZE;
        Reserved1 = new uint[11];
    }
}
