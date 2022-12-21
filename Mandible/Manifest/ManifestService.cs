using Mandible.Abstractions.Manifest;
using Microsoft.Extensions.Logging;
using SevenZip.Compression.LZMA;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mandible.Manifest;

/// <inheritdoc />
public class ManifestService : IManifestService
{
    private readonly ILogger<ManifestService> _logger;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestService"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="client">The HTTP client to use.</param>
    public ManifestService(ILogger<ManifestService> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    /// <inheritdoc />
    public async Task<Digest> GetDigestAsync(string manifestUrl, CancellationToken ct = default)
    {
        // We only support XML manifests
        if (!manifestUrl.EndsWith(".txt"))
            manifestUrl += ".txt";

        await using Stream digestStream = await _client.GetStreamAsync(manifestUrl, ct)
            .ConfigureAwait(false);

        using XmlReader reader = XmlReader.Create
        (
            digestStream,
            new XmlReaderSettings { Async = true }
        );

        return await Digest.DeserializeFromXmlAsync(reader, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Stream> GetFileDataAsync(Digest digest, ManifestFile file, CancellationToken ct = default)
    {
        if (file.Sha is null)
            throw new InvalidOperationException("A file must contain a SHA hash to be downloadable");

        foreach (string host in digest.FallbackHosts)
        {
            try
            {
                UriBuilder downloadUri = new($"{digest.ShaAssetUrl}/{file.Sha[..2]}/{file.Sha[2..5]}/{file.Sha[5..]}")
                {
                    Host = host
                };

                await using Stream manifestData = await _client.GetStreamAsync(downloadUri.Uri, ct).ConfigureAwait(false);
                // We must copy to a MemoryStream so that the data is seekable
                MemoryStream ms = new();

                if (file is { CompressedSize: { }, UncompressedSize: { } } && file.CompressedSize < file.UncompressedSize)
                    LMZADecompress(file.CompressedSize.Value, manifestData, ms);
                else
                    await manifestData.CopyToAsync(ms, ct).ConfigureAwait(false);

                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve manifest file from host {Host}", host);
            }
        }

        return new MemoryStream();
    }

    private static void LMZADecompress(long inputStreamLength, Stream inputStream, Stream outputStream)
    {
        Decoder decoder = new();

        byte[] properties = new byte[5];
        int amountRead = inputStream.Read(properties, 0, 5);
        if (amountRead != 5)
            throw new Exception("Input is too short");

        long outSize = 0;
        for (int i = 0; i < 8; i++)
        {
            int v = inputStream.ReadByte();
            if (v < 0)
                throw new Exception("Input stream is too short");

            outSize |= (long)(byte)v << (8 * i);
            amountRead++;
        }

        decoder.SetDecoderProperties(properties);
        long compressedSize = inputStreamLength - amountRead;
        decoder.Code(inputStream, outputStream, compressedSize, outSize, null!);
    }
}
