using Mandible.Manifest;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Abstractions.Manifest;

/// <summary>
/// Represents a service for retrieving PlanetSide 2 patch manifest data.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Gets a manifest digest.
    /// </summary>
    /// <param name="manifestUrl">The URL of the manifest.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The deserialized digest.</returns>
    Task<Digest> GetDigestAsync(string manifestUrl, CancellationToken ct = default);

    /// <summary>
    /// Gets a stream containing a manifest file's data.
    /// </summary>
    /// <param name="digest">The digest that the <paramref name="file"/> is from.</param>
    /// <param name="file">The file to retrieve.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A stream containing the file data.</returns>
    Task<Stream> GetFileDataAsync(Digest digest, ManifestFile file, CancellationToken ct = default);
}
