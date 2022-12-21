using Mandible.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mandible.Manifest;

/// <summary>
/// Represents a manifest folder.
/// </summary>
/// <param name="Name">
/// The name of the folder. This may be null if the contents of the folder should be merged with
/// its parent, or the root.
/// </param>
/// <param name="DownloadPriority">The download priority of the folder.</param>
/// <param name="Children">The children folders.</param>
/// <param name="Files">The files contained in the folder.</param>
public record Folder
(
    string? Name,
    int? DownloadPriority,
    IReadOnlyList<Folder> Children,
    IReadOnlyList<ManifestFile> Files
)
{
    /// <summary>
    /// Deserializes a <see cref="Folder"/> instance from an <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The reader containing the digest XML stream, positioned on the <c>folder</c> node.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The deserialized <see cref="Folder"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the reader was in an unexpected state.</exception>
    public static async Task<Folder> DeserializeFromXmlAsync(XmlReader reader, CancellationToken ct = default)
    {
        if (reader.NodeType is not XmlNodeType.Element)
            throw new InvalidOperationException("Expected the reader to be on a node");

        if (reader.Name != "folder")
            throw new InvalidOperationException("Expected a folder node");

        string? nameAttribute = reader.GetAttribute("name");
        int? downloadPriority = reader.GetOptionalInt32("downloadPriority");

        List<Folder> children = new();
        List<ManifestFile> files = new();

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();

            if (reader.NodeType is XmlNodeType.EndElement)
                break;

            if (reader.NodeType is not XmlNodeType.Element)
                continue;

            if (reader.Name == "folder")
                children.Add(await DeserializeFromXmlAsync(reader, ct).ConfigureAwait(false));
            else if (reader.Name == "file")
                files.Add(await ManifestFile.DeserializeFromXmlAsync(reader, ct).ConfigureAwait(false));
        }

        return new Folder(nameAttribute, downloadPriority, children, files);
    }
}
