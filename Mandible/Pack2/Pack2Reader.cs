using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;
using System.IO.Compression;

namespace Mandible.Pack2
{
    public class Pack2Reader : IDisposable
    {
        /// <summary>
        /// The indicator placed in front of an asset data block to indicate that it has been compressed.
        /// </summary>
        protected const uint ASSET_COMPRESSION_INDICATOR = 2712847316;

        protected readonly SafeFileHandle _packFileHandle;

        protected Pack2Header? _cachedHeader;
        protected IReadOnlyList<Asset2Header>? _cachedAssetHeaders;

        public bool IsDisposed { get; protected set; }

        public Pack2Reader(string packLocation)
        {
            _packFileHandle = File.OpenHandle(packLocation);
        }

        /// <summary>
        /// Reads the pack header. Subsequent calls return a cached value.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>The pack header.</returns>
        public async Task<Pack2Header> ReadHeaderAsync(CancellationToken ct = default)
        {
            if (_cachedHeader is not null)
                return _cachedHeader.Value;

            Memory<byte> headerBuffer = new(new byte[Pack2Header.SIZE]);

            await RandomAccess.ReadAsync(_packFileHandle, headerBuffer, 0, ct).ConfigureAwait(false);
            _cachedHeader = Pack2Header.Deserialise(headerBuffer.Span);

            return _cachedHeader.Value;
        }

        /// <summary>
        /// Gets the asset header list. Subsequent calls return a cached value.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>The list of asset headers.</returns>
        public async Task<IReadOnlyList<Asset2Header>> ReadAssetHeadersAsync(CancellationToken ct = default)
        {
            if (_cachedAssetHeaders is not null)
                return _cachedAssetHeaders;

            List<Asset2Header> assetHeaders = new();
            Pack2Header header = await ReadHeaderAsync(ct).ConfigureAwait(false);
            Memory<byte> headerBuffer = new(new byte[Asset2Header.SIZE * header.AssetCount]);

            await RandomAccess.ReadAsync(_packFileHandle, headerBuffer, (long)header.AssetMapOffset, ct).ConfigureAwait(false);

            for (int i = 0; i < header.AssetCount; i += Asset2Header.SIZE)
            {
                Memory<byte> assetHeader = headerBuffer.Slice(i, i + Asset2Header.SIZE);
                assetHeaders.Add(Asset2Header.Deserialise(assetHeader.Span));
            }

            _cachedAssetHeaders = assetHeaders;
            return _cachedAssetHeaders;
        }

        /// <summary>
        /// Gets a stream to retrieve the unzipped asset data. Note that the asset data is stored and returned in big endian format.
        /// </summary>
        /// <param name="assetHeader">The asset to retrieve.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A stream of the asset data.</returns>
        public async Task<ReadOnlyMemory<byte>> GetAssetStream(Asset2Header assetHeader, CancellationToken ct = default)
        {
            byte[] data;
            using FileStream fs = new(_packFileHandle, FileAccess.Read, 16384, true);

            if (assetHeader.ZipStatus == AssetZipDefinition.Zipped || assetHeader.ZipStatus == AssetZipDefinition.ZippedAlternate)
            {
                Memory<byte> compressionBlock = new(new byte[8]);
                await fs.ReadAsync(compressionBlock, ct).ConfigureAwait(false);

                uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(compressionBlock.Span);
                uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(compressionBlock[4..8].Span);

                if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
                    throw new InvalidDataException("The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data.");

                data = new byte[decompressedLength];
                using DeflateStream dfs = new(fs, CompressionMode.Decompress, true);

                await dfs.ReadAsync(data, ct).ConfigureAwait(false);
            }
            else
            {
                data = new byte[assetHeader.DataSize];
            }

            return data;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _packFileHandle.Dispose();
                }

                _cachedAssetHeaders = null;

                IsDisposed = true;
            }
        }
    }
}
