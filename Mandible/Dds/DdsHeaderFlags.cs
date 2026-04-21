namespace Mandible.Dds;

/// <summary>
/// Pre-defined flags for the <see cref="DdsHeader.Flags"/> field.
/// </summary>
public enum DdsHeaderFlags : uint
{
    /// <summary>
    /// Required in every .dds file.
    /// </summary>
    DDS_HEADER_FLAGS_TEXTURE = 0x00001007, // DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT

    /// <summary>
    /// Required in a mipmapped texture.
    /// </summary>
    DDS_HEADER_FLAGS_MIPMAP = 0x00020000, // DDSD_MIPMAPCOUNT

    /// <summary>
    /// Required in a depth texture.
    /// </summary>
    DDS_HEADER_FLAGS_VOLUME = 0x00800000, // DDSD_DEPTH

    /// <summary>
    /// Required when pitch is provided for an uncompressed texture.
    /// </summary>
    DDS_HEADER_FLAGS_PITCH = 0x00000008, // DDSD_PITCH

    /// <summary>
    /// Required when pitch is provided for a compressed texture.
    /// </summary>
    DDS_HEADER_FLAGS_LINEARSIZE = 0x00080000 // DDSD_LINEARSIZE
}
