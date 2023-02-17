namespace Mandible.Pack2;

/// <summary>
/// Defines the zip flags used in asset headers.
/// </summary>
public enum Asset2ZipDefinition : uint
{
    /// <summary>
    /// Indicates that the asset data is not zipped. Alternate representation, no longer used.
    /// </summary>
    UnzippedAlternate = 0x00,

    /// <summary>
    /// Indicates that the asset data is zipped. Alternate representation, no longer used.
    /// </summary>
    ZippedAlternate = 0x01,

    /// <summary>
    /// Indicates that the asset data is not zipped.
    /// </summary>
    Unzipped = 0x10,

    /// <summary>
    /// Indicates that the asset data is zipped.
    /// </summary>
    Zipped = 0x11
}
