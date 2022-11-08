using Mandible.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mandible.Manifest;

/// <summary>
/// Represents a manifest digest.
/// </summary>
/// <param name="DigestBuilderVersion">The version of the builder that created the digest.</param>
/// <param name="ProductName">The name of the product that the digest is for.</param>
/// <param name="DefaultServerFolder"></param>
/// <param name="Publisher">The publisher of the digest.</param>
/// <param name="IconPath"></param>
/// <param name="PackageSizeKB">The size in kibibytes of all files in the digest.</param>
/// <param name="FileCount">The number of files in the digest.</param>
/// <param name="LaunchPath"></param>
/// <param name="DefaultLocalFolder"></param>
/// <param name="ShaAssetUrl">The URL to download files from, using their SHA hash.</param>
/// <param name="Timestamp">The time at which the digest was last updated.</param>
/// <param name="CompressionType">The type of compression used on files in the digest.</param>
/// <param name="FallbackHosts">Fallback addresses to use in place of <see cref="ShaAssetUrl"/>.</param>
/// <param name="ExternalDigests">The address of any dependency digests.</param>
/// <param name="Folders">The folders contained in the digest.</param>
public record Digest
(
    int DigestBuilderVersion,
    string ProductName,
    Uri DefaultServerFolder,
    string Publisher,
    string? IconPath,
    int PackageSizeKB,
    int FileCount,
    string LaunchPath,
    string DefaultLocalFolder,
    Uri ShaAssetUrl,
    DateTimeOffset Timestamp,
    string CompressionType,
    IReadOnlyList<string> FallbackHosts,
    IReadOnlyList<Uri> ExternalDigests,
    IReadOnlyList<Folder> Folders
)
{
    /// <summary>
    /// Deserializes a <see cref="Digest"/> instance from an <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The reader containing the digest XML stream.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The deserialized <see cref="Digest"/> instance.</returns>
    /// <exception cref="FormatException">Thrown if the digest XML is malformed.</exception>
    public static async Task<Digest> DeserializeFromXmlAsync(XmlReader reader, CancellationToken ct = default)
    {
        int digestBuilderVersion = 0;
        string productName = string.Empty;
        Uri defaultServerFolder = null!;
        string publisher = string.Empty;
        string? iconPath = null;
        int packageSizeKB = 0;
        int fileCount = 0;
        string launchPath = string.Empty;
        string defaultLocalFolder = string.Empty;
        Uri shaAssetUrl = null!;
        DateTimeOffset timestamp = DateTimeOffset.MinValue;
        string compressionType = string.Empty;
        List<string> fallbackHosts = new();
        List<Uri> externalDigests = new();
        List<Folder> folders = new();
        bool digestElementProcessed = false;

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();

            if (reader.NodeType is not XmlNodeType.Element)
                continue;

            if (reader.Name == "digest")
            {
                if (digestElementProcessed)
                    throw new FormatException("Multiple digest elements present");

                digestBuilderVersion = reader.GetRequiredInt32("digestBuilderVersion");
                productName = reader.GetRequiredAttribute("productName");
                defaultServerFolder = new Uri(reader.GetRequiredAttribute("defaultServerFolder"));
                publisher = reader.GetRequiredAttribute("publisher");
                iconPath = reader.GetAttribute("iconPath");
                packageSizeKB = reader.GetRequiredInt32("packageSizeKB");
                fileCount = reader.GetRequiredInt32("fileCount");
                launchPath = reader.GetRequiredAttribute("launchPath");
                defaultLocalFolder = reader.GetRequiredAttribute("defaultLocalFolder");
                shaAssetUrl = new Uri(reader.GetRequiredAttribute("shaAssetURL"));
                timestamp = reader.GetRequiredTimestamp("timestamp");
                compressionType = reader.GetRequiredAttribute("compressionType");

                digestElementProcessed = true;
            }

            if (reader.Name == "fallback")
                fallbackHosts.Add(reader.GetRequiredAttribute("host"));

            if (reader.Name == "externalDigest")
            {
                string digestUrl = reader.GetRequiredAttribute("url");
                externalDigests.Add(new Uri(digestUrl));
            }

            if (reader.Name == "folder")
            {
                Folder folder = await Folder.DeserializeFromXmlAsync(reader, ct).ConfigureAwait(false);
                folders.Add(folder);
            }
        }

        if (!digestElementProcessed)
            throw new FormatException("No digest element was found");

        return new Digest
        (
            digestBuilderVersion,
            productName,
            defaultServerFolder,
            publisher,
            iconPath,
            packageSizeKB,
            fileCount,
            launchPath,
            defaultLocalFolder,
            shaAssetUrl,
            timestamp,
            compressionType,
            fallbackHosts,
            externalDigests,
            folders
        );
    }
}
