namespace Mandible.Dds;

/// <summary>
/// Pre-defined flags for the <see cref="DdsHeader.Caps2"/> field.
/// </summary>
public enum DdsCubemapFlags : uint
{
    /// <summary>
    /// Required for a cube map.
    /// </summary>
    DDS_CUBEMAP = 0x00000200,

    /// <summary>
    /// Required when these surfaces are stored in a cube map.
    /// </summary>
    DDS_CUBEMAP_POSITIVEX = 0x00000600, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX

    /// <summary>
    /// Required when these surfaces are stored in a cube map.
    /// </summary>
    DDS_CUBEMAP_NEGATIVEX = 0x00000a00, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX

    /// <summary>
    /// Required when these surfaces are stored in a cube map.
    /// </summary>
    DDS_CUBEMAP_POSITIVEY = 0x00001200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY

    /// <summary>
    /// Required when these surfaces are stored in a cube map.
    /// </summary>
    DDS_CUBEMAP_NEGATIVEY = 0x00002200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY

    /// <summary>
    /// Required when these surfaces are stored in a cube map.
    /// </summary>
    DDS_CUBEMAP_POSITIVEZ = 0x00004200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ

    /// <summary>
    /// Required when these surfaces are stored in a cube map.
    /// </summary>
    DDS_CUBEMAP_NEGATIVEZ = 0x00008200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ

    /// <summary>
    /// A bitwise-OR of all cubemap surface axis flags, excluding <see cref="DDS_FLAGS_VOLUME"/>.
    /// </summary>
    DDS_CUBEMAP_ALLFACES = DDS_CUBEMAP_POSITIVEX | DDS_CUBEMAP_NEGATIVEX
        | DDS_CUBEMAP_POSITIVEY | DDS_CUBEMAP_NEGATIVEY
        | DDS_CUBEMAP_POSITIVEZ | DDS_CUBEMAP_NEGATIVEZ,

    /// <summary>
    /// Required for a volume texture.
    /// </summary>
    DDS_FLAGS_VOLUME = 0x00200000 // DDSCAPS2_VOLUME
}
