namespace Mandible.Common;

/// <summary>
/// Enumerates various file types.
/// </summary>
public enum FileType
{
    /// <summary>
    /// Unknown file type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// A Forgelight XML file defining an actor.
    /// </summary>
    ActorDefinition,

    /// <summary>
    /// Collision data (cdt / CDTA).
    /// </summary>
    CollisionData,

    /// <summary>
    /// DDS image data.
    /// </summary>
    DdsImage,

    /// <summary>
    /// Files containing information regarding the automated flora and clutter generation for materials.
    /// </summary>
    Eco,

    /// <summary>
    /// Executable and linkable format.
    /// </summary>
    Elf,

    /// <summary>
    /// An FMOD sound bank file, version 5 (FSB).
    /// </summary>
    FmodSoundBank5,

    /// <summary>
    /// Dynamic graphics effects/FX data.
    /// </summary>
    Fxd,

    /// <summary>
    /// Adobe Shockwave files (SWF) which have had their image data stripped.
    /// </summary>
    Gfx,

    /// <summary>
    /// Unknown.
    /// </summary>
    Indr,

    /// <summary>
    /// JPEG image data.
    /// </summary>
    Jpeg,

    /// <summary>
    /// Material information (DMAT).
    /// </summary>
    MaterialInfo,

    /// <summary>
    /// Model information (DMOD).
    /// </summary>
    ModelInfo,

    /// <summary>
    /// Morpheme animation data (MRN).
    /// </summary>
    MorphemeRuntimeNetwork,

    /// <summary>
    /// Morpheme animation data, 64-bit (MRN).
    /// </summary>
    MorphemeRuntimeNetwork64Bit,

    /// <summary>
    /// Forgelight asset pack, version 1.
    /// </summary>
    Pack1,

    /// <summary>
    /// Forgelight asset pack, version 2.
    /// </summary>
    Pack2,

    /// <summary>
    /// PNG image data.
    /// </summary>
    Png,

    /// <summary>
    /// Resource interchange file format. Generally used as a container for audio or video data.
    /// </summary>
    Riff,

    /// <summary>
    /// Terrain chunk data, LOD0.
    /// </summary>
    TerrainChunkLod0,

    /// <summary>
    /// Terrain chunk data, LOD1.
    /// </summary>
    TerrainChunkLod1,

    /// <summary>
    /// Terrain chunk data, LOD2.
    /// </summary>
    TerrainChunkLod2,

    /// <summary>
    /// Terrain chunk data, LOD3.
    /// </summary>
    TerrainChunkLod3,

    /// <summary>
    /// Occlusion / culling data?
    /// </summary>
    Tome,

    /// <summary>
    /// Truevision TGA raster image data (often called TARGA).
    /// </summary>
    TruevisionTga,

    /// <summary>
    /// Occlusion / culling data?
    /// </summary>
    Vnfo,

    /// <summary>
    /// Zone tiling & object information files.
    /// </summary>
    Zone
}
