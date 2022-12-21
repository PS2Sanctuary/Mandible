using Mandible.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mandible.Manifest;

/// <summary>
/// Represents a manifest file.
/// </summary>
/// <param name="Name">The name of the file.</param>
/// <param name="CompressedSize">The compressed size of the file in bytes.</param>
/// <param name="UncompressedSize">The uncompressed size of the file in bytes.</param>
/// <param name="Crc">A CRC-32 hash of the file contents.</param>
/// <param name="Timestamp">The time at which the file was last updated.</param>
/// <param name="OS">The OS that the file is compatible with.</param>
/// <param name="Sha">A SHA hash that can be used to identify and download the file entry.</param>
/// <param name="Executable">Indicates whether the file should be marked as executable.</param>
/// <param name="Delete">Indicates whether the file should be deleted if previously downloaded.</param>
/// <param name="Patches">Any patches that may be applied to the file.</param>
public record ManifestFile
(
    string Name,
    int? CompressedSize,
    int? UncompressedSize,
    uint? Crc,
    DateTimeOffset? Timestamp,
    string? OS,
    string? Sha,
    bool? Executable,
    bool? Delete,
    IReadOnlyList<ManifestFilePatch> Patches
)
{
    /// <summary>
    /// Deserializes a <see cref="ManifestFile"/> instance from an <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The reader containing the digest XML stream, positioned on the <c>file</c> node.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The deserialized <see cref="ManifestFile"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the reader was in an unexpected state.</exception>
    /// <exception cref="FormatException">Thrown if the file XML is malformed.</exception>
    public static async Task<ManifestFile> DeserializeFromXmlAsync(XmlReader reader, CancellationToken ct = default)
    {
        if (reader.NodeType is not XmlNodeType.Element)
            throw new InvalidOperationException("Expected the reader to be on a node");

        if (reader.Name != "file")
            throw new InvalidOperationException("Expected a file node");

        string? nameAttribute = reader.GetAttribute("name");
        if (nameAttribute is null)
            throw new FormatException("File element must have a name");

        int? compressedSize = reader.GetOptionalInt32("compressedSize");
        int? uncompressedSize = reader.GetOptionalInt32("uncompressedSize");
        uint? crc = reader.GetOptionalUInt32("crc");
        DateTimeOffset? timestamp = reader.GetOptionalTimestamp("timestamp");

        string? os = reader.GetAttribute("os");
        string? sha = reader.GetAttribute("sha");

        bool? executable = reader.GetOptionalBoolean("executable");
        bool? delete = reader.GetOptionalBoolean("delete", "yes");

        List<ManifestFilePatch> patches = new();

        if (!reader.IsEmptyElement)
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                ct.ThrowIfCancellationRequested();

                if (reader.NodeType is XmlNodeType.EndElement && reader.Name == "file")
                    break;

                if (reader.NodeType is not XmlNodeType.Element)
                    continue;

                if (reader.Name == "patch")
                    patches.Add(ManifestFilePatch.DeserializeFromXml(reader));
            }
        }

        return new ManifestFile
        (
            nameAttribute,
            compressedSize,
            uncompressedSize,
            crc,
            timestamp,
            os,
            sha,
            executable,
            delete,
            patches
        );
    }
}
