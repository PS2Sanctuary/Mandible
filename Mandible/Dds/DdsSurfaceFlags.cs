namespace Mandible.Dds;

public enum DdsSurfaceFlags : uint
{
    /// <summary>
    /// Required.
    /// </summary>
    DDS_SURFACE_FLAGS_TEXTURE = 0x00001000, // DDSCAPS_TEXTURE

    /// <summary>
    /// Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map,
    /// or mipmapped volume texture).
    /// </summary>
    DDS_SURFACE_FLAGS_CUBEMAP = 0x00000008, // DDSCAPS_COMPLEX

    /// <summary>
    /// Optional; should be used for a mipmap.
    /// </summary>
    DDS_SURFACE_FLAGS_MIPMAP = 0x00400008 // DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
}
