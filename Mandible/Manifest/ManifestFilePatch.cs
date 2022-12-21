using Mandible.Extensions;
using System;
using System.Xml;

namespace Mandible.Manifest;

/// <summary>
/// Represents a manifest file patch.
/// </summary>
/// <param name="SourceUncompressedSize">The uncompressed size of the patch's source file in bytes.</param>
/// <param name="SourceCrc">The CRC-32 hash of the patch's source file.</param>
/// <param name="SourceTimestamp">The timestamp at which the patch was created from the source file.</param>
/// <param name="PatchCompressedSize">The compressed size of the patch in bytes.</param>
/// <param name="PatchUncompressedSize">The uncompressed size of the patch in bytes.</param>
public record ManifestFilePatch
(
    int SourceUncompressedSize,
    uint SourceCrc,
    DateTimeOffset SourceTimestamp,
    int PatchCompressedSize,
    int PatchUncompressedSize
)
{
    /// <summary>
    /// Deserializes a <see cref="ManifestFilePatch"/> instance from an <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The reader containing the digest XML stream, positioned on the <c>patch</c> node.</param>
    /// <returns>The deserialized <see cref="ManifestFilePatch"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the reader was in an unexpected state.</exception>
    /// <exception cref="FormatException">Thrown if the patch XML is malformed.</exception>
    public static ManifestFilePatch DeserializeFromXml(XmlReader reader)
    {
        if (reader.NodeType is not XmlNodeType.Element)
            throw new InvalidOperationException("Expected the reader to be on a node");

        if (reader.Name != "patch")
            throw new InvalidOperationException("Expected a patch node");

        int sourceUncompressedSize = reader.GetRequiredInt32("sourceUncompressedSize");
        uint sourceCrc = reader.GetRequiredUInt32("sourceCrc");
        DateTimeOffset sourceTimestamp = reader.GetRequiredTimestamp("sourceTimestamp");
        int patchCompressedSize = reader.GetRequiredInt32("patchCompressedSize");
        int patchUncompressedSize = reader.GetRequiredInt32("patchUncompressedSize");

        return new ManifestFilePatch
        (
            sourceUncompressedSize,
            sourceCrc,
            sourceTimestamp,
            patchCompressedSize,
            patchUncompressedSize
        );
    }
}
