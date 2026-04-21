namespace Mandible.Dds;

/// <summary>
/// Pre-defined flags for the <see cref="DdsPixelFormat.Flags"/> field.
/// </summary>
public enum DdsPixelFormatFlags : uint
{
    /// <summary>
    /// Texture contains alpha data; dwRGBAlphaBitMask contains valid data.
    /// </summary>
    DDS_ALPHAPIXELS = 0x00000001, // DDPF_ALPHAPIXELS

    /// <summary>
    /// Used in some older DDS files for alpha channel only uncompressed data (<see cref="DdsPixelFormat.RgbBitCount"/>
    /// contains the alpha channel bitcount; <see cref="DdsPixelFormat.ABitMask"/> contains valid data)
    /// </summary>
    DDS_ALPHA = 0x00000002, // DDPF_ALPHA

    /// <summary>
    /// <see cref="DdsPixelFormat.FourCC"/> contains valid data.
    /// </summary>
    DDS_FOURCC = 0x00000004, // DDPF_FOURCC

    /// <summary>
    /// Texture contains uncompressed RGB data; <see cref="DdsPixelFormat.RgbBitCount"/> and the RGB masks
    /// (<see cref="DdsPixelFormat.RBitMask"/>, <see cref="DdsPixelFormat.GBitMask"/>,
    /// <see cref="DdsPixelFormat.BBitMask"/>) contain valid data.
    /// </summary>
    DDS_RGB = 0x00000040, // DDPF_RGB

    DDS_RGBA = 0x00000041, // DDPF_RGB | DDPF_ALPHAPIXELS

    /// <summary>
    /// Used in some older DDS files for single channel color uncompressed data
    /// (<see cref="DdsPixelFormat.RgbBitCount"/> contains the luminance channel bit count;
    /// <see cref="DdsPixelFormat.RBitMask"/> contains the channel mask). Can be combined with
    /// <see cref="DDS_ALPHAPIXELS"/> for a two channel DDS file.
    /// </summary>
    DDS_LUMINANCE = 0x00020000, // DDPF_LUMINANCE

    DDS_LUMINANCEA = 0x00020001, // DDPF_LUMINANCE | DDPF_ALPHAPIXELS
    DDS_PAL8 = 0x00000020, // DDPF_PALETTEINDEXED8
    DDS_PAL8A = 0x00000021, // DDPF_PALETTEINDEXED8 | DDPF_ALPHAPIXELS
    DDS_BUMPLUMINANCE = 0x00040000, // DDPF_BUMPLUMINANCE
    DDS_BUMPDUDV = 0x00080000, // DDPF_BUMPDUDV
    DDS_BUMPDUDVA = 0x00080001 // DDPF_BUMPDUDV | DDPF_ALPHAPIXELS
}
